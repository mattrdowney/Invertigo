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
public class ConvexCorner /* : Component*/ : ArcOfSphere //TODO: get rid of this in production builds
{	
	[SerializeField] Vector3								path_normal;
	
	[SerializeField] Vector3								arc_left;
	[SerializeField] Vector3								arc_right; //CONSIDER: can be dropped
	
	[SerializeField] Vector3								arc_left_normal;
	[SerializeField] Vector3								arc_right_normal;
	
	[SerializeField] float									arc_angle; //the angle to sweep around the center.
	
	public override float AngularRadius(float radius)
	{
		return radius;
	}
	
	public override float Begin(float radius)
	{
		return 0;
	}

	protected override Vector3 Center(float radius)
	{
		return path_normal * Mathf.Cos(AngularRadius(radius));
	}

	public override bool Contains(Vector3 pos, float radius)
	{
		bool bAboveGround = Vector3.Dot(pos - Center(radius), path_normal) >= 0;
		bool bBelowCOM	  = Vector3.Dot(pos - Center()      , path_normal) <= 0; //COM means center of mass
		bool bIsAtCorrectElevation = bAboveGround && bBelowCOM;

		bool bLeftContains	= Vector3.Dot(pos,  arc_left_normal  ) >= 0;
		bool bRightContains	= Vector3.Dot(pos, arc_right_normal) >= 0;
		bool bCorrectAngle	= bLeftContains && bRightContains;
		
		return bIsAtCorrectElevation && bCorrectAngle;
	}
	
	public override optional<float> Distance(Vector3 to, Vector3 from, float radius) //distance is Euclidean but is (guaranteed?) to be sorted correctly with the current assertions about speed vs player_radius
	{
		optional<float> intersection = Intersect(to, from, radius);
		
		if(intersection.exists)
		{
			float angle = intersection.data;
			Vector3 new_position = Evaluate(angle, radius);
			return (from - new_position).sqrMagnitude; //CONSIDER: does Cartesian distance matter?
		}
		
		return new optional<float>();
	}
	
	void DrawArc(float radius, Color color)
	{
		UnityEditor.Handles.color = color;
		UnityEditor.Handles.DrawWireArc(Center(radius), -path_normal, arc_left*LengthRadius(radius), arc_angle * 180 / Mathf.PI, LengthRadius(radius));
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
	
	public override float End(float radius)
	{
		return arc_angle;
	}
	
	/** return the position of the player based on the circular path
	 *  
	 */
	public override Vector3 Evaluate(float angle, float radius)
	{
		return SphereUtility.Position(arc_left, arc_left_normal, path_normal, AngularRadius(radius), angle);
	}
	
	public override Vector3 EvaluateNormal(float angle, float radius)
	{
		return -SphereUtility.Normal(arc_left, arc_left_normal, path_normal, AngularRadius(radius), angle);
	}
	
	public override Vector3 EvaluateRight(float angle, float radius)
	{
		return SphereUtility.Position(arc_left_normal, -arc_left, path_normal, Mathf.PI / 2 - AngularRadius(radius), angle);
	}
	
	public void Initialize(ArcOfSphere left, ArcOfSphere right)
	{
		this.Save();
		
		Vector3 path_center = right.Evaluate(right.Begin());

		//Debug.DrawRay(path_center, Vector3.up, Color.yellow);

		path_normal 		= right.Evaluate(right.Begin()); 
		
		arc_left  = left.EvaluateNormal(left.End());
		arc_right = right.EvaluateNormal(right.Begin());
		
		Initialize(path_center);
		
		this.Relink(left, right);
	}
	
	public void Initialize(Vector3 center)
	{
		//DebugUtility.Assert(Mathf.Approximately(Vector3.Dot(right edge - left edge, normal), 0),
		//                    "ArcOfSphere: Initialize: failed assert");
		
		arc_left_normal  = -Vector3.Cross(arc_left , -path_normal).normalized; //CHECK: probably right, but just in case
		arc_right_normal =  Vector3.Cross(arc_right, -path_normal).normalized;
		
		arc_angle = Vector3.Angle(arc_left, arc_right) * Mathf.PI / 180;
		
		if(Vector3.Dot(arc_left_normal, arc_right) <= 0)
		{
			arc_angle += Mathf.PI;
		}
		
		RecalculateAABB(this);

		this.Save();
	}
	
	/** Find the point of collision as a parameterization of a circle.
	 *  
	 *  Thoughts: projecting all points onto the plane defined by normal "path_forward" would make the math simple-ish
	 */
	public override optional<float> Intersect(Vector3 to, Vector3 from, float radius) //TODO: FIXME: UNJANKIFY //CHECK: the math could be harder than this //CONSIDER: http://gis.stackexchange.com/questions/48937/how-to-calculate-the-intersection-of-2-circles
	{
		Vector3 intersection = SphereUtility.Intersection(from, to, path_normal, AngularRadius(radius)); 
		
		float x = Vector3.Dot(intersection, arc_left   ) / LengthRadius(radius); //TODO: optimize
		float y = Vector3.Dot(intersection, arc_left_normal) / LengthRadius(radius);
		
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
	
	public override float LengthRadius(float radius)
	{
		Vector3 center   = Center(radius);
		Vector3 position = Evaluate(Begin(), radius);
		
		return (position - center).magnitude;
	}
	
	private void OnDrawGizmos() //TODO: get rid of this in production builds //It's tedious that I can't just put this in the editor code
	{
		// draw floor path
		DrawArc(0.0f, Color.black);
		
		// draw CoM path
		DrawArc(0.025f, Color.grey);
		
		// draw ceil path
		DrawArc(0.05f, Color.white);
		
		DrawDefault();
	}

	public override void Save()
	{
		base.Save();
		Undo.RecordObject(this, "Save convex corner");
	}

	public static ConvexCorner Spawn(ArcOfSphere previous_edge, ArcOfSphere next_edge)
	{
		GameObject prefab = (GameObject) Resources.Load("ConvexCornerPrefab");
		
		#if UNITY_EDITOR
		GameObject obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
		#else
		GameObject obj = Instantiate(prefab) as GameObject;
		#endif
		
		Undo.RegisterCreatedObjectUndo(obj, "Created convex corner");
		
		obj.name = "Convex corner";
		
		ConvexCorner result = obj.GetComponent<ConvexCorner>();
		
		result.Initialize(previous_edge, next_edge);

		result.Save();

		result.LinkBlock(previous_edge);
		
		return result;
	}
}