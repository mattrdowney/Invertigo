/** The primative spherical geometry component that is used to traverse a block or terrain
 *
 * TODO: detailed description
 * 
 * @file
 */

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;

[System.Serializable]
public class ArcOfSphere /* : Component*/ : MonoBehaviour //TODO: get rid of this in production builds
{
	/*CONSIDER: make const*/
	[SerializeField] public ArcOfSphere						next;
	[SerializeField] public ArcOfSphere						prev;

	[SerializeField] Vector3								path_normal;

	[SerializeField] Vector3								arc_left;
	[SerializeField] Vector3								arc_right; //CONSIDER: can be dropped

	[SerializeField] Vector3								arc_left_normal;
	[SerializeField] Vector3								arc_right_normal;
	
	[SerializeField] float									arc_angle; //the angle to sweep around the center.
	[SerializeField] float									angle_to_normal;
	[SerializeField] float									radius_sign;

	public static int 										guid = 0; //TODO: remove this eventually

	public float Begin(float radius)
	{
		return 0; //FIXME: temporary
	}

	public float Begin () { return Begin(0); }

	/** Find the center of a character path when the circle is extruded by the character's radius 
	 * 
	 */
	Vector3 Center(float radius)
	{
		return path_normal * Mathf.Cos(angle_to_normal - radius_sign*radius);
	}

	Vector3 Center() { return Center(0); }

	/** Determine if the character (represented by a point) is inside of a arc (extruded by the radius of the player)
	 *  
	 */
	public bool Contains(Vector3 pos, float radius)
	{
		bool bAboveGround = Vector3.Dot(pos - Center()      , path_normal) >= 0;
		bool bBelowCOM	  = Vector3.Dot(pos - Center(radius), path_normal) <= 0;

		bool bIsAtCorrectElevation = bAboveGround && bBelowCOM;
		bool bLeftContains		   = Vector3.Dot(pos,  arc_left_normal  ) >= 0;
		bool bRightContains		   = Vector3.Dot(pos, arc_right_normal) >= 0;
		bool bIsObtuse			   = Vector3.Dot(arc_left, arc_right) <= 0;
		int  nOutOfThree		   = CountTrueBooleans(bLeftContains, bRightContains, bIsObtuse);

		return bIsAtCorrectElevation && nOutOfThree >= 2; //XXX: might even now be wrong
	}

	/** Counts the number of booleans that are true in a comma separated list of booleans
	 * 
	 *  credit: http://stackoverflow.com/questions/377990/elegantly-determine-if-more-than-one-boolean-is-true
	 * 
	 *  @example "CountTrueBooleans(true, false, true, true);" will return 3
	 */
	static int CountTrueBooleans(params bool[] boolean_list) //allow for comma separated booleans using "params"
	{
		return boolean_list.Count(bIsTrue => bIsTrue); //count booleans that are true using Linq's .Count function
	}

	public optional<float> Distance(Vector3 to, Vector3 from, float radius)
	{
		optional<float> intersection = Intersect(to, from, radius);

		Debug.Log(intersection.exists);

		if(intersection.exists)
		{
			float angle = intersection.data;
			Vector3 new_position = Evaluate(angle);
			return Vector3.Distance(from, new_position); //FIXME: Accidentally used Cartesian distance!
		}
		
		return new optional<float>();
	}

	public ArcOfSphere DivideEdge(Vector3 division_point)
	{
		DebugUtility.Assert(Radius() != 0, "Trying to divide corner!");
		
		ArcOfSphere left_corner  = prev;
		ArcOfSphere right_corner = next;
		
		ArcOfSphere arc = left_corner.LinkRight(division_point);
		
		Initialize(division_point, Evaluate(End()));
		
		ArcOfSphere corner = ArcOfSphere.SpawnCorner(arc, this);
		
		left_corner .InitializeCorner(left_corner .prev, left_corner .next);
		right_corner.InitializeCorner(right_corner.prev, right_corner.next);
		
		return this; //not really necessary
	}

