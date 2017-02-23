using UnityEngine;
using System.Collections;

public class BlockMotor : MonoBehaviour
{
	Transform							transformation;

	public Vector3 RelativePos(Vector3 position)
	{
		position.Normalize(); //redundant-ish
		return transformation.rotation*position;
	}

	public Quaternion RelativeRotation(Quaternion orientation)
	{
		return transformation.rotation * orientation; //XXX: check rhs and lhs
	}
	
	void Start ()
	{
		transformation = this.gameObject.GetComponent<Transform>();
	}

	void Update ()
	{
		//do some stuff
	}
}