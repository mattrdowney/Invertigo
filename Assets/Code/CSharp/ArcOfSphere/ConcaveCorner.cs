using UnityEngine;
using System.Diagnostics;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class ConcaveCorner : Corner
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
		return SphereUtility.SphereToCartesian(new Vector2(radius / Mathf.Cos(arc_angle / 2), arc_angle / 2), arc_left, arc_left_normal, path_normal); //wow, I can't believe it was really cos(angle/2)
	}
	
	public override bool Contains(Vector3 position, float radius)
	{
		bool bAboveGround = Vector3.Dot(position - Center(radius), path_normal) >= 0;
		bool bBelowCenterOfMass	  = Vector3.Dot(position - Center()      , path_normal) <= 0; //COM means center of mass
		bool bIsAtCorrectElevation = bAboveGround && bBelowCenterOfMass;
		
		bool bLeftContains	= Vector3.Dot(position - Center(),  arc_left_normal  ) >= 0;
		bool bRightContains	= Vector3.Dot(position - Center(), arc_right_normal) >= 0;
		bool bCorrectAngle	= bLeftContains && bRightContains;

        DebugUtility.Log("above:", bAboveGround, "below:", bBelowCenterOfMass, "left:", bLeftContains, "right:", bRightContains);

        return bIsAtCorrectElevation && bCorrectAngle;
	}
	
	public override optional<float> Distance(Vector3 to, Vector3 from, float radius) //distance is Euclidean but is (guaranteed?) to be sorted correctly with the current assertions about speed vs player_radius
	{
        optional<Vector3> point = SphereUtility.Intersection(from, to, Center(radius), AngularRadius(radius));
        optional<float> intersection = new optional<float>();

        if(point.exists)
        {
            intersection = CartesianToRadial(point.data);
        }
		
		if(intersection.exists)
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
		float height = Vector3.Distance(next.Evaluate(next.Begin(radius)), Evaluate(End(radius),radius));
		UnityEditor.Handles.DrawWireArc(Center(radius), -Center(radius), prev.Evaluate(prev.End(radius), radius) - Center(radius), arc_angle * 180 / Mathf.PI, height);
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

    public override float End(float radius)
	{
		return arc_angle;
	}
	
	/** return the position of the player based on the circular path
	 *  
	 */
	public override Vector3 Evaluate(float angle, float radius)
	{
		return Center(radius);
	}
	
	public override Vector3 EvaluateNormal(float angle, float radius)
	{
		return Vector3.Slerp(arc_left, arc_right, angle/arc_angle);
	}
	
	public override Vector3 EvaluateRight(float angle, float radius) //TODO: optimize
    {
		return arc_right_normal;
	}

    public override Vector3 EvaluateLeft(float angle, float radius)
    {
        return arc_left_normal;
    }

    public override void Initialize(ArcOfSphere left, ArcOfSphere right)
	{
        #if UNITY_EDITOR
        this.Save();
        #endif
		
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
		
		arc_left_normal  =  Vector3.Cross(arc_left , -path_normal).normalized; //CHECK: probably right, but just in case
		arc_right_normal = -Vector3.Cross(arc_right, -path_normal).normalized;
		
		arc_angle = Vector3.Angle(arc_left, arc_right) * Mathf.PI / 180;
		
		RecalculateAABB(this);
		
        #if UNITY_EDITOR
		this.Save();
        #endif
	}

    public override float LengthRadius(float radius)
	{
		Vector3 CoM   = Center(radius);
		Vector3 floor = Center();
		
		return (CoM - floor).magnitude;
	}
	
    #if UNITY_EDITOR
	private void OnDrawGizmos() //TODO: get rid of this in production builds //It's tedious that I can't just put this in the editor code
	{	
		// draw Floor path
		DrawArc(0.025f, Color.black);
		
		//DrawDefault();
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
		Undo.RecordObject(this, "Save concave corner");
	}
    #endif
	
	public static ConcaveCorner Spawn(ArcOfSphere previous_edge, ArcOfSphere next_edge)
	{
		GameObject prefab = (GameObject) Resources.Load("ConcaveCornerPrefab");
		
		#if UNITY_EDITOR
		GameObject obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
		#else
		GameObject obj = Instantiate(prefab) as GameObject;
		#endif
		
        #if UNITY_EDITOR
		Undo.RegisterCreatedObjectUndo(obj, "Created concave corner");
		#endif

		obj.name = "Concave corner";
		
		ConcaveCorner result = obj.GetComponent<ConcaveCorner>();
		
		result.Initialize(previous_edge, next_edge);
		
		#if UNITY_EDITOR
        result.Save();
		result.LinkBlock(previous_edge);
		#endif
		
		return result;
	}
}