	void DrawArc(float radius, Color color)
	{
		UnityEditor.Handles.color = color;
		UnityEditor.Handles.DrawWireArc(Center(radius), radius_sign*path_normal, arc_left*Radius(radius), arc_angle * 180 / Mathf.PI, Radius(radius));
	}

	void DrawDefault()
	{
		if(Radius() > 0) return;

		UnityEditor.Handles.color = Color.cyan;
		UnityEditor.Handles.DrawLine(Evaluate(Begin()), Evaluate(Begin()) + arc_left*.1f);

		UnityEditor.Handles.color = Color.magenta;
		UnityEditor.Handles.DrawLine(Evaluate(End()), Evaluate(End()) + arc_right*.1f);

		UnityEditor.Handles.color = Color.yellow;
		UnityEditor.Handles.DrawLine(Evaluate(Begin()), Evaluate(Begin()) + arc_left_normal*.1f);

		UnityEditor.Handles.color = Color.green;
		UnityEditor.Handles.DrawLine(Evaluate(End()), Evaluate(End()) + arc_right_normal*.1f);
	}

	void DrawRadial(float angle, float radius, Color color)
	{
		UnityEditor.Handles.color = color;
		UnityEditor.Handles.DrawLine(Evaluate(angle, 0), Evaluate(angle, radius)); 
	}

	public float End(float radius)
	{
		return arc_angle; //FIXME: duct tape solution; doesn't take variable player height into account for concave edges
	}

	public float End () { return End (0); }

	/** return the position of the player based on the circular path
	 *  
	 */
	public Vector3 Evaluate(float angle, float radius)
	{
		return SphereUtility.Position(arc_left, arc_left_normal, path_normal, angle_to_normal - radius_sign*radius, angle);
	}

	public Vector3 Evaluate(float angle) { return Evaluate(angle, 0); }

	/** return the position of the player based on the circular path
	 * 
	 *  return the position of the player based on the circular path
	 *  If the player would go outside of [Begin(radius), End(radius)],
	 *  the arc should transfer control of the player to (prev, next) respectively
	 */
	public static Vector3 Evaluate(ref float angle, float radius, ref ArcOfSphere arc)
	{
		if(angle >= arc.End(radius))
		{
			angle -= arc.End(radius);
			arc = arc.next;
			angle += arc.Begin(radius);
			return Evaluate(ref angle, radius, ref arc);
		}
		if(angle < arc.Begin(radius)) //Consider: when was this an error; shame on me
		{
			angle -= arc.Begin(radius);
			arc = arc.prev;
			angle += arc.End(radius);
			return Evaluate(ref angle, radius, ref arc);
		}
		
		return arc.Evaluate(angle);
	}

	public Vector3 EvaluateNormal(float angle, float radius)
	{
		return SphereUtility.Normal(arc_left, arc_left_normal, path_normal, angle_to_normal - radius_sign*radius, angle);
	}

	public Vector3 EvaluateNormal(float angle) { return EvaluateNormal(angle, 0); }

	public Vector3 EvaluateRight(float angle, float radius)
	{
		return SphereUtility.Position(arc_left_normal, -arc_left, path_normal, angle_to_normal - radius_sign*radius, angle);
	}

	public Vector3 EvaluateRight(float angle) { return EvaluateRight(angle, 0); }

	/** Recompute the orientation of a SphericalIsoscelesarc
	 * 
	 *  Destroys all information other than prev, next. Replaces this information with the information for traversing
	 *      the top of a SphericalIsoscelesarc on a unit sphere.
	 * 
	 * @param left_edge: the left-bottom point (left implies it is the 1st point when enumerated clockwise for concave objects,
	 * 		  bottom implies it is the position of the player's feet)
	 * @param right_edge: the right-bottom point (right implies it is the 2nd point when enumerated clockwise for concave objects,
	 * 		  bottom implies it is the position of the player's feet)
	 * @param normal: the normal plane that intersects lhs and rhs and forms the walking path for the players center
	 * 		  of mass, sign matters because it indicates which direction is up for calculating the center of mass.
	 * 
	 * @example Initialize(Vector3(0,0,1), Vector3(1,0,0), Vector3(0,1,0)) will initialize a Spherical Isosceles arc
	 *          that is a great circle for the feet positions, a large lesser circle for the center of mass position,
	 *          with a 90 degree arc going from forwards to right and a normal going in the positive y-direction.
	 */
	public void Initialize(Vector3 left_edge, Vector3 right_edge, Vector3 normal)
	{
		Save(this);

		radius_sign = +1;

		path_normal = normal.normalized;
		Vector3 path_center = path_normal*Vector3.Dot(left_edge, path_normal); //or right_edge

		arc_left  = (left_edge  - path_center).normalized;
		arc_right = (right_edge - path_center).normalized;

		Initialize(path_center);

		next = this; prev = this;
	}

