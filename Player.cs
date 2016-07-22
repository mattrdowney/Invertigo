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

	float jump_request = -100;

	void Awake()
	{
		motor = gameObject.GetComponent<CharacterMotor>();
		detector = gameObject.GetComponent<CollisionDetector>();
		state = null;

        motor.radius = 0.05f;
		jump_request = -100;
	}

	void Update()
	{
		if(Input.GetButtonDown("Jump"))
		{
			jump_request = Time.time;
		}
	}

	void FixedUpdate() //HACK: just trying to get this to work
	{
		Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")); //HACK: hardcoded and won't support AI

		motor.Move(input);

		if(!motor.grounded && !motor.between_levels)
		{	
			//Calculate collision information
			optional<ArcOfSphere> arc = detector.ArcCast(motor.current_position, motor.previous_position, motor.radius);

            if (!arc.exists)
            {
                arc = detector.BaloonCast(motor.current_position, motor.radius);
            }

			if(arc.exists)
			{
                optional<Vector3> collision_point = arc.data.Intersect(motor.current_position, motor.previous_position, motor.radius);

                if (collision_point.exists)
                {
                    motor.Traverse(arc.data, collision_point.data);
                }
                else
                {
                    Debug.Log("Didn't collide?");
                }
			}
		}
		else if(motor.grounded && Time.time - jump_request < 0.2f)
		{
			motor.Jump();
			jump_request = -100;
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
