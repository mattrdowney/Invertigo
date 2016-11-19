using UnityEngine;
using System.Collections;

abstract public class CharacterState : Behaviour
{
	abstract public void Update(Character self);
}