	public void Initialize(Vector3 left_edge, Vector3 right_edge)
	{
		Initialize(left_edge, right_edge, Vector3.Cross(left_edge, right_edge));
	}

	public void InitializeCorner(ArcOfSphere left, ArcOfSphere right)
	{
		Save(this);

		radius_sign = -1;

		Vector3 path_center = right.Evaluate(right.Begin());
		path_normal =  right.Evaluate(right.Begin()); 

		arc_left  = left.EvaluateNormal(left.End());
		arc_right = right.EvaluateNormal(right.Begin());
		
		Initialize(path_center);

		this.Relink(left, right);
	}

	public void Initialize(Vector3 center)
	{
		//DebugUtility.Assert(Mathf.Approximately(Vector3.Dot(right edge - left edge, normal), 0),
		//                    "ArcOfSphere: Initialize: failed assert");

		arc_left_normal  = -Vector3.Cross(arc_left , radius_sign*path_normal).normalized; //CHECK: probably right, but just in case
		arc_right_normal =  Vector3.Cross(arc_right, radius_sign*path_normal).normalized;

		arc_angle = Vector3.Angle(arc_left, arc_right) * Mathf.PI / 180;
		
		if(Vector3.Dot(arc_left_normal, arc_right) <= 0)
		{
			arc_angle += Mathf.PI;
		}

		angle_to_normal = Mathf.Acos(Mathf.Min(center.magnitude, 1)); //TODO: check

		RecalculateAABB();
	}

	/** Find the point of collision as a parameterization of a circle.
	 *  
	 *  Thoughts: projecting all points onto the plane defined by normal "path_forward" would make the math simple-ish
	 */
	public optional<float> Intersect(Vector3 to, Vector3 from, float radius) //TODO: FIXME: UNJANKIFY //CHECK: the math could be harder than this
	{
		Vector3 right  = Vector3.Cross(from, to);
		Vector3 secant = Vector3.Cross(path_normal, right);
		
		if(Vector3.Dot(secant, from) < 0) secant *= -1; //TODO: check

		secant.Normalize();

		Vector3 intersection = Center(radius) + secant*Radius(radius); 

		float x = Vector3.Dot(intersection, arc_left   ) / Radius(radius);
		float y = Vector3.Dot(intersection, arc_left_normal) / Radius(radius);
		
		float angle = Mathf.Atan2(y,x);

		if(angle < 0)
		{
			angle += 2*Mathf.PI;
		}

		if(angle <= arc_angle)
		{
			return angle;
		}
		return new optional<float>();
	}

	public void LinkBlock(Transform block_transform)
	{
		Undo.SetTransformParent(this.transform, block_transform, "Link arc to block");
		//this.transform.parent = block_transform;
	}

	public void LinkBlock(ArcOfSphere other)
	{
		LinkBlock(other.gameObject.transform.parent);
	}

	public ArcOfSphere LinkRight(Vector3 pos)
	{
		Vector3 right = this.Evaluate(End());

		ArcOfSphere obj = ArcOfSphere.Spawn(right, pos);

		obj.LinkBlock(this);

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
	
	private void OnDrawGizmos() //TODO: get rid of this in production builds //It's tedious that I can't just put this in the editor code
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
		Vector3 center   = Center(radius);
		Vector3 position = Evaluate(Begin(), radius);

		return (position - center).magnitude;
	}

