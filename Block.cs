using UnityEngine;
using System.Collections;

public class Block : Component
{
	BlockMotor			motor;
	BlockBehavior		type;

	public float Collide(BoxCollider box, Vector3 pos)
	{
		if(BlockMotor) pos = motor.rotation*pos;


	}
	
	public Vector3 Evaluate(float t)
	{

	}

	public void Optimize()
	{
		for(int i = 0; i < path.Length; ++i)
		{
			//colls = new BoxCollider[n];?
			//path[i].GetInstanceID(); //rearrange box colliders in block so that they are sorted 0 to n-1
		}

		//Recalculate the box collider extents
	}
}
