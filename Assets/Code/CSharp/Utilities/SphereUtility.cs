using UnityEngine;

public class SphereUtility
{
    public static Vector3 SphereToCartesian(Vector2 phi_theta)
    {
        Vector3 result = new Vector3();

        result.x = Mathf.Sin(phi_theta.x) * Mathf.Cos(phi_theta.y);
        result.y = Mathf.Cos(phi_theta.x);  //NOTE: y/z swapped because unity has weird axes
        result.z = Mathf.Sin(phi_theta.x) * Mathf.Sin(phi_theta.y);

        return result;
    }

    public static Vector3 SphereToCartesian(Vector2 phi_theta, Vector3 x_axis, Vector3 y_axis, Vector3 z_axis)
	{
        Vector3 base_position = SphereToCartesian(phi_theta);

        Vector3 result = new Vector3();
   
        result  = x_axis * base_position.x;
		result += y_axis * base_position.z;  //NOTE: y/z swapped because unity has weird axes
        result += z_axis * base_position.y; 

        return result;
	}

    public static Vector2 CartesianToSphere(Vector3 cartesian_vector)
    {
        DebugUtility.Assert(Mathf.Abs(cartesian_vector.sqrMagnitude - 1f) < 1e-6, "Cartesian vector was not normalized when passed in");

        float phi = Mathf.Acos(cartesian_vector.y);
        float theta = Mathf.Atan2(cartesian_vector.z, cartesian_vector.x);

        return new Vector2(phi, theta);
    }

	/** TODO: aesthetically pleasing explanation
	 * 
	 */
	public static optional<Vector3> Intersection(Vector3 begin, Vector3 end, Vector3 center, float radius) //all credit: http://gis.stackexchange.com/questions/48937/how-to-calculate-the-intersection-of-2-circles
	{
		//TODO: make optional!

		Vector3 path_center = -Vector3.Cross(begin, end).normalized;

        Debug.DrawRay(center, Vector3.right * radius, Color.white, 3f);
        Debug.DrawLine(path_center, Vector3.zero, Color.gray, 3f);

        /*if (Vector3.SqrDistance(center, path_center) > radius*radius + Mathf.Sqrt(1+1)*Mathf.Sqrt(1+1) || //TODO: optional<Vector3>: make sure the two centers are at most radius1 + radius2 apart
            center == path_center || center == -path_center)
        {
            return new optional<Vector3>();
        }*/ //FIXME: JANK, unreadable, erroneous, disgusting

        float cos_r1 = Mathf.Cos(radius); 		//gives the distance that should be travelled along   "center"    to reach the origin of the sphere with radius "radius"
		float cos_r2 = Mathf.Cos(Mathf.PI / 2); //gives the distance that should be travelled along "path_center" to reach the origin of the sphere with radius PI / 2
		
		float cos_angle = Vector3.Dot(center, path_center); //the magnitude of center and path_center are 1, so |center||path_center|cos(angle) = center . path_center gives cos(angle)

        if (Mathf.Abs(cos_angle) >= 1f - 1e-6f) //podal or antipodal points have 0, 1, or infinite intersection points
        { //EW: magic numbers
            return new optional<Vector3>();
        }

		Vector3 binormal_1 = Vector3.Cross(path_center, Vector3.right).normalized; //TODO: optional here I think
		Vector3 binormal_2 = Vector3.Cross(center, Vector3.right).normalized;
		
		float center_fraction = (cos_r1 - cos_r2 * cos_angle) / (1 - cos_angle*cos_angle); //FIXME: rename
		float path_fraction   = (cos_r2 - cos_r1 * cos_angle) / (1 - cos_angle*cos_angle);

		Vector3 intersection_center = center_fraction*center + path_fraction*path_center;

		Vector3 binormal = Vector3.Cross(center, path_center); //TODO: CHECK: is this name right?

		float midpoint_distance = Mathf.Sqrt((1 - Vector3.Dot(intersection_center, intersection_center)) / Vector3.Dot(binormal, binormal)); //CONSIDER: rename?

		Vector3 intersection_1 = intersection_center + midpoint_distance*binormal;
		Vector3 intersection_2 = intersection_center - midpoint_distance*binormal;

		if(Vector3.Distance(begin, intersection_1) < Vector3.Distance(begin, intersection_2)) //NOTE: (to my knowledge) only works when the player can move at most player_radius units per frame
		{
			return intersection_1.normalized;
		}
		
		return intersection_2.normalized;
	}
	
