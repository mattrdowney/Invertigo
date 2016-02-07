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

	SphericalIsoscelesTrapezoid		firstEdge;

	void Listen(SceneView sceneview)
	{
		if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space)
		{
			SceneView.onSceneGUIDelegate -= Listen;
			SceneView.onSceneGUIDelegate += Edit;
			DebugUtility.Print("Switching to Edit");
		}
	}

	void Edit(SceneView sceneview)
	{
		Event e = Event.current;
		
		if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Space)
		{
			SceneView.onSceneGUIDelegate -= Edit;
			SceneView.onSceneGUIDelegate += Listen;
			DebugUtility.Print("Switching to Listen");
		}
		else if(e.type == EventType.MouseDown)
		{
			SceneView.onSceneGUIDelegate -= Edit;
			SceneView.onSceneGUIDelegate += Create;
			DebugUtility.Print("Switching to Create");
		}

		Rotate(sceneview);
		Align(sceneview);
	}

	void Create(SceneView sceneview)
	{
		Event e = Event.current;
		Ray r = Camera.current.ScreenPointToRay(new Vector3(e.mousePosition.x, -e.mousePosition.y));
		Vector3 mousePos = r.direction;
		
		if(e.type == EventType.MouseDown)
		{	
			Object prefab = PrefabUtility.GetPrefabParent(Selection.activeObject);
			
			if (prefab)
			{
				//Undo.IncrementCurrentEventIndex();
				GameObject obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
				PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
				
				obj.transform.position = mousePos;
				Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
			}
			Event.PopEvent(Event.current);
		}

		Rotate(sceneview);
		Align(sceneview);
	}

	void Escape(SceneView sceneview)
	{
		if(Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Escape)
		{
			CancelState();
			DebugUtility.Print("Canceling");
		}
	}

	void Rotate(SceneView sceneview)
	{
		Event e = Event.current;
		if(e.type == EventType.KeyDown && e.keyCode == KeyCode.A)
		{
			SceneView.onSceneGUIDelegate -= Edit;
			SceneView.onSceneGUIDelegate += RotateLeft;
			DebugUtility.Print("Switching to Left");
		}
		else if(e.type == EventType.KeyDown && e.keyCode == KeyCode.D)
		{
			SceneView.onSceneGUIDelegate -= Edit;
			SceneView.onSceneGUIDelegate += RotateRight;
			DebugUtility.Print("Switching to Right");
		}
		else if(e.type == EventType.KeyDown && e.keyCode == KeyCode.W)
		{
			SceneView.onSceneGUIDelegate -= Edit;
			SceneView.onSceneGUIDelegate += RotateUp;
			DebugUtility.Print("Switching to Up");
		}
		else if(e.type == EventType.KeyDown && e.keyCode == KeyCode.S)
		{
			SceneView.onSceneGUIDelegate -= Edit;
			SceneView.onSceneGUIDelegate += RotateDown;
			DebugUtility.Print("Switching to Down");
		}
	}

	void RotateLeft(SceneView sceneview)
	{
		//Debug.Log("Left");
		if(Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.A)
		{
			SceneView.onSceneGUIDelegate -= RotateLeft;
			SceneView.onSceneGUIDelegate += Edit;
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
			SceneView.onSceneGUIDelegate += Edit;
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
			SceneView.onSceneGUIDelegate += Edit;
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
			SceneView.onSceneGUIDelegate += Edit;
			DebugUtility.Print("Switching to Edit");
		}
		pitchTrans.localRotation *= Quaternion.Euler(0.1f, 0f, 0f);
		Align(sceneview);
	}

	void Align(SceneView sceneview)
	{
		sceneview.camera.transform.position = Vector3.zero; //XXX: should be the one I want, then I just use LookAt
		sceneview.AlignViewToObject(forward);
		sceneview.Repaint();
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

	public void CancelState()
	{
		SceneView.onSceneGUIDelegate -= Listen;
		SceneView.onSceneGUIDelegate -= Edit;
		SceneView.onSceneGUIDelegate -= RotateLeft;
		SceneView.onSceneGUIDelegate -= RotateRight;
		SceneView.onSceneGUIDelegate -= RotateUp;
		SceneView.onSceneGUIDelegate -= RotateDown;
		SceneView.onSceneGUIDelegate -= Create;
		SceneView.onSceneGUIDelegate -= Escape;
	}
}
