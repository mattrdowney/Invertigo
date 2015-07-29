using UnityEngine;
using System.Collections;

public class MetalBlock : Block
{
	public override float Collide(Vector2 pos, Vector2 vel, Vector2 grav, Vector2 normal)
	{
		return 0;
	}
	
	public override Vector2 Evaluate(float t)
	{
		return Vector2.zero;
	}
}
