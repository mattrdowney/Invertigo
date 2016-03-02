/** The primative spherical geometry component that is used to traverse a block or terrain
 *
 * TODO: detailed description
 * 
 * @file
 */

using UnityEngine;
using System.Collections;
using System.Linq;
//using System.Diagnostics;

[System.Serializable]
public class SphericalIsoscelesTrapezoid /* : Component*/ : MonoBehaviour //TODO: get rid of this in production builds
{
	/*CONSIDER: make const*/
	[SerializeField] public SphericalIsoscelesTrapezoid		next; //CONSIDER: add k prefix
	[SerializeField] public SphericalIsoscelesTrapezoid		prev;

	[SerializeField] Vector3								path_center;
	[SerializeField] Vector3								path_normal;

	[SerializeField] Vector3								arc_left;
	[SerializeField] Vector3								arc_right; //CONSIDER: can be dropped

	[SerializeField] Vector3								arc_left_up; //FIXME: shitty names 
	[SerializeField] Vector3								arc_right_down; //FIXME
	
	[SerializeField] public float							arc_radius; //FIXME: temp hack public
	[SerializeField] public float							arc_angle; //the angle to sweep around the center. FIXME:
	[SerializeField] float									angle_to_normal;

	static int 												guid = 0;

	/** Find the center of a character path when the circle is extruded by the character's height 
	 * 
	 */
	Vector3 Center(float height)
	{
		return path_normal * Mathf.Cos(angle_to_normal - height);
	}

	/** Determine if the character (represented by a point) is inside of a trapezoid (extruded by the radius of the player)
	 *  
	 */
	public bool Contains(Vector3 pos, float radius)
	{
		float prod = Vector3.Dot(pos - path_center, path_normal);

		bool bIsAtCorrectElevation = 0 <= prod && prod <= 1f; //FIXME: INFINI-JANK
		bool bLeftContains		   = Vector3.Dot(pos, arc_left_up ) >= 0;
		bool bRightContains		   = Vector3.Dot(pos, arc_right_down) >= 0;
		bool bIsObtuse			   = Vector3.Dot(arc_left, arc_right) <= 0;
		int  nOutOfThree		   = CountBooleans(bLeftContains, bRightContains, bIsObtuse);

		return bIsAtCorrectElevation && nOutOfThree >= 2; //XXX: might even now be wrong
	}

	/** Counts the number of booleans that are true in a comma separated list of booleans
	 * 
	 *  credit: http://stackoverflow.com/questions/377990/elegantly-determine-if-more-than-one-boolean-is-true
	 * 
	 *  @example "CountBooleans(true, false, true, true);" will return 3
	 */
	static int CountBooleans(params bool[] boolean_list) //allow for comma separated booleans using "params"
	{
		return boolean_list.Count(bIsTrue => bIsTrue); //count booleans that are true using Linq's .Count function
	}

	public Optional<float> Distance(Vector3 to, Vector3 from)
	{
		Optional<float> intersection = Intersect(to, from, 0.01f);
		
		if(intersection.HasValue)
		{
			float t = intersection.Value;
			Vector3 newPos = Evaluate(t, 0.01f);
			return Vector3.Distance(from, newPos);
		}
		
		return new Optional<float>();
	}

	void DrawArc(float height, Color color)
	{
		UnityEditor.Handles.color = color;

		UnityEditor.Handles.DrawWireArc(Center(height), path_normal, Evaluate(0, height), arc_angle * 180 / Mathf.PI, Radius(height));
	}

	void DrawRadial(float t, float height, Color color)
	{
		//if(arc_radius != 0) return;

		UnityEditor.Handles.color = color;
		UnityEditor.Handles.DrawLine(Evaluate(t, 0), Evaluate(t, height)); 
	}

	/** return the position of the player based on the circular path
	 *  
	 */
	public Vector3 Evaluate(float t, float height)
	{
		float angle = t / arc_radius; //FIXME: include height

		Vector3 x = arc_left    * Mathf.Sin(angle_to_normal - height) * Mathf.Cos(angle);
		Vector3 y = arc_left_up * Mathf.Sin(angle_to_normal - height) * Mathf.Sin(angle);
		Vector3 z = path_normal * Mathf.Cos(angle_to_normal - height);

		return x + y + z;
	}

