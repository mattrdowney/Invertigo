using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(Abaddon))] //http://code.tutsplus.com/tutorials/how-to-add-your-own-tools-to-unitys-editor--active-10047
public class AbaddonEditor : Editor
{
	Transform 						forward;
	Transform						yawTrans;
	Transform						pitchTrans;
	Abaddon							abaddon;
	bool							editing;

	void Editing(SceneView sceneview)
	{
		SceneView.lastActiveSceneView.camera.transform.position = Vector3.zero; //XXX: should be the one I want, then I just use LookAt
		sceneview.AlignViewToObject(forward);
		sceneview.Repaint();
	}

	void Listen(SceneView sceneview)
	{
		Event e = Event.current;

		Ray r = Camera.current.ScreenPointToRay(new Vector3(e.mousePosition.x, -e.mousePosition.y + Camera.current.pixelHeight));
		Vector3 mousePos = r.origin;

		if(e.type == EventType.KeyDown && e.keyCode == KeyCode.A)
		{
			Debug.Log ("Happening1");

			Object prefab = PrefabUtility.GetPrefabParent(Selection.activeObject);
			
			if (prefab)
			{
				//Undo.IncrementCurrentEventIndex();
				GameObject obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
				PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
				Vector3 aligned = new Vector3(Mathf.Floor(mousePos.x/abaddon.width )*abaddon.width  + abaddon.width /2.0f,
				                              Mathf.Floor(mousePos.y/abaddon.height)*abaddon.height + abaddon.height/2.0f, 0.0f);
				obj.transform.position = aligned;
				Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
			}
			Event.PopEvent(Event.current);
		}
		else if(e.type == EventType.KeyDown && e.keyCode == KeyCode.D)
		{
			Debug.Log ("Happening2");
			//Undo.IncrementCurrentEventIndex();
			Undo.SetCurrentGroupName("Delete object set");
			foreach (GameObject obj in Selection.gameObjects)
			{
				Undo.DestroyObjectImmediate(obj);
				DestroyImmediate(obj);
			}
			Event.PopEvent(Event.current);
		}
		if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Space)
		{
			editing = !editing;

			if(editing)
			{
				Debug.Log ("Happening3");
				SceneView.onSceneGUIDelegate -= Editing;
				SceneView.onSceneGUIDelegate += Editing;
			}
			else
			{
				Debug.Log ("Happening4");
				SceneView.onSceneGUIDelegate -= Editing;
			}

			Event.PopEvent(Event.current);
		}

		if(editing)
		{
			if(e.type == EventType.KeyDown && e.keyCode == KeyCode.LeftArrow)
			{
				SceneView.onSceneGUIDelegate -= RotateLeft;
				SceneView.onSceneGUIDelegate += RotateLeft;
			}
			else if(e.type == EventType.KeyDown && e.keyCode == KeyCode.RightArrow)
			{
				SceneView.onSceneGUIDelegate -= RotateRight;
				SceneView.onSceneGUIDelegate += RotateRight;
			}
			else if(e.type == EventType.KeyDown && e.keyCode == KeyCode.UpArrow)
			{
				SceneView.onSceneGUIDelegate -= RotateUp;
				SceneView.onSceneGUIDelegate += RotateUp;
			}
			else if(e.type == EventType.KeyDown && e.keyCode == KeyCode.DownArrow)
			{
				SceneView.onSceneGUIDelegate -= RotateDown;
				SceneView.onSceneGUIDelegate += RotateDown;
			}
			else if(e.type == EventType.KeyUp && e.keyCode == KeyCode.LeftArrow)
			{
				SceneView.onSceneGUIDelegate -= RotateLeft;
			}
			else if(e.type == EventType.KeyUp && e.keyCode == KeyCode.RightArrow)
			{
				SceneView.onSceneGUIDelegate -= RotateRight;
			}
			else if(e.type == EventType.KeyUp && e.keyCode == KeyCode.UpArrow)
			{
				SceneView.onSceneGUIDelegate -= RotateUp;
			}
			else if(e.type == EventType.KeyUp && e.keyCode == KeyCode.DownArrow)
			{
				SceneView.onSceneGUIDelegate -= RotateDown;
			}
		}
	}

	void RotateLeft(SceneView sceneview)
	{
		yawTrans.localRotation *= Quaternion.Euler(0f, -0.1f, 0f);
	}
	void RotateRight(SceneView sceneview)
	{
		yawTrans.localRotation *= Quaternion.Euler(0f, 0.1f, 0f);
	}
	void RotateUp(SceneView sceneview)
	{
		pitchTrans.localRotation *= Quaternion.Euler(-0.1f, 0f, 0f);
	}
	void RotateDown(SceneView sceneview)
	{
		pitchTrans.localRotation *= Quaternion.Euler(0.1f, 0f, 0f);
	}
	
	public void OnEnable()
	{
		abaddon = target as Abaddon;
		yawTrans   = GameObject.Find("/PivotYaw").transform;
		pitchTrans = GameObject.Find("/PivotYaw/PivotPitch").transform;
		forward    = GameObject.Find("/PivotYaw/PivotPitch/Zero").transform;
		editing = false;
		SceneView.onSceneGUIDelegate -= Listen;
		SceneView.onSceneGUIDelegate -= Editing;
		SceneView.onSceneGUIDelegate -= RotateLeft;
		SceneView.onSceneGUIDelegate -= RotateRight;
		SceneView.onSceneGUIDelegate -= RotateUp;
		SceneView.onSceneGUIDelegate -= RotateDown;
		SceneView.onSceneGUIDelegate += Listen;
	}

	public override void OnInspectorGUI()
	{
		//SceneView.lastActiveSceneView.camera.transform.position = Vector3.zero;
		GUILayout.BeginHorizontal();
		GUILayout.Label(" Grid Width ");
		abaddon.width = EditorGUILayout.FloatField(abaddon.width, GUILayout.Width(50));
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.Label(" Grid Height ");
		abaddon.height = EditorGUILayout.FloatField(abaddon.height, GUILayout.Width(50));
		GUILayout.EndHorizontal();
		
		SceneView.RepaintAll();
	}
}
