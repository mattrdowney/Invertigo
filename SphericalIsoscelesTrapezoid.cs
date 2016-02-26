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
	[SerializeField] public float							arc_angle; //FIXME:

	static int 												guid = 0;

	/** Determine if the character (represented by a point) is inside of a trapezoid (extruded by the radius of the player)
	 *  
	 */
	public bool Contains(Vector3 pos, float radius)
	{
		float prod = Vector3.Dot(pos - path_center, path_normal);

		bool bIsAtCorrectElevation = 0 <= prod && prod <= radius*Mathf.Sin(radius); //TODO: radius might have changed logic
		bool bLeftContains		   = Vector3.Dot(pos, arc_left_up ) >= 0;
		bool bRightContains		   = Vector3.Dot(pos, arc_right_down) >= 0;
		bool bIsObtuse			   = Vector3.Dot(arc_left, arc_right) <= 0;
		int  nOutOfThree		   = Truth(bLeftContains, bRightContains, bIsObtuse);

		return bIsAtCorrectElevation && nOutOfThree >= 2; //XXX: might even now be wrong
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
		// draw CoM path
		UnityEditor.Handles.color = color;
		Vector3 adjusted_center = path_center + path_normal*(height*Mathf.Sin(height));
		float   adjusted_radius = arc_radius - (height - height*Mathf.Cos(height));
		Vector3 adjusted_from   = adjusted_center + arc_left*adjusted_radius;
		UnityEditor.Handles.DrawWireArc(adjusted_center, path_normal, adjusted_from, arc_angle * 180 / Mathf.PI, adjusted_radius);
		
		//DebugUtility.Print(adj_from.magnitude.ToString(), 100);
	}

	void DrawRadial(float t, Color color)
	{
		UnityEditor.Handles.color = color;
		UnityEditor.Handles.DrawLine(path_center, Evaluate(t)); 
	}

	/** return the position of the player based on the circular path
	 *  
	 */
	public Vector3 Evaluate(float t, float height)
	{
		float angle = t / arc_radius; //FIXME: factor in height

		Vector3 adjusted_center = path_center + path_normal*height*     Mathf.Sin(height); //TODO: check, this might be a hell of a lot more complicated when:
		float   adjusted_radius = arc_radius  -             height*(1 - Mathf.Cos(height)); //two adjacent SphereIsoTraps are not perpendicular. Might be necessary to do this: http://gis.stackexchange.com/questions/48937/how-to-calculate-the-intersection-of-2-circles

		Vector3 horizontal = adjusted_radius*Mathf.Cos(angle)*arc_left;
		Vector3 vertical   = adjusted_radius*Mathf.Sin(angle)*arc_left_up;

		return horizontal + vertical + adjusted_center;
	}

	/** return the position of the player based on the circular path
	 *  
	 */
	public Vector3 Evaluate(float t) { return Evaluate(t, 0f); }

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
		return Vector3.Cross(right, pos);
	}

	public Vector3 EvaluateRight(float t)
	{
		float angle = t / arc_radius;
		return arc_left_up*Mathf.Cos(angle) + arc_left*Mathf.Sin(angle);
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
		//DebugUtility.Assert(Mathf.Approximately(Vector3.Dot(right_edge - left_edge, normal), 0),
		//                    "SphericalIsoscelesTrapezoid: Initialize: failed assert");
		
		path_normal = normal.normalized; //when at first you don't believe it, pry, pry again
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

		next = prev = this;
		RecalculateAABB();
	}

	public void InitializeCorner(SphericalIsoscelesTrapezoid left, SphericalIsoscelesTrapezoid right)
	{
		path_center =  right.Evaluate(0);
		path_normal = -right.Evaluate(0);

		arc_left  = left.arc_right;
		arc_right = right.arc_left;

		arc_left_up    = -Vector3.Cross(arc_left , path_normal).normalized; //just in case
		arc_right_down =  Vector3.Cross(arc_right, path_normal).normalized;
		
		arc_radius = 0f;
		
		arc_angle = Vector3.Angle(arc_left, arc_right) * Mathf.PI / 180;
		
		if(Vector3.Dot(arc_left_up, arc_right) <= 0)
		{
			arc_angle += Mathf.PI;
		}

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
		
		if(Vector3.Dot(secant, from) < 0f) secant *= -1; //TODO: check

		secant.Normalize();

		Vector3 adjusted_center = path_center + path_normal*(height*Mathf.Sin(height)); //TODO: check
		float   adjusted_radius = arc_radius - height*(1 - Mathf.Cos(height)); //TODO: check 

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
		for(float quadrant = 0f; quadrant < quadrants; ++quadrant)
		{
			float left  = arc_angle*arc_radius*( quadrant        / quadrants); //get beginning of quadrant i.e. 0.00,0.25,0.50,0.75
			float right = arc_angle*arc_radius*((quadrant + 1f ) / quadrants); //get    end    of quadrant i.e. 0.25,0.50,0.75,1.00
			
			float left_product  = Vector3.Dot(Evaluate(left) , desired); //find the correlation factor between left and the desired direction
			float right_product = Vector3.Dot(Evaluate(right), desired);

			/** this is basically a binary search
			 * 
			 *  1) take the left and right vectors and compute their dot products with the desired direction.
			 *  2) take the lesser dot product and ignore that half of the remaining arc
			 */
			for(int iteration = 0; iteration < 8*sizeof(float); ++iteration) //because we are dealing with floats, more precision could help (or hurt?)
			{
				float midpoint = (left + right) / 2f;
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
		DrawArc(0.1f, Color.white);

		// draw left edge
		DrawRadial(0.0f, Color.red);

		// draw right edge
		DrawRadial(arc_angle*arc_radius, Color.blue);
	}

	/** Create a AABB that perfectly contains a circular arc
	 * 
	 *  TODO: detailed description and math link
	 * 
	 *  TODO: Ex. 
	 * 
	 *  @param collider the box collider that will be altered to contain the SphericalIsoscelesTrapezoid
	 */
	public void RecalculateAABB()
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

	public SphericalIsoscelesTrapezoid Relink(SphericalIsoscelesTrapezoid left, SphericalIsoscelesTrapezoid right)
	{
		this.next  = right;
		this.prev  = left;

		left.next  = this;
		right.prev = this;

		return this;
	}

	
	public static SphericalIsoscelesTrapezoid Spawn()
	{
		GameObject prefab = (GameObject) Resources.Load("SphereIsoTrap");
		
		#if UNITY_EDITOR
		GameObject obj = UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as GameObject;
		#else
		GameObject obj = Instantiate(prefab) as GameObject;
		#endif

		prefab.name = guid.ToString();

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

	/** Counts the number of booleans that are true in a comma separated list of booleans
	 * 
	 *  credit: http://stackoverflow.com/questions/377990/elegantly-determine-if-more-than-one-boolean-is-true
	 */
	public static int Truth(params bool[] booleans)
	{
		return booleans.Count(b => b);
	}
}