using UnityEngine;
using System.Collections;

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
}
