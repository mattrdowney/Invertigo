/** The primative spherical geometry component that is used to traverse a block or terrain
 *
 * TODO: detailed description
 * 
 * @file
 */

using UnityEngine;
using System.Collections;
using System.Linq;

public class SphericalIsoscelesTrapezoid /*TODO: get rid of this in production builds*/ : MonoBehaviour //will become Component
{
	/*TODO: make into [Serializable] const*/
	public SphericalIsoscelesTrapezoid		next; //compiler hates uninitialized [Serializable] const
	public SphericalIsoscelesTrapezoid		prev;

	Vector3							pathNormal;
	float							comPathDist;
	float							footPathDist;

	Vector3							arcLeft;
	Vector3							arcUp;
	Vector3							arcRight;

	float							arcRadius;
	float							arcCutoffAngle;

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

	/**
	 *  return the position of the player based on the circular path
	 */
	public Vector3 Evaluate(float t)
	{
		float angle = t / arcRadius;
		Vector3 center = pathNormal*comPathDist;
		Vector3 x = -arcLeft*arcRadius*Mathf.Cos(angle); //-arcLeft for "right" is intentional
		Vector3 y =  arcUp  *arcRadius*Mathf.Sin(angle);
		return x + y + center; 
	}

	/**
	 *  return the position of the player based on the circular path
	 *  If the player would go outside of [0, arcCutoffAngle*arcRadius],
	 *  the Trapezoid should transfer control of the player to (prev, next) respectively
	 */
	public static Vector3 Evaluate(ref float t, ref SphericalIsoscelesTrapezoid seg)
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

	public Vector3 EvaluateNormal(Vector3 pos, Vector3 right)
	{
		return Vector3.Cross(right, pos);
	}

	public Vector3 EvaluateRight(float t)
	{
		float angle = t / arcRadius;
		return arcUp*Mathf.Cos(angle) + arcLeft*Mathf.Sin(angle);
	}

	/** Recompute the orientation of a SphericalIsoscelesTrapezoid
	 * 
	 *  Destroys all information other than prev, next. Replaces this information with the information for traversing
	 *      the top of a SphericalIsoscelesTrapezoid on a unit sphere.
	 * 
	 * @param left_edge: the left-bottom point (left implies it is the 1st point when enumerated clockwise for concave objects,
	 * 		  bottom implies it is the position of the player's feet)
	 * @param right_edge: the right-bottom point (right implies it is the 2nd point when enumerated clockwise for concave objects,
	 * 		  bottom implies it is the position of the player's feet)
	 * @param normal: the normal plane that intersects lhs and rhs and forms the walking path for the players center
	 * 		  of mass, sign matters because it indicates which direction is up for calculating the center of mass.
	 * 
	 * @example Initialize(Vector3(0,0,1),Vector3(1,0,0), Vector3(0,1,0)) will initialize a Spherical Isosceles Trapezoid
	 *          that is a great circle for the feet positions, a large lesser circle for the center of mass position,
	 *          with a 90 degree arc going from forwards to right and a normal going in the positive y-direction.
	 */

	public void Initialize(Vector3 left_edge, Vector3 right_edge, Vector3 normal)
	{
		//DebugUtility.Assert(Mathf.Approximately(Vector3.Dot(right_edge - left_edge, normal), 0),
		//                    "SphericalIsoscelesTrapezoid: Initialize: failed assert");

		Vector3 v3 = new Vector3(1,0,0); //FIXME:
		Vector3 v4 = new Vector3(1,0,0); //FIXME:

		// probably unnecessary
		Vector3 top = right_edge - left_edge, right = right_edge - v3, bottom = v3 - v4, left = left_edge - v4;
		
		pathNormal   = normal;
		comPathDist  = Vector3.Dot(left_edge, normal); //use lhs or rhs
		footPathDist = comPathDist - .01f;//LevelData.Instance.playerRadius; /*FIXME JANK*/; //should be sizes[levels]
		
		arcLeft  =  Vector3.Cross(pathNormal, left);
		arcUp    = -Vector3.Cross(pathNormal, arcLeft);
		arcRight = -Vector3.Cross(pathNormal, right);
		
		Vector3 pathCenter = comPathDist*pathNormal;
		
		arcRadius = (left_edge - pathCenter).magnitude; //use lhs or rhs
		
		float angle = Vector3.Angle(left_edge - pathCenter, right_edge - pathCenter);

		if(Vector3.Dot(arcUp, arcRight) < 0)
		{
			angle += 180f;
		}
		
		arcCutoffAngle = angle;
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

	private void OnDrawGizmos() //TODO: get rid of this in production builds
	{
		UnityEditor.Handles.color = Color.red;
		Vector3 center = pathNormal*comPathDist;
		Vector3 from = center + -arcLeft*arcRadius;
		UnityEditor.Handles.DrawWireArc(center, pathNormal, from, -arcCutoffAngle, arcRadius); //seems to draw ccw
	}

	/** Create a AABB that perfectly contains a circular arc
	 * 
	 *  TODO: detailed description and math link
	 * 
	 *  TODO: Ex. 
	 * 
	 *  @param collider the box collider that will be altered to contain the SphericalIsoscelesTrapezoid
	 */
	public void RecalculateAABB(BoxCollider collider)
	{

	}

	/** Counts the number of booleans that are true in a comma separated list of booleans
	 * 
	 *  credit: http://stackoverflow.com/questions/377990/elegantly-determine-if-more-than-one-boolean-is-true
	 */
	public static int Truth(params bool[] booleans)
	{
		return booleans.Count(b => b);
	}
}