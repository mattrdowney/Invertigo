using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(ArcOfSphere))]
public class ArcOfSphereEditor : Editor
{
	ArcOfSphere								self;

	//shift, latitude/longitude grid aligned with extra static variables for controlling the number of subdivisions (for making levels appear to grow/shrink)

	void CornerEditor()
	{
		//space, move corner to position and recalculate
		//q, place a new corner using LinkLeft at mouse cursor
		//e, place a new corner using LinkRight at mouse cursor
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

	void PathEditor()
	{
		//q, allign the arc with the imaginary great circle from half the left corner's sweeping angle
		//e, ditto for the right corner

		//k, rotate normal clockwise
		//l, rotate normal counter-clockwise

		//space, place corner at mouse position
	}

	public void Step()
	{
		//9, cycle to left ArcOfSphere
		//0, cycle to right ArcOfSphere
		//turning on/off CornerEditor and PathEditor shouldn't be neccessary?
	}
}