	public static void Accelerate(ref float phi, ref float theta, ref float vertical_velocity, ref float horizontal_velocity, float gravity, float traction, float delta_time)
	{
		phi   += vertical_velocity   * delta_time / 2;
		theta += horizontal_velocity * delta_time / 2 / Mathf.Sin(phi); //Assert: Sin(0) and Sin(180) must be un-reachable
		
		vertical_velocity   += gravity  * delta_time / 2;
		horizontal_velocity += traction * delta_time; 
		vertical_velocity   += gravity  * delta_time / 2;
		
		phi   += vertical_velocity   * delta_time / 2;
		theta += horizontal_velocity * delta_time / 2 / Mathf.Sin(phi); //Assert: Sin(0) and Sin(180) must be un-reachable
	}
	
	/*
	public static void Accelerate(ref float phi, ref float theta, ref float vertical_velocity, ref float horizontal_velocity)
	{
		//eq 73 doesn't work http://mathworld.wolfram.com/SphericalCoordinates.html unless I change my definition of x velocity
		//y_position does not depend on x_position, but x_position depends on y_position
		//x_position(t, y_position)
		//y_position(t)
		//maybe using the angular version of x_velocity would work better in the long run? I will assume for now that it won't, though

		velocity_x = distance travelled per second
		velocity_y = distance travelled per second aka radians per second

		..... BEGIN SHITTY_ATTEMPT

		median_y_radius(position_y1, position_y2) //median isn't the best word
		{
			float y1_sign = System.Math.Sign(position_y1 - Mathf.PI / 2);
			float y2_sign = System.Math.Sign(position_y2 - Mathf.PI / 2);

			if(y1_sign*y2_sign == -1) //this probably isn't necessary if I use real integration
			{
				//there is a subtle integration bug here, since the radii are strictly averaged
				return ( median_y_radius(position_y1, Mathf.PI / 2) + median_y_radius(Mathf.PI / 2, position_y2) ) / 2; 
			}

			float radius1 = Radius(position_y1);
			float radius2 = Radius(position_y2);

			//?

			return median_radius;
		}

		position_x = position_x_o + median_y_radius*2*PI/velocity_x*delta_time;
		position_y = position_y_o + velocity_y*delta_time;

		..... END SHITTY_ATTEMPT

		1/(b-a)*Integral(a,b)[Sin(phi)]d_phi <-- mean value theorem doesn't work perfectly because of acceleration...

		//a = position_y;
		//b = position_y + velocity_y*delta_time;

		goes to

		?

		//https://www.niksula.hut.fi/~hkankaan/Homepages/gravity.html //Man, I always bring out this link...
		velocity_x += traction   * delta_time / 2; //If I wanted to be more precise/fancy, I could factor the change of velocity_x into IntegrateX //Okay, so maybe "I" couldn't
		velocity_y += gravity    * delta_time / 2; //I think we need some RK4 up in here... //not really, I don't think it works by virtue of not being a time-exclusive derivative
		position_x += IntegrateX(delta_time, velocity_x, position_y, velocity_y);
		position_y += velocity_y * delta_time;
		velocity_x += traction   * delta_time / 2;
		velocity_y += gravity    * delta_time / 2;

		//Seriously, why am I doing this to myself? With enough frame-rate no sane person would notice...
		//sleeping on it...

		//Maybe I can do 1/(Integral(a,b)[P(x)]dx)*Integral(a,b)[P(x)*E(x)]]d_x
		// instead of 1/(b-a)*Integral(a,b)[1*E(x)]]d_x
		// which means the equation might be:
		// 1/(vel_y*pos_y + 2*vel_y*pos_y*pos_y)*Integral(a,b)[(vel_y + vel_y*pos_y)*E(x)]]d_x

		//this, the way I see it now, is a way of weighting the distribution,
		// sort of like how you can get the moment of inertia by Integral(a,b)[Mass(x)*DistanceFromCenter(x)]dx/TotalMass

		//I need a probability distribution function that generates the fraction of time spent in a given position during freefall...
		// while the original equation might be it, I sort of doubt it.

		//by "probability distribution function" I probably don't mean probability density or mass functions since I doubt everything will add conveniently up to 1
		// since timestamps are variable (from .02s to .1s to ?)

		//likely turn of events: use delta = vel_o*t + .5*a*t*t as a density function where vel_xo*t (mostly(?)) "drops out"
		// this is because delta is a representation (if I'm right) of what thin slice of space a particle occupies at time t.
		// I might have to take a reciprocal or something, though

		//End bullshit math
	}
	*/
}