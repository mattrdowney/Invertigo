using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(Abaddon))] //http://code.tutsplus.com/tutorials/how-to-add-your-own-tools-to-unitys-editor--active-10047
public class AbaddonEditor : Editor
{
	Abaddon									self;

	Vector3									first_edge;
	Vector3									last_edge;

	ArcOfSphere								first_trapezoid; //FIXME: Property or Optional, null used for convenience
	ArcOfSphere								last_trapezoid; //FIXME: Property or Optional, null used for convenience

	Transform								block_transform;

	void Create(SceneView scene_view)
	{
		Event e = Event.current;

		if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Space)
		{
			Vector3 click_point = AbaddonUtility.CursorCast(scene_view.camera, e.mousePosition);

			if(!first_trapezoid)
			{
				first_trapezoid = last_trapezoid = ArcOfSphere.Spawn(last_edge, click_point, Vector3.Cross (last_edge, click_point));

				first_trapezoid.gameObject.transform.parent = block_transform;
			}
			else
			{
				ArcOfSphere trapezoid = last_trapezoid.LinkRight(click_point);
				ArcOfSphere corner    = ArcOfSphere.SpawnCorner(last_trapezoid, trapezoid);

				trapezoid.gameObject.transform.parent = block_transform;
				corner   .gameObject.transform.parent = block_transform;

				last_trapezoid = trapezoid;
			}
		}
		else if(e.type == EventType.MouseDown && e.button == 0)
		{
			ArcOfSphere trapezoid = last_trapezoid.LinkRight(first_edge);
			ArcOfSphere corner0   = ArcOfSphere.SpawnCorner(last_trapezoid, trapezoid);
			ArcOfSphere corner1   = ArcOfSphere.SpawnCorner(trapezoid, first_trapezoid);

			trapezoid.gameObject.transform.parent = block_transform;
			corner0  .gameObject.transform.parent = block_transform;
			corner1  .gameObject.transform.parent = block_transform;

			first_trapezoid = last_trapezoid = null;

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

			first_edge = last_edge = AbaddonUtility.CursorCast(scene_view.camera, Event.current.mousePosition);

			block_transform = Block.Spawn().transform;

			Debug.Log(block_transform.name);

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
