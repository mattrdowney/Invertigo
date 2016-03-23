using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CharacterMotor : MonoBehaviour //TODO: make abstract //CONSIDER: make Component
{
	[SerializeField] Vector3						_curPosition;
	[SerializeField] Vector3						_prevPosition;
	
	[SerializeField] public float					phi;
	[SerializeField] public float					theta;

	[SerializeField] public float					vertical_velocity;
	[SerializeField] public float					horizontal_velocity;

	[SerializeField] Vector3						_south;
	[SerializeField] Vector3						_west;

	optional<GroundInfo>							ground;

	Dictionary<string, float>						flags; //XXX: Strategy Pattern?

	[SerializeField] Vector2						_input;

	[SerializeField] float							_radius;

	//TODO: make flags into delegater Queue<*fun()> https://social.msdn.microsoft.com/Forums/en-US/2c08a0d0-58e4-4df6-b6d3-75e785fff8a8/array-of-function-pointers?forum=csharplanguage

	//ideally there should be three setters: position, velocity, and segment

	public float angle
	{
		get
		{
			return ground.data.angle;
		}
		set
		{
			ground.data.angle = value;
			curPosition = ArcOfSphere.Evaluate(ref ground, radius);
		}
	}

	public Block block
	{
		get
		{
			return ground.data.block;
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

			_west  =  FindWest(_curPosition);
			_south = -Vector3.Cross(_west, _curPosition).normalized;

			if(ground.exists)
			{
				ground.data.right  = ground.data.arc.EvaluateRight (ground.data.angle, 0);
				ground.data.normal = ground.data.arc.EvaluateNormal(ground.data.angle, 0);
			}
		}
	}

	public bool grounded
	{
		get
		{
			return ground.exists;
		}
	}

	public float height
	{
		get
		{
			return ground.data.height;
		}
		set
		{
			ground.data.height = value;
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
			return ground.data.normal;
		}
		//TODO: vector reject velocity onto normal as well, move to curPosition set
	}

	public float radius
	{
		get
		{
			return _radius;
		}
		set
		{
			_radius = value;
		}
	}

	public Vector3 right
	{
		get
		{
			return ground.data.right;
		}
	}

	public ArcOfSphere segment
	{
		get
		{
			return ground.data.arc;
		}
	}

	public Vector3 South
	{
		get
		{
			return _south;
		}
	}

	public Vector3 prevPosition
	{
		get { return _prevPosition; }
	}

	/** Horizontal (x) velocity is the distance travelled along the circumference of the intersection of the unit sphere and an xz plane.
	 *  Vertical   (y) velocity is the distance travelled towards or away from the North Pole
	 */
	/*public Vector2 velocity
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

			if(ground.exists)
			{
				_velocity = Vector3.ProjectOnPlane(_velocity, ground.data.normal);
			}
		}
	}*/

	public Vector3 West
	{
		get
		{
			return _west;
		}
	}

	public Vector3 FindWest(Vector3 pos)
	{
		float xz = Mathf.Sqrt(pos.x*pos.x + pos.z*pos.z);
		float xfactor = pos.x / (pos.x + pos.z);
		float zfactor = 1f - xfactor;
		return new Vector3(-xfactor/pos.y, 1/xz, -zfactor*pos.y).normalized; //TODO: check
	}

	public void Traverse(optional<ArcOfSphere> arc, Vector3 desiredPos)
	{
		if(arc.exists)
		{
			ground = new GroundInfo();

			ground.data.arc 	= arc.data;
			ground.data.block   = arc.data.GetComponentInParent<Block>();
			ground.data.angle	= arc.data.Intersect(desiredPos, prevPosition, radius).data; //NOTE: must be guaranteed to exist by calling function for this to work (e.g. Collision Detector :: Update)
			height				= arc.data.LengthRadius(radius);

			curPosition = ArcOfSphere.Evaluate(ref ground, radius);

			ground.data.right	= arc.data.EvaluateRight (ground.data.angle, radius); 
			ground.data.normal	= arc.data.EvaluateNormal(ground.data.angle, radius);
		}
		else
		{
			ground = new optional<GroundInfo>();
		}
	}

	public void Move(Vector2 input)
	{
		if(grounded)
		{
			angle += input.x / height / 64;

			transform.position = ArcOfSphere.Evaluate(ref ground, radius);
		}
		else
		{
			SphereUtility.Accelerate(ref phi, ref theta, ref vertical_velocity, ref horizontal_velocity, 0.01f, -input.x/100, Time.fixedDeltaTime);

			transform.position = SphereUtility.Position(Vector3.right, Vector3.forward, Vector3.up, phi, theta).normalized;
		}

		curPosition = transform.position;
	}

	public void Jump()
	{
		if(grounded)
		{
			phi   = Mathf.Acos(transform.position.y);
			theta = Mathf.Atan2(transform.position.z, transform.position.x);

			Vector3 normal = ground.data.arc.EvaluateNormal(ground.data.angle);

			Debug.Log("normal: " + normal);
			Debug.Log("South: " + South);
			Debug.Log("West: " + West);

			horizontal_velocity = 0.1f*Vector3.Dot(West, normal);
			vertical_velocity   = 0.1f*Vector3.Dot(South, normal);


			Traverse(new optional<ArcOfSphere>(), transform.position);
		}
	}
}
