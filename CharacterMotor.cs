using UnityEngine;
using System.Collections;
using System.Collections.Generic;

abstract public class CharacterMotor : Component
{

	public Character								self;
	public Optional<SphericalIsoscelesTrapezoid>	segment;
	public Optional<Block>							block;

	Vector3											_curPosition;
	Vector3											_prevPosition;

	public Optional<float>							t;

	Dictionary<string, float>						flags; //XXX: Strategy Pattern?

	Vector2											_input;

	//TODO: make flags into delegater Queue<*fun()> https://social.msdn.microsoft.com/Forums/en-US/2c08a0d0-58e4-4df6-b6d3-75e785fff8a8/array-of-function-pointers?forum=csharplanguage

	public Vector3 curPosition
	{
		get { return _curPosition; }
		set { _prevPosition = _curPosition; _curPosition = value; }
	}

	public Vector3 prevPosition
	{
		get { return _prevPosition; }
	}

	public Vector2 input
	{
		get { return _input; }
		set { _input = value; if(value.SqrMagnitude() > 1) _input = value.normalized; }
	}

	abstract public void Move();
}
