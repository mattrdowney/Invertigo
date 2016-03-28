using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(Invertigo))] //http://code.tutsplus.com/tutorials/how-to-add-your-own-tools-to-unitys-editor--active-10047
public class InvertigoEditor : Editor
{
	Invertigo									self;

	Vector3									first_click_point;

	Edge									edge; //FIXME: Property or Optional, null used for convenience

	void Create(SceneView scene_view)
	{
		Event e = Event.current;

		if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Space)
		{
			Vector3 click_point = InvertigoUtility.CursorCast(scene_view.camera, e.mousePosition);

			if(!edge)
			{
				Transform block_transform = Block.Spawn().transform;

				edge = ShapeMaker.StartShape(block_transform, first_click_point, click_point);
			}
			else
			{
				edge = ShapeMaker.DivideEdge(edge, click_point);
			}
		}
		else if(e.type == EventType.MouseDown && e.button == 0)
		{
			edge = null;

			SceneView.onSceneGUIDelegate -= Create;
			SceneView.onSceneGUIDelegate += Edit;

			Debug.Log("Switching to Edit");
		}
	}

	void Edit(SceneView scene_view)
	{
		if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space)
		{
			SceneView.onSceneGUIDelegate -= Edit;
			SceneView.onSceneGUIDelegate += Create;

			first_click_point = InvertigoUtility.CursorCast(scene_view.camera, Event.current.mousePosition);

			Debug.Log("Switching to Create");
			InvertigoUtility.Align(scene_view);
		}
	}	

	void Listen(SceneView scene_view)
	{
		if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space)
		{
			SceneView.onSceneGUIDelegate -= Listen;
			SceneView.onSceneGUIDelegate += Edit;
			InvertigoUtility.AllowRotation();
			InvertigoUtility.Align(scene_view);
		}
	}

	public void OnEnable()
	{
		self = target as Invertigo;
		SceneView.onSceneGUIDelegate += Listen;
	}

	public void OnDisable()
	{
		SceneView.onSceneGUIDelegate -= Listen;
		SceneView.onSceneGUIDelegate -= Edit;
		SceneView.onSceneGUIDelegate -= Create;

		InvertigoUtility.DisallowRotation();
	}


}
