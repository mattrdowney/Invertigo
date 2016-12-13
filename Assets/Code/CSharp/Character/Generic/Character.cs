using UnityEngine;
using System.Collections;

abstract public class Character : MonoBehaviour
{
	protected CollisionDetector detector;
	protected CharacterMotor motor;
	protected CharacterState state;

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
