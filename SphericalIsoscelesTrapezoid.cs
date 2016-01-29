//step 1: Character Controller adds the observed SphericalIsoscelesTriangle to a vector in OnCollisionEnter (and removes it in OnCollisionExit)
//step 2: Character Controller asks the block if a collision actually occuring (AABB testing is just for the quick collision detection).
//step 3: if a collision is happening, a list of TTCs (time till collision) are sorted to find the closest collision.
//step 4: the player moves in contact with the object and performs camera transitions accordingly if there was a collision.
//step 5: if no collisions were detected, player moves left, right, or stands still depending on input and block behavior (e.g. treadmills).
//step 6: ...

using UnityEngine;
using System.Collections;

public class SphericalIsoscelesTrapezoid /*TODO: get rid of this in production builds*/ : MonoBehaviour //will become Component
{
	/*TODO: make into [Serializable] const*/
	SphericalIsoscelesTrapezoid		next; //compiler hates uninitialized [Serializable] const
	SphericalIsoscelesTrapezoid		prev;

	Vector3							pathNormal;
	float							comPathDist;
	float							footPathDist; //SIMPLIFY: comPathDist should approximately be footPathDistance + .5 or 1 depending on player's height

	Vector3							arcLeft;
	Vector3							arcUp; //CACHED: equivalent to +/-Vector3.Cross(pathNormal, arcLeft/Right)
	Vector3							arcRight;
	
	float							arcRadius;
	float							arcCutoffAngle;
	
	public void Initialize(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 cutNormal) //v1 -> v2 is the com path, v2 -> v3 is right (?), v3 -> foot path, v4 -> v1 is left (?), eccentricity is the bend in the angle
	{
		//the invariant
		DebugUtility.Assert(Mathf.Approximately(Vector3.Dot(v1 - v2, cutNormal), 0) && Mathf.Approximately(Vector3.Dot(v3 - v4, cutNormal), 0), "SphericalIsoscelesTrapezoid: Initialize: failed assert");

		Vector3 top = v2 - v1, right = v2 - v3, bottom = v3 - v4, left = v1 - v4;

		pathNormal   = cutNormal;
		comPathDist  = Vector3.Dot(v1, cutNormal); //use v1 or v2
		footPathDist = comPathDist - .001f /*FIXME JANK*/; //should be sizes[levels]

		arcLeft  =  Vector3.Cross(pathNormal, left);
		arcUp    = -Vector3.Cross(pathNormal, arcLeft);
		arcRight = -Vector3.Cross(pathNormal, right);

		Vector3 pathCenter = comPathDist*pathNormal;

		arcRadius = (v1 - pathCenter).magnitude; //use v1 or v2

		float angle = Vector3.Angle(v1 - pathCenter, v2 - pathCenter);
		if(Vector3.Dot(arcUp, arcRight) < 0) angle += 180f;

		arcCutoffAngle = angle;
	}

	/**
     *  Determine if the character (represented by a point) is inside of a trapezoid (extruded by the radius of the player)
	 */
	public bool Contains(CharacterMotor charMotor)
	{
		Vector3 pos = charMotor.curPosition;

		float prod = Vector3.Dot(pos, pathNormal);

		return (footPathDist <= prod && prod <= comPathDist) && !(Vector3.Dot(pos, arcLeft) > 0 && Vector3.Dot(pos, arcRight) > 0); //XXX: might still be wrong //FIXME: De Morgan
	}

	/**
	 *  return the position of the player based on the circular path
	 *  If the player would go outside of [0, arcCutoffAngle*arcRadius], the Trapezoid should transfer control of the player to (prev, next) respectively
	 */
	public Vector3 Evaluate(CharacterMotor charMotor) //evaluate and Intersect can be combined (?), just add a locked boolean and only swap blocks if the intersected entity is closer
	{
		DebugUtility.Assert(charMotor.t.HasValue && charMotor.segment.HasValue && charMotor.block.HasValue, "SphericalIsoscelesTrapezoid: Evaluate(CharacterMotor): Assert failed");

		if(charMotor.t.Value > arcCutoffAngle*arcRadius)
		{
			charMotor.t.Value -= this.arcCutoffAngle*this.arcRadius;
			return prev.Evaluate(charMotor);
		}
		if(charMotor.t.Value < 0)
		{
			charMotor.t.Value += prev.arcCutoffAngle*prev.arcRadius;
			return prev.Evaluate(charMotor);
		}
		
		return Evaluate(charMotor.t.Value);
	}

	public Vector3 EvaluateNormal(CharacterMotor charMotor)
	{
		return -Vector3.Cross(charMotor.curPosition, charMotor.right); //TODO: check
	}

	public Vector3 FindGravity(CharacterMotor charMotor)
	{
		Vector3 pos = charMotor.curPosition;
		float xz = Mathf.Sqrt(pos.x*pos.x + pos.z*pos.z);
		float xfactor = pos.x / (pos.x + pos.z);
		float zfactor = 1f - xfactor;
		return new Vector3(xfactor/pos.y, -1/xz, zfactor*pos.y); //TODO: check
	}

	public Vector3 Evaluate(float t)
	{
		float angle = t/arcRadius;
		return -arcLeft*arcRadius*Mathf.Cos(angle) + arcUp*arcRadius*Mathf.Sin(angle) + pathNormal*comPathDist; //-arcLeft for "right" is intentional
	}

	/**
	 *  Find the point of collision as a parameterization of a circle.
	 */
	public Optional<float> Intersect(CharacterMotor charMotor)
	{
		Vector3 right  = Vector3.Cross(charMotor.prevPosition, charMotor.curPosition);
		Vector3 secant = Vector3.Cross(pathNormal, right);
		
		if(Vector3.Dot(secant, charMotor.prevPosition) < 0f) secant *= -1;
		
		Vector3 intersection = pathNormal*comPathDist + secant*arcRadius;
		
		float x = Vector3.Dot(intersection, arcRight);
		float y = Vector3.Dot(intersection, arcUp);
		
		float angle = Mathf.Atan2(y,x);
		if(angle < 0) angle += 2*Mathf.PI;

		if(angle <= arcCutoffAngle) return angle*arcRadius;
		return new Optional<float>();
	}

	public Optional<float> Distance(CharacterMotor charMotor)
	{
		Optional<float> intersection = Intersect(charMotor);
		if(intersection.HasValue)
		{
			Vector3 newPos = Evaluate(intersection.Value);
			return Vector3.Distance(charMotor.prevPosition, newPos);
		}

		return new Optional<float>();
	}
	
	private void OnDrawGizmos() //TODO: get rid of this in production builds
	{
		UnityEditor.Handles.color = Color.red;
		Vector3 center = pathNormal*comPathDist;
		Vector3 from = center + arcRight*arcRadius;
		UnityEditor.Handles.DrawWireArc(center, pathNormal, from, arcCutoffAngle, arcRadius);
	}
}