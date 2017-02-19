using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CharacterMotor : Motor //TODO: make abstract //CONSIDER: make Component
{
    CharacterState                                  state;                                  
    public optional<CollisionDetector>              collision_detector;

	[SerializeField] Vector3						_current_position;
	[SerializeField] Vector3						_previous_position;
	
	[SerializeField] public float					phi;
	[SerializeField] public float					theta;

	[SerializeField] public float					vertical_velocity;
	[SerializeField] public float					horizontal_velocity;

	public optional<GroundInfo>						ground;
    optional<Nexus>                                 connection;

	Dictionary<string, float>						flags; //XXX: Strategy Pattern?

	[SerializeField] Vector2						_input;

	[SerializeField] float							_radius;

    public float                                    jump_request;

	//TODO: make flags into delegater Queue<*fun()> https://social.msdn.microsoft.com/Forums/en-US/2c08a0d0-58e4-4df6-b6d3-75e785fff8a8/array-of-function-pointers?forum=csharplanguage

	//ideally there should be ~4 setters: position, velocity, segment, Nexus

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

    public bool between_levels
    {
        get
        {
            return connection.exists;
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
		set
		{
			_previous_position = _current_position;
            _current_position = transform.position = value; //seriously?
		}
	}

	public Vector3 East
	{
		get
		{
			return Vector3.Cross(Vector3.up, current_position).normalized;;
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

    public Vector3 left
    {
        get
        {
            return arc.EvaluateLeft(angle, radius);
        }
    }

	public Vector3 normal
	{
		get
		{
			return arc.EvaluateNormal(angle, radius);
		}
	}

	public Vector3 North
	{
		get
		{
			return -South;
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
            this.transform.localScale = new Vector3(value, value, value);
            _radius = value * 0.5f /*this.GetComponent<SphereCollider>().radius*/;
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
			return Vector3.Cross(East, current_position).normalized;
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
			return -East;
		}
	}

    void Start()
    {
        state = new PlayerFallingState();
        collision_detector = this.gameObject.GetComponent<CollisionDetector>();
        radius = 0.05f;
		jump_request = -100;
    }

    void Update()
    {
        if(Input.GetButtonDown("Jump"))
		{
			jump_request = Time.time;
		}

        input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis ("Vertical"));
    }

    void FixedUpdate()
    {
        // use State Machine design pattern
        state = state.StateMachine(this);
    }

	public bool Traverse(ArcOfSphere path, Vector3 desiredPos) //I don't like these parameters, they can be fixed //I don't like that this is public, it should be private and exposed via a generic move option if possible
	{
        optional<float> interpolation_factor = path.CartesianToRadial(desiredPos);

        if (interpolation_factor.exists)
        {
            ground = new GroundInfo();

            ground.data.angle = interpolation_factor.data;

            ground.data.arc = path;
            ground.data.block = path.GetComponentInParent<Block>();
            ground.data.height = path.LengthRadius(radius);
            ground.data.begin = path.Begin(radius);
            ground.data.end = path.End(radius);

            current_position = ArcOfSphere.Evaluate(ground.data, radius);
        }
        else
        {
            Debug.Log("Critical Failure: Traverse's interpolation factor doesn't exist!");
        }

        return interpolation_factor.exists;

    }

    public void EnterNexus(Nexus _connection)
    {
        connection = _connection;
        this.GetComponent<CollisionDetector>().Deactivate(); //TODO: UNJANKIFY?
        ground = new optional<GroundInfo>();
    }

    public void ExitNexus()
    {
        connection = new optional<Nexus>();
    }

    public void Move(Vector2 input)
	{
        if (between_levels) //FEATURE: enable movement (grounded and aerial) in between_levels
        {
            connection.data.Move(Input.GetAxis("Vertical"), this);
        }
        else if (grounded) //FIXME: this entire block if it is JANK //spelling -_-'
		{
			if(input.sqrMagnitude > 1) input.Normalize(); 

			Transform camera_transform = GameObject.Find("MainCamera").transform; //FIXME: JANK

			Vector3 input3D = new Vector3(input.x, input.y, 0f); //FIXME: JANK
			if(input3D.sqrMagnitude > 1) input3D.Normalize();

            float left_product  = Vector3.Dot(camera_transform.rotation * input3D, left);
            float right_product = Vector3.Dot(camera_transform.rotation * input3D, right);
            float product = -Mathf.Abs(left_product);
            if (right_product > left_product)
            {
                product = +Mathf.Abs(right_product);
            }
            if (right_product < 0 && left_product < 0)
            {
                product = 0;
            }
            angle += product / height / 64; //FIXME: slight math error here-ish

			current_position = ArcOfSphere.Evaluate(ground.data, radius);

			transform.rotation = Quaternion.LookRotation(current_position, arc.EvaluateNormal(angle, radius));
		}
		else
		{
			SphereUtility.Accelerate(ref phi, ref theta, ref vertical_velocity, ref horizontal_velocity, 0.03f, -input.x/10, Time.fixedDeltaTime);

			current_position = SphereUtility.SphereToCartesian(new Vector2(phi, theta));
			transform.rotation = Quaternion.LookRotation(current_position, North);
		}
	}

	public bool Jump()
	{
		if(grounded)
		{
            Vector3 normal = ground.data.arc.EvaluateNormal(ground.data.angle);

            phi   = Mathf.Acos(current_position.y); //TODO: Transform2sD.SphericalPosition
			theta = Mathf.Atan2(current_position.z, current_position.x); //FIXME: magic numbers

			Transform camera_transform = GameObject.Find("MainCamera").transform; //FIXME: JANK

			Vector3 input3D = new Vector3(input.x, input.y, 0f);
            input3D = camera_transform.rotation * input3D;
            input3D = Vector3.ProjectOnPlane(input3D, normal);

			if(input3D.sqrMagnitude > 1) input3D.Normalize();

			horizontal_velocity = -0.1f*Vector3.Dot(East , normal) + -0.1f*Vector3.Dot(East , input3D);
			vertical_velocity   = -0.1f*Vector3.Dot(North, normal) + -0.1f*Vector3.Dot(North, input3D);

			ground = new optional<GroundInfo>();

            current_position = (current_position + normal * 1e-6f).normalized;
		}

        state = new PlayerFallingState();

        return grounded;
	}
}
