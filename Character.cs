using UnityEngine;
using System.Collections;

[RequireComponent (typeof (CollisionDetector))]
[RequireComponent (typeof (CharacterMotor))]
abstract public class Character : MonoBehaviour
{
	CollisionDetector coll;
	CharacterMotor motor;
	CharacterState state;

	public Vector3   position()
	{
		return this.transform.position;
	}

	public Quaternion rotation()
	{
		return this.transform.rotation;
	}

	public void Move()
	{
		//motor.Move();
	}

	//camera.transparencySortMode = TransparencySortMode.Perspective;
}