	/** return the position of the player based on the circular path
	 *  
	 */
	public Vector3 Evaluate(float t) { return Evaluate(t, 0); }

	/** return the position of the player based on the circular path
	 * 
	 *  return the position of the player based on the circular path
	 *  If the player would go outside of [0, arcCutoffAngle*arcRadius],
	 *  the Trapezoid should transfer control of the player to (prev, next) respectively
	 */
	public static Vector3 Evaluate(ref float t, float height, ref SphericalIsoscelesTrapezoid seg)
	{
		if(t > seg.arc_angle*seg.arc_radius)
		{
			t -= seg.arc_angle*seg.arc_radius;
			seg = seg.next;
			return Evaluate(ref t, 0.01f, ref seg);
		}
		if(t < 0)
		{
			t += seg.prev.arc_angle*seg.prev.arc_radius;
			seg = seg.prev;
			return Evaluate(ref t, 0.01f, ref seg);
		}
		
		return seg.Evaluate(t, height);
	}

	public Vector3 EvaluateNormal(Vector3 pos, Vector3 right)
	{
		return Vector3.Cross(right, pos).normalized;
	}

	public Vector3 EvaluateNormal(float t, float height)
	{
		Vector3 pos = Evaluate(t, height);
		Vector3 right = EvaluateRight(t, height);

		return EvaluateNormal(pos, right);
	}

	public Vector3 EvaluateRight(float t, float height)
	{
		float angle = t / arc_radius;
		Vector3 x = arc_left_up * Mathf.Sin(angle_to_normal + height) * Mathf.Cos(angle);
		Vector3 y = arc_left    * Mathf.Sin(angle_to_normal + height) * Mathf.Sin(angle);
		Vector3 z = path_normal * Mathf.Cos(angle_to_normal + height);
		return x + y + z;
	}

	/** Recompute the orientation of a SphericalIsoscelesTrapezoid
	 * 
	 *  Destroys all information other than prev, next. Replaces this information with the information for traversing
	 *      the top of a SphericalIsoscelesTrapezoid on a unit sphere.
	 * 
	 * @param left_edge: the left-bottom point (left implies it is the 1st point when enumerated clockwise for concave objects,
	 * 		  bottom implies it is the position of the player's feet)
	 * @param right_edge: the right-bottom point (right implies it is the 2nd point when enumerated clockwise for concave objects,
	 * 		  bottom implies it is the position of the player's feet)
	 * @param normal: the normal plane that intersects lhs and rhs and forms the walking path for the players center
	 * 		  of mass, sign matters because it indicates which direction is up for calculating the center of mass.
	 * 
	 * @example Initialize(Vector3(0,0,1), Vector3(1,0,0), Vector3(0,1,0)) will initialize a Spherical Isosceles Trapezoid
	 *          that is a great circle for the feet positions, a large lesser circle for the center of mass position,
	 *          with a 90 degree arc going from forwards to right and a normal going in the positive y-direction.
	 */
	public void Initialize(Vector3 left_edge, Vector3 right_edge, Vector3 normal) //TODO: delegate Initialize corner?
	{
		//DebugUtility.Assert(Mathf.Approximately(Vector3.Dot(right_edge - left_edge, normal), 0),
		//                    "SphericalIsoscelesTrapezoid: Initialize: failed assert");
		
		path_normal = normal.normalized;
		path_center = normal*Vector3.Dot(left_edge, normal); //or right_edge

		arc_left  = (left_edge  - path_center).normalized; //FIXME: obsolete for corner triangles
		arc_right = (right_edge - path_center).normalized; //FIXME: obsolete

		arc_left_up    = -Vector3.Cross(arc_left , path_normal).normalized; //just in case
		arc_right_down =  Vector3.Cross(arc_right, path_normal).normalized;
		
		arc_radius = (left_edge - path_center).magnitude; //or right_edge
		
		arc_angle = Vector3.Angle(arc_left, arc_right) * Mathf.PI / 180;

		if(Vector3.Dot(arc_left_up, arc_right) <= 0)
		{
			arc_angle += Mathf.PI;
		}

		angle_to_normal = Mathf.Acos(path_center.magnitude); //TODO: check

		next = this; prev = this;
		RecalculateAABB();
	}

