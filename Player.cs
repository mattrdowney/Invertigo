using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterMotor))]
[RequireComponent(typeof(CollisionDetector))]
public class Player : Character
{
	//move into character motor
	//enum PlayerState {FALLING, GROUNDED, MAGNETIZED, GRAPPLING} //TODO: switch state machines to Behaviours

	//move to new class
	//enum GunState {EMPTY, LOADED, LOADING, FIRING /*useful for guns that have detonators*/, SWAP_OUT, SWAP_IN}

	//PlayerState state = PlayerState.GROUNDED;

	//Block ground;

	void Awake()
	{
		motor = gameObject.GetComponent<CharacterMotor>();
		detector = gameObject.GetComponent<CollisionDetector>();
		state = null;

		motor.radius = this.GetComponent<SphereCollider>().radius * this.transform.localScale.x;
	}

	void FixedUpdate() //HACK: just trying to get this to work
	{
		Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")); //HACK: hardcoded and won't support AI

		motor.Move(input);

		if(!motor.grounded)
		{	
			//Calculate collision information
			optional<ArcOfSphere> arc = detector.ArcCast(motor.curPosition, motor.prevPosition, motor.radius);

			motor.Traverse(arc, motor.curPosition);
		}

		//move left/right
		motor.input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis ("Vertical"));

		//figure out position for next frame and move there
		//charMotor.Move(/*AI-Info*/);
	}
	
	public void Move(CharacterMotor charMotor)
	{
		
	}
}
