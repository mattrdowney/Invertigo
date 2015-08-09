using UnityEngine;
using System.Collections;

public class Block : Component
{
	BlockMotor							motor;
	BlockBehavior						type;

	public float Collide(SphericalIsoscelesTrapezoid trap, CharacterMotor charMotor)
	{
		if(BlockMotor) pos = motor.rotation*pos;


	}
	
	public Vector3 Evaluate(float t)
	{

	}
}
