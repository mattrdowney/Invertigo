using UnityEngine;
using System.Collections;

abstract public class Character : MonoBehaviour
{
	protected CollisionDetector collision_detector;
	protected CharacterMotor character_motor;
	protected CharacterState character_state;

	public Vector3 position()
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
