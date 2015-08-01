using UnityEngine;
using System.Collections;

using UnityEditor; //TODO: get rid of this in production builds

public class SphericalTriangle : Shape //originally this code did not derive from monobehavior, added extra data (for ease of use and quicker code execution), and used unserializable planes which are themselves pointers to structs of structs (Plane -> Vector3 -> float)
{
	public Vector3 pathNormal;
	public float   pathDist;

	public Vector3 arcLeft;
	public Vector3 arcUp;
	public Vector3 arcRight;

	public float arcRadius;
	public float arcCutoffAngle;

//	public SphericalTriangle(Vector3 v1, Vector3 v2, Vector3 v3, float eccentricity /*1,2,3?*/) //v1 -> v2 is the path, v2 -> v3 is right (?), v3 -> v1 is left (?), eccentricity is the bend in the angle
//	{
//
//	}

	override public bool Contains(ref Vector3 pos) //FIXME: might contain error in or statement regarding >/<
	{
		if(Vector3.Dot(pos, pathNormal) >= pathDist && 
		  (Vector3.Dot(pos, arcLeft) <= 0 || Vector3.Dot(pos, arcRight) <= 0) ) //I feel like this is the solution, or at least close
			return true;

		return false;
	}

	override public Vector3 Evaluate(float t)
	{
		//if(t > arcCutoffAngle*arcRadius) return next.Evaluate(t - this.arcCutoffAngle*this.arcRadius);
		//if(t < 0)						 return prev.Evaluate(t + prev.arcCutoffAngle*prev.arcRadius);
	
		float angle = t/arcRadius;
		return arcRight*arcRadius*Mathf.Cos(angle) + arcUp*arcRadius*Mathf.Sin(angle) + pathNormal*pathDist;
	}

	override public float Intersect(ref Vector3 oldPos, ref Vector3 newPos)
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