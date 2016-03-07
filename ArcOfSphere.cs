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
public class ArcOfSphere /* : Component*/ : MonoBehaviour //TODO: get rid of this in production builds
{
	/*CONSIDER: make const*/
	[SerializeField] public ArcOfSphere		next; //CONSIDER: add k prefix
	[SerializeField] public ArcOfSphere		prev;
	
	[SerializeField] Vector3								path_normal;
	[SerializeField] Vector3								path_forward; //CONSIDER: how do things need to change?

	[SerializeField] Vector3								arc_left;
	[SerializeField] Vector3								arc_right; //CONSIDER: can be dropped

	[SerializeField] Vector3								arc_left_up; //FIXME: shitty names 
	[SerializeField] Vector3								arc_right_down; //FIXME

	[SerializeField] public float							arc_radius; //FIXME: temp hack public
	[SerializeField] public float							arc_angle; //the angle to sweep around the center. FIXME:
	[SerializeField] float									angle_to_normal;

	static int 												guid = 0;

	float Begin(float radius)
	{
		return 0; //FIXME: temporary
	}

	float Begin () { return Begin(0); }

	/** Find the center of a character path when the circle is extruded by the character's radius 
	 * 
	 */
	Vector3 Center(float radius)
	{
		return path_forward * Mathf.Cos(angle_to_normal - radius);
	}

	Vector3 Center() { return Center(0); }

