﻿/** The primative spherical geometry component that is used to traverse a block or terrain
 *
 * TODO: detailed description
 * 
 * @file
 */

using UnityEngine;
using System.Diagnostics;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class Edge /* : Component*/ : ArcOfSphere //TODO: get rid of this in production builds
{	
	[SerializeField] Vector3								path_normal;
	
	[SerializeField] Vector3								arc_left;
	[SerializeField] Vector3								arc_right; //CONSIDER: can be dropped
	
	[SerializeField] Vector3								arc_left_normal;
	[SerializeField] Vector3								arc_right_normal;
	
	[SerializeField] float									arc_angle; //the angle to sweep around the center.
	[SerializeField] float									angle_to_normal;
	
	public override float AngularRadius(float radius)
	{
		return angle_to_normal - radius;
	}
	
	public override float Begin(float radius) //TODO: optimize, avoid recursion if convenient
	{
		if(radius == 0) return 0; //CRISIS AVERTED: FIXME tho: 'tis JANK as fuck

		Vector3 left = prev.Evaluate(prev.End(radius), radius);
		Vector3 right = Evaluate(0, radius);
		return Vector3.Angle(left, right) * Mathf.PI / 180;
	}

    public override optional<float> CartesianToRadial(Vector3 position) //TODO: FIXME: UNJANKIFY //CHECK: the math could be harder than this //CONSIDER: http://gis.stackexchange.com/questions/48937/how-to-calculate-the-intersection-of-2-circles
    {
        position -= Center();

        float x = Vector3.Dot(position, arc_left);
        float y = Vector3.Dot(position, arc_left_normal);

        float angle = Mathf.Atan2(y, x);

        if (angle < 0)
        {
            angle += 2 * Mathf.PI;
        }

        if (angle <= arc_angle)
        {
            return angle;
        }

        return new optional<float>();
    }

    protected override Vector3 Center(float radius)
	{
		return path_normal * Mathf.Cos(AngularRadius(radius));
	}
	
	public override bool Contains(Vector3 pos, float radius)
	{
		bool bAboveGround = Vector3.Dot(pos - Center()      , path_normal) >= 0;
		bool bBelowCOM	  = Vector3.Dot(pos - Center(radius), path_normal) <= 0; //COM means center of mass
		bool bIsAtCorrectElevation = bAboveGround && bBelowCOM;

		bool bLeftContains		   = Vector3.Dot(pos - Evaluate(Begin()), arc_left_normal ) >= 0;
		bool bRightContains		   = Vector3.Dot(pos - Evaluate(  End()), arc_right_normal) >= 0;
		bool bIsObtuse			   = Vector3.Dot(arc_left, arc_right) >= 0;
		int  nOutOfThree		   = CountTrueBooleans(bLeftContains, bRightContains, bIsObtuse);

        DebugUtility.Log("above:", bAboveGround, "below:", bBelowCOM, "left:", bLeftContains, "right:", bRightContains, "obtuse:", bIsObtuse);
		
        return bIsAtCorrectElevation && nOutOfThree >= 2;
	}
	
	public override optional<float> Distance(Vector3 to, Vector3 from, float radius) //distance is Euclidean but is (guaranteed?) to be sorted correctly with the current assertions about speed vs player_radius
	{
        optional<Vector3> point = SphereUtility.Intersection(from, to, Center(radius), AngularRadius(radius));
        optional<float> intersection = new optional<float>();

        if (point.exists)
        {
            intersection = CartesianToRadial(point.data);
        }

        if (intersection.exists)
		{
			float angle = intersection.data;
			Vector3 new_position = Evaluate(angle, radius);
			return (from - new_position).sqrMagnitude; //CONSIDER: does Cartesian distance matter?
		}
		
		return new optional<float>();
	}
	
    #if UNITY_EDITOR
	void DrawArc(float radius, Color color)
	{
		UnityEditor.Handles.color = color;
		UnityEditor.Handles.DrawWireArc(Center(radius), path_normal, Evaluate(Begin(radius), radius) - Center(radius), (End(radius) - Begin(radius)) * 180 / Mathf.PI, LengthRadius(radius));
	}

    void DrawDefault()
	{	
		UnityEditor.Handles.color = Color.cyan;
		UnityEditor.Handles.DrawLine(Evaluate(Begin()), Evaluate(Begin()) + arc_left*.1f);
		
		UnityEditor.Handles.color = Color.magenta;
		UnityEditor.Handles.DrawLine(Evaluate(End()), Evaluate(End()) + arc_right*.1f);
		
		UnityEditor.Handles.color = Color.yellow;
		UnityEditor.Handles.DrawLine(Evaluate(Begin()), Evaluate(Begin()) + arc_left_normal*.1f);
		
		UnityEditor.Handles.color = Color.green;
		UnityEditor.Handles.DrawLine(Evaluate(End()), Evaluate(End()) + arc_right_normal*.1f);
	}
    
    void DrawRadial(float angle, float radius, Color color) //CONSIDER: use DrawWireArc
	{
		UnityEditor.Handles.color = color;
		UnityEditor.Handles.DrawLine(Evaluate(angle, 0), Evaluate(angle, radius)); 
	}
    #endif
	
	public override float End(float radius) //TODO: optimize, avoid recursion if convenient
	{
		if(radius == 0) return arc_angle; //CRISIS AVERTED: FIXME tho: 'tis JANK as fuck

		Vector3 left = Evaluate(arc_angle, radius);
		Vector3 right = next.Evaluate(next.Begin(radius), radius);
		return arc_angle - Vector3.Angle(left, right) * Mathf.PI / 180;
	}
	
	/** return the position of the player based on the circular path
	 *  
	 */
	public override Vector3 Evaluate(float angle, float radius)
	{
		return SphereUtility.SphereToCartesian(new Vector2(AngularRadius(radius), angle), arc_left, arc_left_normal, path_normal);
	}
	
	/** return the position of the player based on the circular path
	 * 
	 *  return the position of the player based on the circular path
	 *  If the player would go outside of [Begin(radius), End(radius)],
	 *  the arc should transfer control of the player to (prev, next) respectively
	 */
	public override Vector3 EvaluateNormal(float angle, float radius)
	{
        return Vector3.Slerp(arc_left, arc_right, angle / arc_angle) * Mathf.Cos(AngularRadius(radius)) + path_normal * Mathf.Sin(AngularRadius(radius)); //TODO: optimize?
    }
	
	public override Vector3 EvaluateRight(float angle, float radius) //TODO: optimize
	{
		return SphereUtility.SphereToCartesian(new Vector2(AngularRadius(radius), angle), arc_left_normal, -arc_left, path_normal);
	}
	
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
        #if UNITY_EDITOR
		this.Save();
        #endif
		
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
	
	public void Initialize(Vector3 center)
	{
		//DebugUtility.Assert(Mathf.Approximately(Vector3.Dot(right edge - left edge, normal), 0),
		//                    "ArcOfSphere: Initialize: failed assert");
		
		arc_left_normal  = -Vector3.Cross(arc_left , path_normal).normalized; //CHECK: probably right, but just in case
		arc_right_normal =  Vector3.Cross(arc_right, path_normal).normalized;
		
		arc_angle = Vector3.Angle(arc_left, arc_right) * Mathf.PI / 180;
		
		if(Vector3.Dot(arc_left_normal, arc_right) <= 0)
		{
			arc_angle += Mathf.PI;
		}
		
		angle_to_normal = Mathf.Acos(Mathf.Min(center.magnitude, 1)); //TODO: check
		
		RecalculateAABB(this);

        #if UNITY_EDITOR
		this.Save();
        #endif
	}

    public static bool IsConvex(Edge left, Edge right)
	{
		return Vector3.Dot(left.EvaluateNormal(left.End()), right.EvaluateRight(right.Begin())) < 0;
	}
	
	public override float LengthRadius(float radius)
	{
		Vector3 center   = Center(radius);
		Vector3 position = Evaluate(Begin(), radius);
		
		return (position - center).magnitude;
	}

	public static Edge LinkRight(ArcOfSphere left, Vector3 position)
	{
		Vector3 right = left.Evaluate(left.End());
		
		Edge obj = Edge.StartEdge(left.transform.parent.transform, right, position);

		obj.Relink(left, left.next);

		return obj;
	}
	
    #if UNITY_EDITOR
	private void OnDrawGizmos() //TODO: get rid of this in production builds //It's tedious that I can't just put this in the editor code
	{
		// draw floor path
		DrawArc(0.0f, Color.black);
		
		// draw CoM path
		//DrawArc(0.025f, Color.grey);
		
		// draw ceil path
		//DrawArc(0.05f, Color.white);
		
		DrawDefault();
	}
    #endif

    protected override Vector3 Pole()
    {
        return path_normal;
    }
    
    #if UNITY_EDITOR
    public override void Save()
	{
		base.Save();
		Undo.RecordObject(this, "Save edge");
	}
    #endif

	public static Edge StartEdge(Transform block_transform, Vector3 left_point, Vector3 right_point)
	{
		GameObject prefab = (GameObject) Resources.Load("EdgePrefab");
		
		#if UNITY_EDITOR
		GameObject obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        #else
		GameObject obj = Instantiate(prefab) as GameObject;
        #endif

        #if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(obj, "Created edge");
		#endif

		obj.name = "Edge";

		Edge result = obj.GetComponent<Edge>();

		result.Initialize(left_point, right_point);

		#if UNITY_EDITOR
        result.Save();
		result.LinkBlock(block_transform);
		#endif

		return result;
	}
}