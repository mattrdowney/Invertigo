using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(Abaddon))] //http://code.tutsplus.com/tutorials/how-to-add-your-own-tools-to-unitys-editor--active-10047
public class AbaddonEditor : Editor
{
	Transform 								forward; //should be unneccessary, but w/e
	Transform								yawTrans;
	Transform								pitchTrans;
	Abaddon									abaddon;

	Vector3									first_edge;
	Vector3									last_edge;

	SphericalIsoscelesTrapezoid				first_trapezoid; //FIXME: Property or Optional, null used for convenience
	SphericalIsoscelesTrapezoid				last_trapezoid; //FIXME: Property or Optional, null used for convenience

	void Align(SceneView scene_view)
	{
		scene_view.AlignViewToObject(forward);
		scene_view.camera.transform.position = Vector3.zero;
		scene_view.Repaint();
	}

	void AllowRotation()
	{
		SceneView.onSceneGUIDelegate += WaitLeft;
		SceneView.onSceneGUIDelegate += WaitRight;
		SceneView.onSceneGUIDelegate += WaitUp;
		SceneView.onSceneGUIDelegate += WaitDown;
	}	

	void Create(SceneView scene_view)
	{
		Event e = Event.current;

		if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Space)
		{	
			Vector3 click_point = CursorCast(scene_view.camera, e.mousePosition);

			Debug.Log(last_edge.ToString());
			Debug.Log(click_point.ToString());

			SphericalIsoscelesTrapezoid next_trapezoid = CreateSphericalIsoscelesTrapezoid(
					last_edge, click_point, Vector3.Cross(last_edge, click_point));

			if(last_trapezoid)
			{
				last_trapezoid.next = next_trapezoid;
			}
			else
			{
				first_trapezoid = next_trapezoid;
			}
			next_trapezoid.prev = last_trapezoid;

			last_trapezoid = next_trapezoid;

			last_edge = click_point;

			Debug.Log("Pew");
		}
		else if(e.type == EventType.MouseDown && e.button == 0)
		{
			SphericalIsoscelesTrapezoid next_trapezoid = CreateSphericalIsoscelesTrapezoid(
					last_edge, first_edge, Vector3.Cross(last_edge, first_edge));

			next_trapezoid.next = first_trapezoid;
			next_trapezoid.prev = last_trapezoid;

			first_trapezoid.prev = next_trapezoid;
			last_trapezoid.next  = next_trapezoid;

			last_trapezoid = null;

			next_trapezoid.name = "last";

			SceneView.onSceneGUIDelegate -= Create;
			SceneView.onSceneGUIDelegate += Edit;

			Debug.Log("Pew");
			Debug.Log("Switching to Edit");
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
		if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space)
		{
			SceneView.onSceneGUIDelegate -= Edit;
			SceneView.onSceneGUIDelegate += Create;

			first_edge = last_edge = CursorCast(scene_view.camera, Event.current.mousePosition);

			Debug.DrawRay(first_edge, Vector3.up, Color.red);
			Debug.Log("Switching to Create");
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
		}
	}

	public void OnEnable()
	{
		abaddon = target as Abaddon;
		yawTrans   = GameObject.Find("/PivotYaw").transform;
		pitchTrans = GameObject.Find("/PivotYaw/PivotPitch").transform;
		forward    = GameObject.Find("/PivotYaw/PivotPitch/Zero").transform;
		SceneView.onSceneGUIDelegate += Listen;
	}

	public void OnDisable()
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
	}

	void RotateLeft(SceneView scene_view)
	{
		if(Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.A)
		{
			SceneView.onSceneGUIDelegate -= RotateLeft;
			SceneView.onSceneGUIDelegate += WaitLeft;
		}
		yawTrans.localRotation *= Quaternion.Euler(0f, -0.1f, 0f);
		Align(scene_view);
	}
	
	void RotateRight(SceneView scene_view)
	{
		if(Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.D)
		{
			SceneView.onSceneGUIDelegate -= RotateRight;
			SceneView.onSceneGUIDelegate += WaitRight;
		}
		yawTrans.localRotation *= Quaternion.Euler(0f, 0.1f, 0f);
		Align(scene_view);
	}
	
	void RotateUp(SceneView scene_view)
	{
		if(Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.W)
		{
			SceneView.onSceneGUIDelegate -= RotateUp;
			SceneView.onSceneGUIDelegate += WaitUp;
		}
		pitchTrans.localRotation *= Quaternion.Euler(-0.1f, 0f, 0f);
		Align(scene_view);
	}
	
	void RotateDown(SceneView scene_view)
	{
		if(Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.S)
		{
			SceneView.onSceneGUIDelegate -= RotateDown;
			SceneView.onSceneGUIDelegate += WaitDown;
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
		}
	}
	
	void WaitRight(SceneView scene_view)
	{
		if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.D)
		{
			SceneView.onSceneGUIDelegate -= WaitRight;
			SceneView.onSceneGUIDelegate += RotateRight;
		}
	}
	
	void WaitUp(SceneView scene_view)
	{
		if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.W)
		{
			SceneView.onSceneGUIDelegate -= WaitUp;
			SceneView.onSceneGUIDelegate += RotateUp;
		}
	}
	
	void WaitDown(SceneView scene_view)
	{
		if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.S)
		{
			SceneView.onSceneGUIDelegate -= WaitDown;
			SceneView.onSceneGUIDelegate += RotateDown;
		}
	}
}