	/** Determine if the character (represented by a point) is inside of a trapezoid (extruded by the radius of the player)
	 *  
	 */
	public bool Contains(Vector3 pos, float radius)
	{
		bool bAboveGround = Vector3.Dot(pos - Center(0)     , path_normal) >= 0;
		bool bBelowCOM	  = Vector3.Dot(pos - Center(radius), path_normal) <= 0;

		bool bIsAtCorrectElevation = bAboveGround && bBelowCOM; //FIXME: INFINI-JANK
		bool bLeftContains		   = Vector3.Dot(pos,  arc_left_up  ) >= 0;
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

	void DrawArc(float radius, Color color)
	{
		UnityEditor.Handles.color = color;
		UnityEditor.Handles.DrawWireArc(Center(radius), path_normal, arc_left*Radius(radius), arc_angle * 180 / Mathf.PI, Radius(radius));
	}

	void DrawDefault()
	{
		if(arc_radius > .1f) return;

		UnityEditor.Handles.color = Color.cyan;
		UnityEditor.Handles.DrawLine(Evaluate(Begin()), Evaluate(Begin()) + arc_left*.1f);

		UnityEditor.Handles.color = Color.magenta;
		UnityEditor.Handles.DrawLine(Evaluate(End()), Evaluate(End()) + arc_right*.1f);

		UnityEditor.Handles.color = Color.yellow;
		UnityEditor.Handles.DrawLine(Evaluate(Begin()), Evaluate(Begin()) + arc_left_up*.1f);

		UnityEditor.Handles.color = Color.green;
		UnityEditor.Handles.DrawLine(Evaluate(End()), Evaluate(End()) + arc_right_down*.1f);
	}

	void DrawRadial(float t, float radius, Color color)
	{
		UnityEditor.Handles.color = color;
		UnityEditor.Handles.DrawLine(Evaluate(t, 0), Evaluate(t, radius)); 
	}

	float End(float radius)
	{
		return arc_angle*arc_radius; //FIXME: duct tape solution; doesn't take variable player height into account for concave edges
	}

	float End () { return End (0); }

	/** return the position of the player based on the circular path
	 *  
	 */
	public Vector3 Evaluate(float t, float radius)
	{
		float angle = t / arc_radius; //FIXME: include radius

		return SphereUtility.Position(arc_left, arc_left_up, path_forward, angle_to_normal - radius, angle);
	}

	/** return the position of the player based on the circular path
	 *  
	 */
	public Vector3 Evaluate(float t) { return Evaluate(t, 0); }

	/** return the position of the player based on the circular path
	 * 
	 *  return the position of the player based on the circular path
	 *  If the player would go outside of [Begin(radius), End(radius)],
	 *  the Trapezoid should transfer control of the player to (prev, next) respectively
	 */
	public static Vector3 Evaluate(ref float t, float radius, ref ArcOfSphere seg)
	{
		if(t > seg.End(radius))
		{
			t -= seg.End(radius);
			seg = seg.next;
			t += seg.Begin(radius);
			return Evaluate(ref t, 0.01f, ref seg);
		}
		if(t < seg.Begin(radius))
		{
			t -= seg.Begin(radius);
			seg = seg.prev;
			t += seg.End(radius);
			return Evaluate(ref t, 0.01f, ref seg);
		}
		
		return seg.Evaluate(t, radius);
	}

	public Vector3 EvaluateNormal(float t, float radius)
	{
		float angle = t / arc_radius;

		return SphereUtility.Normal(arc_left, arc_left_up, path_normal, angle_to_normal - radius, angle);
	}

	public Vector3 EvaluateRight(float t, float radius)
	{
		float angle = t / arc_radius;
		return SphereUtility.Position(arc_left_up, -arc_left, path_forward, angle_to_normal - radius, angle);
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
	public void Initialize(Vector3 left_edge, Vector3 right_edge, Vector3 normal)
	{
		path_normal  = normal.normalized;
		path_forward = normal.normalized;
		Vector3 path_center = path_normal*Vector3.Dot(left_edge, path_normal); //or right_edge

		arc_left  = (left_edge  - path_center).normalized; //FIXME: obsolete for corner triangles
		arc_right = (right_edge - path_center).normalized; //FIXME: obsolete

		arc_radius = (left_edge - path_center).magnitude; //or right_edge

		Initialize(path_center);

		next = this; prev = this;
	}

	public void InitializeCorner(ArcOfSphere left, ArcOfSphere right)
	{
		Vector3 path_center = right.Evaluate(0,0);
		path_normal  = -right.Evaluate(0,0);
		path_forward =  right.Evaluate(0,0); 


		arc_left  = left.EvaluateNormal(left.End(0), 0);
		arc_right = right.EvaluateNormal(right.Begin(0), 0);

		arc_radius = 1e-36f;//0; //FIXME: make zero; magic numbers aren't ideal
		
		Initialize(path_center);

		this.Relink(left, right);
	}

	public void Initialize(Vector3 center)
	{
		//DebugUtility.Assert(Mathf.Approximately(Vector3.Dot(right edge - left edge, normal), 0),
		//                    "SphericalIsoscelesTrapezoid: Initialize: failed assert");

		arc_left_up    = -Vector3.Cross(arc_left , path_normal).normalized; //CHECK: probably right, but just in case
		arc_right_down =  Vector3.Cross(arc_right, path_normal).normalized;

		arc_angle = Vector3.Angle(arc_left, arc_right) * Mathf.PI / 180; //FIXME: logic error
		
		if(Vector3.Dot(arc_left_up, arc_right) <= 0)
		{
			arc_angle += Mathf.PI;
		}

		angle_to_normal = Mathf.Acos(Mathf.Min(center.magnitude, 1)); //TODO: check

		RecalculateAABB();
	}

	/** Find the point of collision as a parameterization of a circle.
	 *  
	 */
	public Optional<float> Intersect(Vector3 to, Vector3 from, float radius) //TODO: FIXME: UNJANKIFY //CHECK: the math could be harder than this
	{
		Vector3 right  = Vector3.Cross(from, to);
		Vector3 secant = Vector3.Cross(path_normal, right);
		
		if(Vector3.Dot(secant, from) < 0) secant *= -1; //TODO: check

		secant.Normalize();

		Vector3 intersection = Center(radius) + secant*Radius(radius);

		float x = Vector3.Dot(intersection, arc_left   ) / Radius(radius);
		float y = Vector3.Dot(intersection, arc_left_up) / Radius(radius);
		
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

	public ArcOfSphere LinkLeft(Vector3 pos)
	{
		Vector3 left = this.Evaluate(Begin());

		ArcOfSphere obj = ArcOfSphere.Spawn(pos, left, Vector3.Cross(pos, left));

		return obj.Relink(prev, this);
	}

	public ArcOfSphere LinkRight(Vector3 pos)
	{
		Vector3 right = this.Evaluate(End());

		ArcOfSphere obj = ArcOfSphere.Spawn(right, pos, Vector3.Cross(right, pos));

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
			float left  = End()*( quadrant      / quadrants); //get beginning of quadrant i.e. 0.00,0.25,0.50,0.75
			float right = End()*((quadrant + 1) / quadrants); //get    end    of quadrant i.e. 0.25,0.50,0.75,1.00
			
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
		DrawArc(0.025f, Color.grey);

		// draw ceil path
		DrawArc(0.05f, Color.white);

		DrawRadial(Begin(0.05f), 0.05f, Color.red);

		DrawRadial(End(0.05f), 0.05f, Color.blue);

		DrawDefault();
	}

	public float Radius(float radius)
	{
		Vector3 center = Center(radius);
		Vector3 pos    = Evaluate(0, radius);

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

		float x_min = MaxGradient(Vector3.left   ).x - 1e-6f;
		float x_max = MaxGradient(Vector3.right  ).x + 1e-6f;
		float y_min = MaxGradient(Vector3.down   ).y - 1e-6f;
		float y_max = MaxGradient(Vector3.up     ).y + 1e-6f;
		float z_min = MaxGradient(Vector3.back   ).z - 1e-6f;
		float z_max = MaxGradient(Vector3.forward).z + 1e-6f;

		transform.position = new Vector3((x_max + x_min) / 2,
		                              (y_max + y_min) / 2,
		                              (z_max + z_min) / 2);

		collider.size   = new Vector3( x_max - x_min,
									   y_max - y_min,
									   z_max - z_min);
	}

	ArcOfSphere Relink(ArcOfSphere left, ArcOfSphere right)
	{
		this.next  = right;
		this.prev  = left;

		left.next  = this;
		right.prev = this;

		return this;
	}

	
	static ArcOfSphere Spawn()
	{
		GameObject prefab = (GameObject) Resources.Load("SphereIsoTrap");
		
		#if UNITY_EDITOR
		GameObject obj = UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as GameObject;
		#else
		GameObject obj = Instantiate(prefab) as GameObject;
		#endif

		obj.name = guid.ToString();

		guid++;
		
		return obj.GetComponent<ArcOfSphere>();
	}

	public static ArcOfSphere Spawn(Vector3 left_edge, Vector3 right_edge, Vector3 normal)
	{
		ArcOfSphere trapezoid = Spawn();
		
		trapezoid.Initialize(left_edge, right_edge, normal);
		
		return trapezoid; //used for next/prev
	}

	public static ArcOfSphere SpawnCorner(ArcOfSphere left, ArcOfSphere right)
	{
		ArcOfSphere trapezoid = Spawn();

		trapezoid.InitializeCorner(left, right);

		return trapezoid; //used for next/prev
	}
}