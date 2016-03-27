using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CharacterMotor : MonoBehaviour //TODO: make abstract //CONSIDER: make Component
{
	[SerializeField] Vector3						_current_position;
	[SerializeField] Vector3						_previous_position;
	
	[SerializeField] public float					phi;
	[SerializeField] public float					theta;

	[SerializeField] public float					vertical_velocity;
	[SerializeField] public float					horizontal_velocity;

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
			current_position = ArcOfSphere.Evaluate(ground.data, radius);
		}
	}

	public ArcOfSphere arc
	{
		get
		{
			return ground.data.arc;
		}
	}

	public Block block
	{
		get
		{
			return ground.data.block;
		}
	}

	public Vector3 current_position
	{
		get
		{
			return _current_position;
		}
		set //set must contain extra logic for gravity, normal, and right
		{
			_previous_position = _current_position;
			_current_position = value;
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
			return arc.EvaluateNormal(angle, radius);
		}
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
			return arc.EvaluateRight(angle, radius);
		}
	}

	public Vector3 South
	{
		get
		{
			return Vector3.Cross(West, current_position).normalized;
		}
	}

	public Vector3 previous_position
	{
		get { return _previous_position; }
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
			//Assert position != Vector3.up or Vector3.down
			return Vector3.Cross(Vector3.up, current_position).normalized;
		}
	}

	public void Traverse(ArcOfSphere path, Vector3 desiredPos)
	{
		ground = new GroundInfo();

		ground.data.arc		= path;
		ground.data.block	= path.GetComponentInParent<Block>();
		ground.data.angle	= path.Intersect(desiredPos, previous_position, radius).data; //NOTE: must be guaranteed to exist by calling function for this to work (e.g. Collision Detector :: Update)
		ground.data.height	= path.LengthRadius(radius);
		ground.data.begin   = path.Begin(radius);
		ground.data.end		= path.End(radius);

		current_position = ArcOfSphere.Evaluate(ground.data, radius);
	}

	public void Move(Vector2 input)
	{
		if(grounded) //FIXME: this entire if is JANK
		{
			if(input.sqrMagnitude > 1) input.Normalize();

			Transform camera_transform = GameObject.Find("MainCamera").transform;

			Vector3 input3D = new Vector3(input.x, input.y, 0f);
			if(input3D.sqrMagnitude > 1) input3D.Normalize();

			angle += Vector3.Dot(camera_transform.rotation*input3D, right) / height / 64;

			transform.position = ArcOfSphere.Evaluate(ground.data, radius);
		}
		else
		{
			SphereUtility.Accelerate(ref phi, ref theta, ref vertical_velocity, ref horizontal_velocity, 0.03f, -input.x/10, Time.fixedDeltaTime);

			transform.position = SphereUtility.Position(Vector3.right, Vector3.forward, Vector3.up, phi, theta).normalized;
		}

		current_position = transform.position;
	}

	public void Jump()
	{
		if(grounded)
		{
			phi   = Mathf.Acos(transform.position.y);
			theta = Mathf.Atan2(transform.position.z, transform.position.x);

			Vector3 normal = ground.data.arc.EvaluateNormal(ground.data.angle);

			Transform camera_transform = GameObject.Find("MainCamera").transform;

			Vector3 input3D = new Vector3(input.x, input.y, 0f);
			if(input3D.sqrMagnitude > 1) input3D.Normalize();

			horizontal_velocity = -0.1f*Vector3.Dot(West, normal) + -0.1f*Vector3.Dot(West, camera_transform.rotation*input3D);
			vertical_velocity   =  0.1f*Vector3.Dot(South, normal) + 0.1f*Vector3.Dot(South, camera_transform.rotation*input3D);

			ground = new optional<GroundInfo>();
		}
	}
}
