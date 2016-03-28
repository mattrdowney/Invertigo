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
			//self.prev.Initialize(stuffstuffstuff);
			//self.next.Initialize(stuffstuffstuff);

			//InvertigoUtility.Reattach(self.prev.prev, self.prev, self, self.next, self.next.next); //TODO: do I need 5 args?
		}
		else if(e.type == EventType.KeyDown && e.keyCode == KeyCode.M) //m, remove selected corner
		{
			//SelectArc(self.RemoveCorner());
		}
		else if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Q) //q, place a new corner using LinkLeft at mouse cursor //Consider: delete, redundant
		{
			Vector3 click_point = InvertigoUtility.CursorCast(scene_view.camera, e.mousePosition);
			
			//self.prev.DivideEdge(click_point);
		}
		else if(e.type == EventType.KeyDown && e.keyCode == KeyCode.E) //e, place a new corner using LinkRight at mouse cursor //Consider: delete, redundant
		{
			Vector3 click_point = InvertigoUtility.CursorCast(scene_view.camera, e.mousePosition);

			//self.next.DivideEdge(click_point);
		}
	}

	void OnDrawGizmos()
	{

	}

	public void OnEnable()
	{
		self = target as ArcOfSphere;
		if(self.LengthRadius() == 0)
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
			Vector3 click_point = InvertigoUtility.CursorCast(scene_view.camera, e.mousePosition);

			//self.DivideEdge(click_point);

			//Reattach()
		}
		else if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Q) //q, allign the arc with the imaginary great circle from half the left corner's sweeping angle
		{
			
		}
		else if(e.type == EventType.KeyDown && e.keyCode == KeyCode.E) //e, ditto for the right corner
		{
			
		}
		else if(e.type == EventType.KeyDown && e.keyCode == KeyCode.K) //k, rotate normal clockwise
		{
			//self.Initialize(self.Begin(), self.End(), +stuffstuffstuff);
		}
		else if(e.type == EventType.KeyDown && e.keyCode == KeyCode.L) //l, rotate normal counter-clockwise
		{
			//self.Initialize(self.Begin(), self.End(), -stuffstuffstuff);
		}
	}

	
	void SelectArc(ArcOfSphere arc)
	{
		Object[] selection = new Object[]{(Object)arc.gameObject};
		Selection.objects = selection;
	}

	public void Step(SceneView scene_view)
	{
		Event e = Event.current;

		if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Alpha9)
		{
			SelectArc(self.prev);
		}
		else if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Alpha0)
		{
			SelectArc(self.next);
		}
		//turning on/off CornerEditor and PathEditor shouldn't be neccessary?
		//what if multiple selected objects switch the selection? (seems to do nothing)
	}



}
