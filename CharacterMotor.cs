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

	Vector2											_velocity;
	Vector3											_south;
	Vector3											_west;
	Optional<Vector3>								_normal;
	Optional<Vector3>								_right;

	public Optional<float>							t;

	Dictionary<string, float>						flags; //XXX: Strategy Pattern?

	Vector2											_input;

	//TODO: make flags into delegater Queue<*fun()> https://social.msdn.microsoft.com/Forums/en-US/2c08a0d0-58e4-4df6-b6d3-75e785fff8a8/array-of-function-pointers?forum=csharplanguage

	//ideally there should be three setters: position, velocity, and segment

	public Vector3 curPosition
	{
		get
		{
			return _curPosition;
		}
		set //set must contain extra logic for gravity, normal, and right
		{
			_prevPosition = _curPosition;
			_curPosition = value;
		}
	}

	public Vector3 prevPosition
	{
		get { return _prevPosition; }
	}

	/** Horizontal (x) velocity is the distance travelled along the circumference of the intersection of the unit sphere and an xz plane.
	 *  Vertical   (y) velocity is the distance travelled towards or away from the North Pole
	 */
	public Vector2 velocity
	{
		get { return _velocity; }
		set { _velocity = value; if(value.sqrMagnitude > 1) _velocity = value.normalized;
			  if(_normal.HasValue) _velocity = Vector3.ProjectOnPlane(_velocity, _normal.Value); } //FIXME: better way to project velocity? set is independent of position
	}

	public Vector3 South
	{
		get { return _south; }
		set { _south = value; if(value.sqrMagnitude > 1) _south = value.normalized; } //TODO: get rid of set and put it in curPosition set
	}

	public Vector3 West
	{
		get { return _south; }
		set { _south = value; if(value.sqrMagnitude > 1) _south = value.normalized; } //TODO: get rid of set and put it in curPosition set
	}

	public Optional<Vector3> normal
	{
		get { return _normal; }
		set { _normal = value; if(value.HasValue && value.Value.sqrMagnitude > 1) _normal.Value.Normalize(); } //TODO: vector reject velocity onto normal as well, move to curPosition set
	}

	public Optional<Vector3> right
	{
		get { return _right; }
		set { _right = value; if(value.HasValue && value.Value.sqrMagnitude > 1) _right.Value.Normalize(); } //TODO: move to curPosition set
	}

	public Vector2 input
	{
		get { return _input; }
		set { _input = value; if(value.sqrMagnitude > 1) _input = value.normalized; }
	}

	abstract public void Move();
}
