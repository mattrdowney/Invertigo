using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(Abaddon))] //http://code.tutsplus.com/tutorials/how-to-add-your-own-tools-to-unitys-editor--active-10047
public class AbaddonEditor : Editor
{
	Abaddon									self;

	Vector3									first_click_point;

	ArcOfSphere								arc; //FIXME: Property or Optional, null used for convenience

	void Create(SceneView scene_view)
	{
		Event e = Event.current;

		if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Space)
		{
			Vector3 click_point = AbaddonUtility.CursorCast(scene_view.camera, e.mousePosition);

			if(!arc)
			{
				Transform block_transform = Block.Spawn().transform;

				arc = ArcOfSphere.StartShape(first_click_point, click_point, block_transform);
			}
			else
			{
				arc = arc.DivideEdge(click_point);
			}
		}
		else if(e.type == EventType.MouseDown && e.button == 0)
		{
			arc = null;

			ArcOfSphere.guid = 0;

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

			first_click_point = AbaddonUtility.CursorCast(scene_view.camera, Event.current.mousePosition);

			Debug.Log("Switching to Create");
			AbaddonUtility.Align(scene_view);
		}
	}	

	void Listen(SceneView scene_view)
	{
		if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space)
		{
			SceneView.onSceneGUIDelegate -= Listen;
			SceneView.onSceneGUIDelegate += Edit;
			AbaddonUtility.AllowRotation();
			AbaddonUtility.Align(scene_view);
		}
	}

	public void OnEnable()
	{
		self = target as Abaddon;
		SceneView.onSceneGUIDelegate += Listen;
	}

	public void OnDisable()
	{
		SceneView.onSceneGUIDelegate -= Listen;
		SceneView.onSceneGUIDelegate -= Edit;
		SceneView.onSceneGUIDelegate -= Create;

		AbaddonUtility.DisallowRotation();
	}


}
