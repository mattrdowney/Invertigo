using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(ArcOfSphere))]
public class ArcOfSphereEditor : Editor
{
	ArcOfSphere								self;

	//shift, latitude/longitude grid aligned with extra static variables for controlling the number of subdivisions (for making levels appear to grow/shrink)

	void CornerEditor(SceneView scene_view)
	{
		Event e = Event.current;

		if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Space) //space, move corner to position and recalculate
		{

		}
		else if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Q) //q, place a new corner using LinkLeft at mouse cursor
		{

		}
		else if(e.type == EventType.KeyDown && e.keyCode == KeyCode.E) //e, place a new corner using LinkRight at mouse cursor
		{

		}
	}

	public void OnEnable()
	{
		self = target as ArcOfSphere;
		if(self.arc_radius == 1e-36f) //FIXME: make zero
		{
			SceneView.onSceneGUIDelegate += CornerEditor;
		}
		else
		{
			SceneView.onSceneGUIDelegate += PathEditor;
		}
		SceneView.onSceneGUIDelegate += Step;
	}
	
	public void OnDisable()
	{
		SceneView.onSceneGUIDelegate -= CornerEditor;
		SceneView.onSceneGUIDelegate -= PathEditor;
		SceneView.onSceneGUIDelegate -= Step;
	}

	void PathEditor(SceneView scene_view)
	{
		Event e = Event.current;
		
		if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Space) //space, place corner at mouse position
		{
			
		}
		else if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Q) //q, allign the arc with the imaginary great circle from half the left corner's sweeping angle
		{
			
		}
		else if(e.type == EventType.KeyDown && e.keyCode == KeyCode.E) //e, ditto for the right corner
		{
			
		}
		else if(e.type == EventType.KeyDown && e.keyCode == KeyCode.K) //k, rotate normal clockwise
		{
			
		}
		else if(e.type == EventType.KeyDown && e.keyCode == KeyCode.L) //l, rotate normal counter-clockwise
		{
			
		}
	}

	public void Step(SceneView scene_view)
	{
		Event e = Event.current;

		if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Alpha9)
		{
			Object[] selection = new Object[]{(Object)self.prev.gameObject};
			Selection.objects = selection;
		}
		else if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Alpha0)
		{
			Object[] selection = new Object[]{(Object)self.next.gameObject};
			Selection.objects = selection;
		}
		//turning on/off CornerEditor and PathEditor shouldn't be neccessary?
		//what if multiple selected objects switch the selection? (seems to do nothing)
	}
}
