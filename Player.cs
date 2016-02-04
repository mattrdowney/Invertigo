using UnityEngine;
using System.Collections.Generic;

public class Player : Character
{
	CollisionDetector				detector;
	CharacterMotor					charMotor;

	//move into character motor
	//enum PlayerState {FALLING, GROUNDED, MAGNETIZED, GRAPPLING} //TODO: switch state machines to Behaviours

	//move to new class
	//enum GunState {EMPTY, LOADED, LOADING, FIRING /*useful for guns that have detonators*/, SWAP_OUT, SWAP_IN}

	//PlayerState state = PlayerState.GROUNDED;

	//Block ground;

	void FixedUpdate ()
	{
		//Calculate collision information
		detector.Update(charMotor.curPosition,charMotor.prevPosition);

		//move left/right
		charMotor.input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis ("Vertical"));

		//figure out position for next frame and move there
		charMotor.Move(/*AI-Info*/);
	}
	
	public void Move(CharacterMotor charMotor)
	{
		
	}
}
