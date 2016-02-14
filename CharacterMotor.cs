using UnityEngine;
using System.Collections;
using System.Collections.Generic;

abstract public class CharacterMotor : Component
{
	Vector3											_curPosition;
	Vector3											_prevPosition;

	Vector2											_velocity;

	Vector3											_south;
	Vector3											_west;

	Optional<GroundInfo>							ground;

	Dictionary<string, float>						flags; //XXX: Strategy Pattern?

	Vector2											_input;

	//TODO: make flags into delegater Queue<*fun()> https://social.msdn.microsoft.com/Forums/en-US/2c08a0d0-58e4-4df6-b6d3-75e785fff8a8/array-of-function-pointers?forum=csharplanguage

	//ideally there should be three setters: position, velocity, and segment
	
	public Block block
	{
		get
		{
			return ground.Value.block;
		}
	}

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

			_south = FindSouth(_curPosition);
			_west  = Vector3.Cross(_south, _curPosition);

			if(ground.HasValue)
			{
				ground.Value.right  = ground.Value.segment.EvaluateRight(ground.Value.t);
				ground.Value.normal = ground.Value.segment.EvaluateNormal(curPosition, ground.Value.right);
			}
		}
	}

	public bool grounded
	{
		get
		{
			return ground.HasValue;
		}
	}

	public Vector2 input
	{
		get
		{
			return _input;
		}
		set
		{
			_input = value;
			
			if(value.sqrMagnitude > 1)
			{
				_input = value.normalized;
			}
		}
	}

	public Vector3 normal
	{
		get
		{
			return ground.Value.normal;
		}
		//TODO: vector reject velocity onto normal as well, move to curPosition set
	}
	
	public Vector3 right
	{
		get
		{
			return ground.Value.right;
		}
	}

	public SphericalIsoscelesTrapezoid segment
	{
		get
		{
			return ground.Value.segment;
		}
	}

	public Vector3 South
	{
		get
		{
			return _south;
		}
	}

	public float t
	{
		get
		{
			return ground.Value.t;
		}
		set
		{
			ground.Value.t = value;
			curPosition = SphericalIsoscelesTrapezoid.Evaluate(ref ground.Value.t, 0.01f, ref ground.Value.segment);
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
		get
		{
			return _velocity;
		}
		set
		{
			_velocity = value;

			if(value.sqrMagnitude > 1)
			{
				_velocity.Normalize();
			}

			if(ground.HasValue)
			{
				_velocity = Vector3.ProjectOnPlane(_velocity, ground.Value.normal);
			}
		}
	}

	public Vector3 West
	{
		get
		{
			return _west;
		}
	}

	public Vector3 FindSouth(Vector3 pos)
	{
		float xz = Mathf.Sqrt(pos.x*pos.x + pos.z*pos.z);
		float xfactor = pos.x / (pos.x + pos.z);
		float zfactor = 1f - xfactor;
		return new Vector3(xfactor/pos.y, -1/xz, zfactor*pos.y); //TODO: check
	}

	public void Traverse(Optional<SphericalIsoscelesTrapezoid> SIT, Vector3 desiredPos, Vector3 curPos)
	{
		if(SIT.HasValue)
		{
			ground = new GroundInfo();

			ground.Value.segment = SIT.Value;
			ground.Value.block   = SIT.Value.GetComponentInParent<Block>();
			ground.Value.t		 = SIT.Value.Intersect(desiredPos, curPos, 0.01f).Value; //NOTE: must be guaranteed to exist by calling function for this to work (e.g. Collision Detector :: Update)

			curPosition = SphericalIsoscelesTrapezoid.Evaluate(ref ground.Value.t, 0.01f, ref ground.Value.segment);

			ground.Value.right	 = SIT.Value.EvaluateRight(ground.Value.t);
			ground.Value.normal	 = SIT.Value.EvaluateNormal(curPos, ground.Value.right);
		}
		else
		{
			ground = new Optional<GroundInfo>();
		}
	}
}
