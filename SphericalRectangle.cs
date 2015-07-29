using UnityEngine;
using System.Collections;

public class SphericalRectangle : Shape
{
	override public bool Contains(ref Vector3 pos)
	{
		return false;
	}
	
	override public Vector3 Evaluate(float t)
	{
		return Vector3.zero;
	}
	
	override public float Intersect(ref Vector3 oldPos, ref Vector3 newPos)
	{
		return 0;
	}
}