	public float Radius() { return Radius(0); }

	/** Create a AABB that perfectly contains a circular arc
	 * 
	 *  TODO: detailed description and math link
	 * 
	 *  TODO: Ex. 
	 * 
	 *  @param collider the box collider that will be altered to contain the ArcOfSphere
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

		transform.position = new Vector3((x_max + x_min) / 2,
		                    			 (y_max + y_min) / 2,
		                              	 (z_max + z_min) / 2);

		collider.size   = new Vector3(x_max - x_min, 
		                              y_max - y_min,
		                              z_max - z_min);
	}

	ArcOfSphere Relink(ArcOfSphere left, ArcOfSphere right)
	{
		Save(this);
		Save(left);
		Save(right);

		this.next  = right;
		this.prev  = left;

		left.next  = this;
		right.prev = this;

		return this;
	}

	public ArcOfSphere RemoveCorner()
	{
		DebugUtility.Assert(Radius() == 0, "Trying to remove edge!");

		ArcOfSphere left_edge  = prev;
		ArcOfSphere right_edge = next;
		ArcOfSphere left_corner  = left_edge.prev;
		ArcOfSphere right_corner = right_edge.next;

		Save(left_edge);
		Save(left_corner);
		Save(right_corner);

		Vector3 begin = left_edge.Evaluate(left_edge.Begin());
		Vector3 end   = right_edge.Evaluate(right_edge.End());

		left_edge.Relink(left_corner, right_corner);
		left_edge.Initialize(begin, end);

		Undo.DestroyObjectImmediate(this.gameObject);
		Undo.DestroyObjectImmediate(right_edge.gameObject);
		//DestroyImmediate(this.gameObject);
		//DestroyImmediate(right_edge.gameObject);

		left_corner .InitializeCorner(left_corner .prev, left_corner .next);
		right_corner.InitializeCorner(right_corner.prev, right_corner.next);
		
		return left_edge;
	}

	static void Save(ArcOfSphere arc)
	{
		Undo.RecordObject(arc, "Save arc of sphere");
		Undo.RecordObject(arc.transform, "Save transform");
		Undo.RecordObject(arc.GetComponent<BoxCollider>(), "Save box collider");
	}

	static ArcOfSphere Spawn()
	{
		GameObject prefab = (GameObject) Resources.Load("ArcOfSpherePrefab");
		
		#if UNITY_EDITOR
		GameObject obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
		#else
		GameObject obj = Instantiate(prefab) as GameObject;
		#endif

		Undo.RegisterCreatedObjectUndo(obj, "Created arc");

		obj.name = guid.ToString();

		guid++;
		
		return obj.GetComponent<ArcOfSphere>();
	}

	public static ArcOfSphere Spawn(Vector3 left_edge, Vector3 right_edge, Vector3 normal)
	{
		ArcOfSphere arc = Spawn();
		
		arc.Initialize(left_edge, right_edge, normal);
		
		return arc; //used for next/prev
	}

	public static ArcOfSphere Spawn(Vector3 left_edge, Vector3 right_edge)
	{
		return Spawn(left_edge, right_edge, Vector3.Cross(left_edge, right_edge));
	}

	public static ArcOfSphere SpawnCorner(ArcOfSphere left, ArcOfSphere right)
	{
		ArcOfSphere arc = Spawn();

		arc.InitializeCorner(left, right);

		arc.LinkBlock(left);

		return arc; //used for next/prev
	}

	public static ArcOfSphere StartShape(Vector3 left_edge, Vector3 right_edge, Transform block_transform)
	{
		ArcOfSphere arc0 = Spawn();
		arc0.Initialize(left_edge, right_edge);

		ArcOfSphere arc1 = arc0.LinkRight(left_edge);

		SpawnCorner(arc0, arc1).LinkBlock(block_transform);
		SpawnCorner(arc1, arc0).LinkBlock(block_transform);
		arc0.LinkBlock(block_transform);
		arc1.LinkBlock(block_transform);

		return arc1;
	}
}