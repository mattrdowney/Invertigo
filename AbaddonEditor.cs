using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(Abaddon))] //http://code.tutsplus.com/tutorials/how-to-add-your-own-tools-to-unitys-editor--active-10047
public class AbaddonEditor : Editor
{
	Transform 						forward; //should be unneccessary, but w/e
	Transform						yawTrans;
	Transform						pitchTrans;
	Abaddon							abaddon;

	Vector3							firstEdge;
	Vector3							lastEdge;

	void Align(SceneView sceneview)
	{
		sceneview.AlignViewToObject(forward);
		sceneview.camera.transform.position = Vector3.zero; //XXX: should be the one I want, then I just use LookAt
		sceneview.Repaint();
	}

	void AllowRotation()
	{
		SceneView.onSceneGUIDelegate += WaitLeft;
		SceneView.onSceneGUIDelegate += WaitRight;
		SceneView.onSceneGUIDelegate += WaitUp;
		SceneView.onSceneGUIDelegate += WaitDown;
	}	

	public void CancelState()
	{
		SceneView.onSceneGUIDelegate -= Listen;
		SceneView.onSceneGUIDelegate -= Edit;
		SceneView.onSceneGUIDelegate -= WaitLeft;
		SceneView.onSceneGUIDelegate -= WaitRight;
		SceneView.onSceneGUIDelegate -= WaitUp;
		SceneView.onSceneGUIDelegate -= WaitDown;
		SceneView.onSceneGUIDelegate -= RotateLeft;
		SceneView.onSceneGUIDelegate -= RotateRight;
		SceneView.onSceneGUIDelegate -= RotateUp;
		SceneView.onSceneGUIDelegate -= RotateDown;
		SceneView.onSceneGUIDelegate -= Create;
		SceneView.onSceneGUIDelegate -= Escape;
	}

	void Create(SceneView sceneview)
	{
		Event e = Event.current;
		
		if(e.type == EventType.MouseDown)
		{	
			Vector3 clickPoint = CursorCast(sceneview.camera, e.mousePosition);

			Object prefab = PrefabUtility.GetPrefabParent(Selection.activeObject);
			
			if (prefab)
			{
				//Undo.IncrementCurrentEventIndex();
				GameObject obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
				PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
				
				obj.transform.position = clickPoint;
				Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
			}
			Event.PopEvent(Event.current);
		}
		else if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Space)
		{
			SceneView.onSceneGUIDelegate -= Create;
			SceneView.onSceneGUIDelegate += Edit;
			//TODO: FINALIZE firstEdge
			DebugUtility.Print("Switching to Edit");
		}
	}

	Vector3 CursorCast(Camera cam, Vector2 mousePos)
	{
		return cam.ScreenPointToRay(new Vector3(mousePos.x, cam.pixelHeight - mousePos.y, 1)).direction;
	}

	void Edit(SceneView sceneview)
	{
		Event e = Event.current;
		
		if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Space)
		{
			CancelState();
			SceneView.onSceneGUIDelegate += Listen;
			DebugUtility.Print("Switching to Listen");
		}
		else if(e.type == EventType.MouseDown)
		{
			SceneView.onSceneGUIDelegate -= Edit;
			SceneView.onSceneGUIDelegate += Create;

			Debug.Log(e.mousePosition);

			firstEdge = lastEdge = CursorCast(sceneview.camera, e.mousePosition);
			Debug.Log (firstEdge.magnitude);
			Debug.DrawRay(firstEdge, Vector3.up, Color.red, 10f);
			DebugUtility.Print("Switching to Create");
		}
	}	

	void Escape(SceneView sceneview)
	{
		if(Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Escape)
		{
			CancelState();
			DebugUtility.Print("Canceling");
		}
	}

	void Listen(SceneView sceneview)
	{
		if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space)
		{
			SceneView.onSceneGUIDelegate -= Listen;
			SceneView.onSceneGUIDelegate += Edit;
			AllowRotation();
			Align(sceneview);
			DebugUtility.Print("Switching to Edit");
		}
	}

	public void OnEnable()
	{
		abaddon = target as Abaddon;
		yawTrans   = GameObject.Find("/PivotYaw").transform;
		pitchTrans = GameObject.Find("/PivotYaw/PivotPitch").transform;
		forward    = GameObject.Find("/PivotYaw/PivotPitch/Zero").transform;
		CancelState();
		SceneView.onSceneGUIDelegate += Listen;
		SceneView.onSceneGUIDelegate += Escape;
	}

	public void OnDisable()
	{
		CancelState();
	}

	void RotateLeft(SceneView sceneview)
	{
		//Debug.Log("Left");
		if(Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.A)
		{
			SceneView.onSceneGUIDelegate -= RotateLeft;
			SceneView.onSceneGUIDelegate += WaitLeft;
			DebugUtility.Print("Switching to Edit");
		}
		yawTrans.localRotation *= Quaternion.Euler(0f, -0.1f, 0f);
		Align(sceneview);
	}
	
	void RotateRight(SceneView sceneview)
	{
		//Debug.Log("Right");
		if(Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.D)
		{
			SceneView.onSceneGUIDelegate -= RotateRight;
			SceneView.onSceneGUIDelegate += WaitRight;
			DebugUtility.Print("Switching to Edit");
		}
		yawTrans.localRotation *= Quaternion.Euler(0f, 0.1f, 0f);
		Align(sceneview);
	}
	
	void RotateUp(SceneView sceneview)
	{
		//Debug.Log("Up");
		if(Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.W)
		{
			SceneView.onSceneGUIDelegate -= RotateUp;
			SceneView.onSceneGUIDelegate += WaitUp;
			DebugUtility.Print("Switching to Edit");
		}
		pitchTrans.localRotation *= Quaternion.Euler(-0.1f, 0f, 0f);
		Align(sceneview);
	}
	
	void RotateDown(SceneView sceneview)
	{
		//Debug.Log("Down");
		if(Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.S)
		{
			SceneView.onSceneGUIDelegate -= RotateDown;
			SceneView.onSceneGUIDelegate += WaitDown;
			DebugUtility.Print("Switching to Edit");
		}
		pitchTrans.localRotation *= Quaternion.Euler(0.1f, 0f, 0f);
		Align(sceneview);
	}

	void WaitLeft(SceneView sceneview)
	{
		if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.A)
		{
			SceneView.onSceneGUIDelegate -= WaitLeft;
			SceneView.onSceneGUIDelegate += RotateLeft;
			DebugUtility.Print("Switching to Left");
		}
	}
	
	void WaitRight(SceneView sceneview)
	{
		if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.D)
		{
			SceneView.onSceneGUIDelegate -= WaitRight;
			SceneView.onSceneGUIDelegate += RotateRight;
			DebugUtility.Print("Switching to Right");
		}
	}
	
	void WaitUp(SceneView sceneview)
	{
		if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.W)
		{
			SceneView.onSceneGUIDelegate -= WaitUp;
			SceneView.onSceneGUIDelegate += RotateUp;
			DebugUtility.Print("Switching to Up");
		}
	}
	
	void WaitDown(SceneView sceneview)
	{
		if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.S)
		{
			SceneView.onSceneGUIDelegate -= WaitDown;
			SceneView.onSceneGUIDelegate += RotateDown;
			DebugUtility.Print("Switching to Down");
		}
	}
}
