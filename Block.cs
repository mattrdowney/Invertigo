using UnityEngine;
using System.Collections;

public abstract class Block : Component
{
	ArrayList<Shape> path;

	public abstract float Collide(Vector2 pos, Vector2 vel, Vector2 grav, Vector2 normal);
	
	public abstract Vector2 Evaluate(float t);
}
