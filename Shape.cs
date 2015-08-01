using UnityEngine;
using System.Collections;

abstract public class Shape /*TODO: get rid of this in production builds*/ : MonoBehaviour
{
	BoxCollider coll;

	abstract public bool Contains(ref Vector3 pos);
	
	abstract public Vector3 Evaluate(float t);

	abstract public float Intersect(ref Vector3 oldPos, ref Vector3 newPos);
}
