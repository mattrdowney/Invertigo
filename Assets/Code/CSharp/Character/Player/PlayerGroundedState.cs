using UnityEngine;

public class PlayerGroundedState : CharacterState
{
	override public CharacterState StateMachine(CharacterMotor self)
    {
        GroundedUpdate(self);

        if (JumpingTransitionState(self))
        {
            return new PlayerFallingState();
        }

        return this;
    }
}