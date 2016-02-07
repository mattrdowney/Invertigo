using UnityEngine;
using System.Collections;
using System.Linq;

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
		footPathDist = comPathDist - LevelData.Instance.playerRadius; /*FIXME JANK*/; //should be sizes[levels]

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
	public bool Contains(Vector3 pos)
	{
		float prod = Vector3.Dot(pos, pathNormal);

		bool bIsAtCorrectElevation = footPathDist <= prod && prod <= comPathDist;
		bool bLeftContains		   = Vector3.Dot(pos, arcLeft) <= 0;
		bool bRightContains		   = Vector3.Dot(pos, arcRight) <= 0;
		bool bIsObtuse			   = Vector3.Dot(arcLeft, arcRight) < 0;
		int  nOutOfThree		   = Truth(bLeftContains, bRightContains, bIsObtuse);

		return bIsAtCorrectElevation && nOutOfThree >= 2; //XXX: might even now be wrong
	}

	public static int Truth(params bool[] booleans) //all credit: http://stackoverflow.com/questions/377990/elegantly-determine-if-more-than-one-boolean-is-true
	{
		return booleans.Count(b => b);
	}

	/**
	 *  return the position of the player based on the circular path
	 *  If the player would go outside of [0, arcCutoffAngle*arcRadius], the Trapezoid should transfer control of the player to (prev, next) respectively
	 */
	public static Vector3 Evaluate(ref float t, ref SphericalIsoscelesTrapezoid seg) //evaluate and Intersect can be combined (?), just add a locked boolean and only swap blocks if the intersected entity is closer
	{
		if(t > seg.arcCutoffAngle*seg.arcRadius)
		{
			t -= seg.arcCutoffAngle*seg.arcRadius;
			seg = seg.next;
			return Evaluate(ref t, ref seg);
		}
		if(t < 0)
		{
			t += seg.prev.arcCutoffAngle*seg.prev.arcRadius;
			seg = seg.prev;
			return Evaluate(ref t, ref seg);
		}
		
		return seg.Evaluate(t);
	}

	public Vector3 Evaluate(float t) //evaluate and Intersect can be combined (?), just add a locked boolean and only swap blocks if the intersected entity is closer
	{
		float angle = t / arcRadius;
		return -arcLeft*arcRadius*Mathf.Cos(angle) + arcUp*arcRadius*Mathf.Sin(angle) + pathNormal*comPathDist; //-arcLeft for "right" is intentional
	}

	public Vector3 EvaluateRight(float t)
	{
		float angle = t / arcRadius;
		return arcUp*Mathf.Cos(angle) + arcLeft*Mathf.Sin(angle);
	}

	public Vector3 EvaluateNormal(Vector3 pos, Vector3 right)
	{
		return Vector3.Cross(right, pos);
	}

	/**
	 *  Find the point of collision as a parameterization of a circle.
	 */
	public Optional<float> Intersect(Vector3 to, Vector3 from) //TODO: FIXME: UNJANKIFY
	{
		Vector3 right  = Vector3.Cross(from, to);
		Vector3 secant = Vector3.Cross(pathNormal, right);
		
		if(Vector3.Dot(secant, from) < 0f) secant *= -1; //TODO: check

		secant.Normalize();
		
		Vector3 intersection = pathNormal*comPathDist + secant*arcRadius;

		float x = Vector3.Dot(intersection, -arcLeft) / arcRadius;
		float y = Vector3.Dot(intersection, arcUp) / arcRadius;
		
		float angle = Mathf.Atan2(y,x);

		if(angle < 0)
		{
			angle += 2*Mathf.PI;
		}

		if(angle <= arcCutoffAngle)
		{
			return angle*arcRadius;
		}
		return new Optional<float>();
	}

	public Optional<float> Distance(Vector3 to, Vector3 from)
	{
		Optional<float> intersection = Intersect(to, from);

		if(intersection.HasValue)
		{
			float t = intersection.Value;
			Vector3 newPos = Evaluate(t);
			return Vector3.Distance(from, newPos);
		}

		return new Optional<float>();
	}
	
	private void OnDrawGizmos() //TODO: get rid of this in production builds
	{
		UnityEditor.Handles.color = Color.red;
		Vector3 center = pathNormal*comPathDist;
		Vector3 from = center + -arcLeft*arcRadius;
		UnityEditor.Handles.DrawWireArc(center, pathNormal, from, arcCutoffAngle, arcRadius);
	}
}