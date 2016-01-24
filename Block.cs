using UnityEngine;
using System.Collections;

public class Block : Component
{
	BlockMotor						motor;

	public Vector3 Evaluate(CharacterMotor charMotor)
	{
		return Vector3.zero;
	}
}