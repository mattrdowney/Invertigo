using UnityEngine;
using System.Collections;

using UnityEditor; //TODO: get rid of this in production builds

public class SphericalTriangle : Shape //originally this code did not derive from monobehavior, added extra data (for ease of use and quicker code execution), and used unserializable planes which are themselves pointers to structs of structs (Plane -> Vector3 -> float)
{
	public Plane arcPlane;
	public Plane bisector;
	public Plane trisector;

	public Vector3 arcRight;
	public Vector3 arcUp;

	public float arcRadius;
	public float arcCutoffAngle;

	bool Beyond(ref Vector3 pos, ref Plane p)
	{
		return Vector3.Dot(p.normal, pos) >= p.distance;
	}

	override public bool Contains(ref Vector3 pos)
	{
		if(Beyond(ref pos, ref arcPlane) && (Beyond(ref pos, ref bisector) || Beyond(ref pos, ref trisector)))
			return true;

		return false;
	}

	override public Vector3 Evaluate(float t)
	{
		//if(t > arcCutoffAngle*arcRadius) return next.Evaluate(t - this.arcCutoffAngle*this.arcRadius);
		//if(t < 0)						 return prev.Evaluate(t + prev.arcCutoffAngle*prev.arcRadius);

		float angle = t/arcRadius;
		return arcRight*arcRadius*Mathf.Cos(angle) + arcUp*arcRadius*Mathf.Sin(angle) + arcPlane.normal*arcPlane.distance;
	}

	override public float Intersect(ref Vector3 oldPos, ref Vector3 newPos)
	{
		Vector3 right = Vector3.Cross(oldPos, newPos);
		Vector3 secant = Vector3.Cross(arcPlane.normal, right);

		if(Vector3.Dot(secant, oldPos) < 0f) secant *= -1;
		Vector3 intersection = arcPlane.normal*arcPlane.distance + secant*arcRadius;

		float x = Vector3.Dot(intersection, arcRight);
		float y = Vector3.Dot(intersection, arcUp);

		float angle = Mathf.Atan2(y,x);
		angle = (angle > 0 ? angle : (2*Mathf.PI + angle));

		return angle > arcCutoffAngle ? -1 : angle*arcRadius;
	}

	private void OnDrawGizmos() //TODO: get rid of this in production builds
	{
		UnityEditor.Handles.color = Color.red;
		Vector3 center = arcPlane.normal*arcPlane.distance;
		Vector3 from = center + arcRight*arcRadius;
		UnityEditor.Handles.DrawWireArc(center, arcPlane.normal, from, arcCutoffAngle, arcRadius);
	}
}