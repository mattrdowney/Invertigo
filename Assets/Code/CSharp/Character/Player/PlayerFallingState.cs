using UnityEngine;

public class PlayerFallingState : CharacterState
{
	override public CharacterState StateMachine(CharacterMotor self)
    {
        FallingUpdate(self);

        if (LandingTransitionState(self))
        {
            return new PlayerGroundedState();
        }

        return this;
    }
}
