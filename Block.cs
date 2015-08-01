using UnityEngine;
using System.Collections.Generic;

public abstract class Block : Component
{
	Shape[] path;
	BoxCollider[] colls;

	Dictionary<BoxCollider, int> nonoptimal; //sorted dictionary -> map, dictionary -> unordered_map

	public abstract float Collide(Vector2 pos, Vector2 vel, Vector2 grav, Vector2 normal);
	
	public abstract Vector2 Evaluate(float t);	
}
