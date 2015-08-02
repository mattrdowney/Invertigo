using UnityEngine;
using System.Collections;

abstract public class Shape /*TODO: get rid of this in production builds*/ : MonoBehaviour
{
	//BoxCollider coll; ???
	Block parent;
	Shape next;
	Shape prev;

	public Vector3 comPathNormal; //holy shit, spherical triangles are spherical rectangles with one length 0, which simplifies concave collisions significantly
	public float   comPathDist;

	public Vector3 footPathNormal;
	public float   footPathDist;

	abstract public bool Contains(ref Vector3 pos);
	
	abstract public Vector3 Evaluate(float t);

	abstract public float Intersect(ref Vector3 oldPos, ref Vector3 newPos);

	abstract public void AddCollider(BoxCollider coll);
}
