using UnityEngine;
using System.Collections;

public class SphericalRectangle /*TODO: get rid of this in production builds*/ : MonoBehaviour //will become Component
{
	Block					parent;

	SphericalRectangle		next;
	SphericalRectangle		prev;

	Vector3					comPathNormal; //I wish I could make these const, but I suppose that would make in-editor tools for manipulating the rectangle useless (Now that I think about it, though, the values can probably be set in the editor despite being const)
	float					comPathDist;

	Vector3					footPathNormal;
	float					footPathDist;

	Vector3					arcLeft;
	Vector3					arcUp;
	Vector3					arcRight;
	
	float					arcRadius;
	float					arcCutoffAngle;
	
	public void Initialize(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float eccentricity /*1,2,3?*/) //v1 -> v2 is the com path, v2 -> v3 is right (?), v3 -> foot path, v4 -> v1 is left (?), eccentricity is the bend in the angle
	{

	}
	
	public bool Contains(ref Vector3 pos) //FIXME: might contain error in or statement regarding >/<
	{
		if( ( Vector3.Dot(pos, comPathNormal) >= comPathDist   &&   Vector3.Dot(pos, footPathNormal) <= footPathDist )
		    ( Vector3.Dot(pos, arcLeft)		  <= 0			   ||   Vector3.Dot(pos, arcRight)		 <= 0            ) ) //I feel like this is the solution, or at least close
			return true;
		
		return false;
	}
	
	public Vector3 Evaluate(CharacterMotor motor) //evaluate and Intersect can be combined (?), just add a locked boolean and only swap blocks if the intersected entity is closer
	{
		//if(t > arcCutoffAngle*arcRadius) return next.Evaluate(t - this.arcCutoffAngle*this.arcRadius);
		//if(t < 0)						 return prev.Evaluate(t + prev.arcCutoffAngle*prev.arcRadius);
		
		float angle = t/arcRadius;
		return arcRight*arcRadius*Mathf.Cos(angle) + arcUp*arcRadius*Mathf.Sin(angle) + pathNormal*pathDist;
	}
	
	public float Intersect(ref Vector3 oldPos, ref Vector3 newPos)
	{
		Vector3 right = Vector3.Cross(oldPos, newPos);
		Vector3 secant = Vector3.Cross(pathNormal, right);
		
		if(Vector3.Dot(secant, oldPos) < 0f) secant *= -1;
		
		Vector3 intersection = pathNormal*pathDist + secant*arcRadius;
		
		float x = Vector3.Dot(intersection, arcRight);
		float y = Vector3.Dot(intersection, arcUp);
		
		float angle = Mathf.Atan2(y,x);
		angle = (angle > 0 ? angle : (2*Mathf.PI + angle));
		
		return angle > arcCutoffAngle ? -1 : angle*arcRadius;
	}
	
	private void OnDrawGizmos() //TODO: get rid of this in production builds
	{
		UnityEditor.Handles.color = Color.red;
		Vector3 center = pathNormal*pathDist;
		Vector3 from = center + arcRight*arcRadius;
		UnityEditor.Handles.DrawWireArc(center, pathNormal, from, arcCutoffAngle, arcRadius);
	}
}
