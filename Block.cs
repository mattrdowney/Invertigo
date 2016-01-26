using UnityEngine;
using System.Collections;

abstract public class Block : Component
{
	BlockMotor						motor;

	public virtual Vector3 Evaluate(CharacterMotor charMotor)
	{
		return Vector3.zero;
	}
}

//metal
//plastic
//glass
//electrified metal (aka wire)
//corroded metal //requires MonoBehaviour
//tread
//pushable plastic block
//magnetic

//block->metal

//metal->plastic
//metal->corroded metal
//metal->wire
//metal->magnetic (old mud block)

//plastic->tread
//plastic->pushable plastic
//plastic->glass