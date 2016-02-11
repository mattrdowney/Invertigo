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

	Vector3							first_edge;
	Vector3							last_edge;

	SphericalIsoscelesTrapezoid		trapezoid;

	void Align(SceneView scene_view)
	{
		scene_view.AlignViewToObject(forward);
		scene_view.camera.transform.position = Vector3.zero; //XXX: should be the one I want, then I just use LookAt
		scene_view.Repaint();
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

	void Create(SceneView scene_view)
	{
		Event e = Event.current;
		
		if(e.type == EventType.MouseDown && e.button == 0)
		{	
			Vector3 click_point = CursorCast(scene_view.camera, e.mousePosition);

			SphericalIsoscelesTrapezoid next_trapezoid = CreateSphericalIsoscelesTrapezoid(
					last_edge, click_point, Vector3.Cross(last_edge, click_point));
			
			trapezoid.next = next_trapezoid;
			next_trapezoid.prev = trapezoid;
			
			trapezoid = next_trapezoid;

			last_edge = click_point;

			DebugUtility.Print("Pew");

			Event.PopEvent(e);
		}
		else if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Space)
		{
			SceneView.onSceneGUIDelegate -= Create;
			SceneView.onSceneGUIDelegate += Edit;
			//TODO: FINALIZE firstEdge
			DebugUtility.Print("Switching to Edit");
		}
		else if(e.type == EventType.KeyDown && e.keyCode == KeyCode.P)
		{
			SphericalIsoscelesTrapezoid next_trapezoid = CreateSphericalIsoscelesTrapezoid(
					last_edge, first_edge, Vector3.Cross(last_edge, first_edge));

			trapezoid.next = next_trapezoid;
			next_trapezoid.prev = trapezoid;

			trapezoid = next_trapezoid;

			SceneView.onSceneGUIDelegate -= Create;
			SceneView.onSceneGUIDelegate += Edit;

			DebugUtility.Print("Pew");
			DebugUtility.Print("Switching to Edit");
		}
	}

	SphericalIsoscelesTrapezoid CreateSphericalIsoscelesTrapezoid(Vector3 left_edge, Vector3 right_edge, Vector3 normal)
	{
		GameObject obj = PrefabUtility.InstantiatePrefab(abaddon.prefab) as GameObject;

		BoxCollider	collider = obj.GetComponent<BoxCollider>(); 
		SphericalIsoscelesTrapezoid trapezoid = obj.GetComponent<SphericalIsoscelesTrapezoid>();

		trapezoid.Initialize(left_edge, right_edge, normal);
		trapezoid.RecalculateAABB(collider);

		return trapezoid; //used for next/prev
	}

	Vector3 CursorCast(Camera cam, Vector2 mousePos)
	{
		return cam.ScreenPointToRay(new Vector3(mousePos.x, cam.pixelHeight - mousePos.y, 1)).direction;
	}

	void Edit(SceneView scene_view)
	{
		Event e = Event.current;
		
		if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Space)
		{
			CancelState();
			SceneView.onSceneGUIDelegate += Listen;
			DebugUtility.Print("Switching to Listen");
		}
		else if(e.type == EventType.MouseDown && e.button == 0)
		{
			SceneView.onSceneGUIDelegate -= Edit;
			SceneView.onSceneGUIDelegate += Create;

			first_edge = last_edge = CursorCast(scene_view.camera, e.mousePosition);

			Debug.DrawRay(first_edge, Vector3.up, Color.red, 10f);
			DebugUtility.Print("Switching to Create");
		}
	}	

	void Escape(SceneView scene_view)
	{
		if(Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Escape)
		{
			CancelState();
			DebugUtility.Print("Canceling");
		}
	}

	void Instantiate(Vector3 p1, Vector3 p2)
	{

	}

	void Listen(SceneView scene_view)
	{
		if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space)
		{
			SceneView.onSceneGUIDelegate -= Listen;
			SceneView.onSceneGUIDelegate += Edit;
			AllowRotation();
			Align(scene_view);
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

	void RotateLeft(SceneView scene_view)
	{
		//Debug.Log("Left");
		if(Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.A)
		{
			SceneView.onSceneGUIDelegate -= RotateLeft;
			SceneView.onSceneGUIDelegate += WaitLeft;
			DebugUtility.Print("Switching to Edit");
		}
		yawTrans.localRotation *= Quaternion.Euler(0f, -0.1f, 0f);
		Align(scene_view);
	}
	
	void RotateRight(SceneView scene_view)
	{
		//Debug.Log("Right");
		if(Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.D)
		{
			SceneView.onSceneGUIDelegate -= RotateRight;
			SceneView.onSceneGUIDelegate += WaitRight;
			DebugUtility.Print("Switching to Edit");
		}
		yawTrans.localRotation *= Quaternion.Euler(0f, 0.1f, 0f);
		Align(scene_view);
	}
	
	void RotateUp(SceneView scene_view)
	{
		//Debug.Log("Up");
		if(Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.W)
		{
			SceneView.onSceneGUIDelegate -= RotateUp;
			SceneView.onSceneGUIDelegate += WaitUp;
			DebugUtility.Print("Switching to Edit");
		}
		pitchTrans.localRotation *= Quaternion.Euler(-0.1f, 0f, 0f);
		Align(scene_view);
	}
	
	void RotateDown(SceneView scene_view)
	{
		//Debug.Log("Down");
		if(Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.S)
		{
			SceneView.onSceneGUIDelegate -= RotateDown;
			SceneView.onSceneGUIDelegate += WaitDown;
			DebugUtility.Print("Switching to Edit");
		}
		pitchTrans.localRotation *= Quaternion.Euler(0.1f, 0f, 0f);
		Align(scene_view);
	}

	void WaitLeft(SceneView scene_view)
	{
		if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.A)
		{
			SceneView.onSceneGUIDelegate -= WaitLeft;
			SceneView.onSceneGUIDelegate += RotateLeft;
			DebugUtility.Print("Switching to Left");
		}
	}
	
	void WaitRight(SceneView scene_view)
	{
		if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.D)
		{
			SceneView.onSceneGUIDelegate -= WaitRight;
			SceneView.onSceneGUIDelegate += RotateRight;
			DebugUtility.Print("Switching to Right");
		}
	}
	
	void WaitUp(SceneView scene_view)
	{
		if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.W)
		{
			SceneView.onSceneGUIDelegate -= WaitUp;
			SceneView.onSceneGUIDelegate += RotateUp;
			DebugUtility.Print("Switching to Up");
		}
	}
	
	void WaitDown(SceneView scene_view)
	{
		if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.S)
		{
			SceneView.onSceneGUIDelegate -= WaitDown;
			SceneView.onSceneGUIDelegate += RotateDown;
			DebugUtility.Print("Switching to Down");
		}
	}
}
