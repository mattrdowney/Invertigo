using UnityEngine;
using System.Collections;

public class SphereUtility
{
	public static Vector3 Position(Vector3 x_axis, Vector3 y_axis, Vector3 z_axis, float phi, float theta)
	{
		Vector3 x = x_axis * Mathf.Sin(phi) * Mathf.Cos(theta);
		Vector3 y = y_axis * Mathf.Sin(phi) * Mathf.Sin(theta);
		Vector3 z = z_axis * Mathf.Cos(phi);
		return x + y + z;
	}
}
