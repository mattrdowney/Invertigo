using UnityEngine;
using System.Collections;

public class RotationLockedCamera : InvertigoCamera
{
	Transform camera_transform;
	CharacterMotor motor;

	// Use this for initialization
	void Start ()
	{
		GameObject target = GameObject.Find("Player");
		motor = target.GetComponent<CharacterMotor>();

		camera_transform = this.gameObject.GetComponent<Transform>();
	}
	
	// Update is called once per frame
	void LateUpdate ()
	{
		camera_transform.LookAt(motor.current_position, -motor.South);
	}
}