	public void InitializeCorner(SphericalIsoscelesTrapezoid left, SphericalIsoscelesTrapezoid right) //TODO: please delegate in the future
	{
		path_center =  right.Evaluate(0); //or left.Evaluate(radius*angle)
		path_normal = -right.Evaluate(0);

		arc_left  = left.EvaluateNormal(left.arc_angle*left.arc_radius, 0); //FIXME: incorrect logic
		arc_right = right.EvaluateNormal(0, 0); //FIXME: incorrect

		arc_left_up    = -Vector3.Cross(arc_left , path_normal).normalized; //CHECK: probably right, but just in case
		arc_right_down =  Vector3.Cross(arc_right, path_normal).normalized;
		
		arc_radius = 0;
		
		arc_angle = Vector3.Angle(arc_left, arc_right) * Mathf.PI / 180;
		
		if(Vector3.Dot(arc_left_up, arc_right) <= 0)
		{
			arc_angle += Mathf.PI;
		}

		angle_to_normal = Mathf.Acos(path_center.magnitude); //TODO: check

		this.Relink(left, right);
		RecalculateAABB();
	}

	/** Find the point of collision as a parameterization of a circle.
	 *  
	 */
	public Optional<float> Intersect(Vector3 to, Vector3 from, float height) //TODO: FIXME: UNJANKIFY
	{
		Vector3 right  = Vector3.Cross(from, to);
		Vector3 secant = Vector3.Cross(path_normal, right);
		
		if(Vector3.Dot(secant, from) < 0) secant *= -1; //TODO: check

		secant.Normalize();

		Vector3 adjusted_center = path_center + path_normal*0; //FIXME: UBER-DJANK
		float   adjusted_radius = arc_radius  +				0; //FIXME: INCREDI-JJANK

		Vector3 intersection = adjusted_center + secant*adjusted_radius;

		float x = Vector3.Dot(intersection, arc_left   ) / adjusted_radius;
		float y = Vector3.Dot(intersection, arc_left_up) / adjusted_radius;
		
		float angle = Mathf.Atan2(y,x);

		if(angle < 0)
		{
			angle += 2*Mathf.PI;
		}

		if(angle <= arc_angle)
		{
			return angle*arc_radius; //there needs to be a mechanism for changing speed based on radius...
		}
		return new Optional<float>();
	}

	public SphericalIsoscelesTrapezoid LinkLeft(Vector3 pos)
	{
		Vector3 left = this.Evaluate(0);

		SphericalIsoscelesTrapezoid obj = SphericalIsoscelesTrapezoid.Spawn(pos, left, Vector3.Cross(pos, left));

		return obj.Relink(prev, this);
	}

	public SphericalIsoscelesTrapezoid LinkRight(Vector3 pos)
	{
		Vector3 right = this.Evaluate(arc_angle*arc_radius);

		SphericalIsoscelesTrapezoid obj = SphericalIsoscelesTrapezoid.Spawn(right, pos, Vector3.Cross(right, pos));

		return obj.Relink(this, next);
	}

