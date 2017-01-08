using UnityEngine;
using System.Collections;

abstract public class CharacterState : ScriptableObject
{
	abstract public CharacterState StateMachine(CharacterMotor self);

    public void FallingUpdate(CharacterMotor self)
    {
        SphereUtility.Accelerate(ref self.phi, ref self.theta, ref self.vertical_velocity, ref self.horizontal_velocity, 0.03f, -self.input.x/10, Time.fixedDeltaTime);

		self.current_position = SphereUtility.SphereToCartesian(new Vector2(self.phi, self.theta));
		self.transform.rotation = Quaternion.LookRotation(self.current_position, self.North);
    }

    public void GroundedUpdate(CharacterMotor self)
    {
        if(self.input.sqrMagnitude > 1)
        {
            self.input.Normalize();
        } 
        
		Vector3 input3D = new Vector3(self.input.x, self.input.y, 0f); //FIXME: JANK
		if(input3D.sqrMagnitude > 1) input3D.Normalize();

        Quaternion rotation = Quaternion.LookRotation(self.current_position, self.arc.EvaluateNormal(self.angle, self.radius));

        float left_product  = Vector3.Dot(rotation * input3D, self.left);
        float right_product = Vector3.Dot(rotation * input3D, self.right);
        float product = -Mathf.Abs(left_product);
        if (right_product > left_product)
        {
            product = +Mathf.Abs(right_product);
        }
        if (right_product < 0 && left_product < 0)
        {
            product = 0;
        }

        self.angle += product / self.height / 64; //FIXME: slight math error here-ish

		self.current_position = ArcOfSphere.Evaluate(self.ground.data, self.radius);

		self.transform.rotation = Quaternion.LookRotation(self.current_position, self.arc.EvaluateNormal(self.angle, self.radius));
    }

    public bool JumpingTransitionState(CharacterMotor self)
	{
        if (self.grounded && Time.time - self.jump_request < 0.2f)
        {
            Vector3 normal = self.ground.data.arc.EvaluateNormal(self.ground.data.angle);

            self.phi   = Mathf.Acos(self.current_position.y); //TODO: Transform2sD.SphericalPosition
			self.theta = Mathf.Atan2(self.current_position.z, self.current_position.x); //FIXME: magic numbers

			Quaternion rotation = Quaternion.LookRotation(self.current_position, self.arc.EvaluateNormal(self.angle, self.radius));

			Vector3 input3D = new Vector3(self.input.x, self.input.y, 0f);
            input3D = rotation * input3D;
            input3D = Vector3.ProjectOnPlane(input3D, normal);

			if(input3D.sqrMagnitude > 1) input3D.Normalize();

			self.horizontal_velocity = -0.1f*Vector3.Dot(self.East , normal) + -0.1f*Vector3.Dot(self.East , input3D);
			self.vertical_velocity   = -0.1f*Vector3.Dot(self.North, normal) + -0.1f*Vector3.Dot(self.North, input3D);

			self.ground = new optional<GroundInfo>();

            self.current_position = (self.current_position + normal * 1e-6f).normalized;
            
            self.jump_request = -100;

            return true;
        }

        return false;
	}

    public bool LandingTransitionState(CharacterMotor self)
    {
        if (!self.collision_detector.exists)
        {
            return false;
        }

        //Calculate collision information
        optional<ArcOfSphere> arc = self.collision_detector.data.ArcCast(self.current_position, self.previous_position, self.radius);

        if (arc.exists)
        {
            optional<Vector3> collision_point = arc.data.Intersect(self.current_position, self.previous_position, self.radius);

            if (collision_point.exists)
            {
                self.Traverse(arc.data, collision_point.data);
            }
            else
            {
                Debug.Log("Didn't collide?");
            }
        }

        return arc.exists;
    }
}
