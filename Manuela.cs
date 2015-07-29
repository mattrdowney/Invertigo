using UnityEngine;
using System.Collections;

public class Manuela : MonoBehaviour
{
	enum PlayerState {FALLING, GROUNDED, MAGNETIZED, GRAPPLING}

	PlayerState state = PlayerState.GROUNDED;

	Block ground;

	void Start ()
	{
		//raycast down to find the ground
	}

	void Update ()
	{

	}
}