	Vector3 MaxGradient(Vector3 desired)
	{
		Vector3 max_gradient = Vector3.zero;
		float max_product = Mathf.NegativeInfinity;

		/** if we don't calculate per quadrant, calculations for an arc with angle 2*PI become ambiguous because left == right
		 */ 
		float quadrants = Mathf.Ceil(arc_angle / (Mathf.PI / 2f)); //maximum of 4, always integral, float for casting "left" and "right"
		for(float quadrant = 0; quadrant < quadrants; ++quadrant)
		{
			float left  = arc_angle*arc_radius*( quadrant      / quadrants); //get beginning of quadrant i.e. 0.00,0.25,0.50,0.75
			float right = arc_angle*arc_radius*((quadrant + 1) / quadrants); //get    end    of quadrant i.e. 0.25,0.50,0.75,1.00
			
			float left_product  = Vector3.Dot(Evaluate(left) , desired); //find the correlation factor between left and the desired direction
			float right_product = Vector3.Dot(Evaluate(right), desired);

			/** this is basically a binary search
			 * 
			 *  1) take the left and right vectors and compute their dot products with the desired direction.
			 *  2) take the lesser dot product and ignore that half of the remaining arc
			 */
			for(int iteration = 0; iteration < 8*sizeof(float); ++iteration) //because we are dealing with floats, more precision could help (or hurt?)
			{
				float midpoint = (left + right) / 2;
				if(left_product < right_product) //is the right vector closer to the desired direction?
				{
					left = midpoint; //throw out the left half if the right vector is closer
					left_product = Vector3.Dot(Evaluate(left), desired);
				}
				else
				{
					right = midpoint; //throw out the right half if the left vector is closer
					right_product = Vector3.Dot(Evaluate(right), desired);
				}
			}
			
			/** figure out if this quadrant contains a larger gradient
			 */
			if(max_product < right_product)
			{
				max_gradient = Evaluate(right);
				max_product = right_product;
			}
			if(max_product < left_product)
			{
				max_gradient = Evaluate(left);
				max_product = left_product;
			}
		}
		return max_gradient;
	}
	
	private void OnDrawGizmos() //TODO: get rid of this in production builds
	{
		// draw floor path
		DrawArc(0.0f, Color.black);

		// draw CoM path
		DrawArc(0.05f, Color.white);

		DrawRadial(0, 0.1f, Color.red);

		DrawRadial(arc_angle*arc_radius, 0.1f, Color.blue);
	}

	public float Radius(float height)
	{
		Vector3 center = Center(height);
		Vector3 pos    = Evaluate(0, height);

		return (pos - center).magnitude;
	}

	/** Create a AABB that perfectly contains a circular arc
	 * 
	 *  TODO: detailed description and math link
	 * 
	 *  TODO: Ex. 
	 * 
	 *  @param collider the box collider that will be altered to contain the SphericalIsoscelesTrapezoid
	 */
	void RecalculateAABB()
	{
		BoxCollider	collider = this.GetComponent<BoxCollider>(); 

		float x_min = MaxGradient(Vector3.left   ).x;
		float x_max = MaxGradient(Vector3.right  ).x;
		float y_min = MaxGradient(Vector3.down   ).y;
		float y_max = MaxGradient(Vector3.up     ).y;
		float z_min = MaxGradient(Vector3.back   ).z;
		float z_max = MaxGradient(Vector3.forward).z;

		collider.center = new Vector3((x_max + x_min) / 2,
		                              (y_max + y_min) / 2,
		                              (z_max + z_min) / 2);

		collider.size   = new Vector3( x_max - x_min,
									   y_max - y_min,
									   z_max - z_min);
	}

	SphericalIsoscelesTrapezoid Relink(SphericalIsoscelesTrapezoid left, SphericalIsoscelesTrapezoid right)
	{
		this.next  = right;
		this.prev  = left;

		left.next  = this;
		right.prev = this;

		return this;
	}

	
	static SphericalIsoscelesTrapezoid Spawn()
	{
		//GameObject obj = (GameObject)Instantiate(Resources.Load("SphereIsoTrap")); ;
		GameObject prefab = (GameObject) Resources.Load("SphereIsoTrap");
		
		#if UNITY_EDITOR
		GameObject obj = UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as GameObject;
		#else
		GameObject obj = Instantiate(prefab) as GameObject;
		#endif

		obj.name = guid.ToString();

		guid++;
		
		return obj.GetComponent<SphericalIsoscelesTrapezoid>();
	}

	public static SphericalIsoscelesTrapezoid Spawn(Vector3 left_edge, Vector3 right_edge, Vector3 normal)
	{
		SphericalIsoscelesTrapezoid trapezoid = Spawn();
		
		trapezoid.Initialize(left_edge, right_edge, normal);
		
		return trapezoid; //used for next/prev
	}

	public static SphericalIsoscelesTrapezoid SpawnCorner(SphericalIsoscelesTrapezoid left, SphericalIsoscelesTrapezoid right)
	{
		SphericalIsoscelesTrapezoid trapezoid = Spawn();

		trapezoid.InitializeCorner(left, right);

		return trapezoid; //used for next/prev
	}
}