using UnityEngine;
using UnityEditor;
using System.Collections;

public static class InvertigoUtility
{
	public static void Align(SceneView scene_view)
	{
		Transform forward = GameObject.Find("/PivotYaw/PivotPitch/Zero").transform;

		scene_view.AlignViewToObject(forward);
		scene_view.camera.transform.position = Vector3.zero;
		scene_view.Repaint();
	}

	public static void AllowRotation()
	{
		SceneView.onSceneGUIDelegate += WaitLeft;
		SceneView.onSceneGUIDelegate += WaitRight;
		SceneView.onSceneGUIDelegate += WaitUp;
		SceneView.onSceneGUIDelegate += WaitDown;
	}

	public static void DisallowRotation()
	{
		SceneView.onSceneGUIDelegate += WaitLeft;
		SceneView.onSceneGUIDelegate += WaitRight;
		SceneView.onSceneGUIDelegate += WaitUp;
		SceneView.onSceneGUIDelegate += WaitDown;
		SceneView.onSceneGUIDelegate -= RotateLeft;
		SceneView.onSceneGUIDelegate -= RotateRight;
		SceneView.onSceneGUIDelegate -= RotateUp;
		SceneView.onSceneGUIDelegate -= RotateDown;
	}

	public static Vector3 CursorCast(Camera cam, Vector2 mousePos)
	{
		return cam.ScreenPointToRay(new Vector3(mousePos.x, cam.pixelHeight - mousePos.y, cam.nearClipPlane)).direction;
	}

	public static Vector3 CursorCast(Camera cam, Vector2 mousePos, int rows, int columns)
	{
		return Snap(CursorCast(cam, mousePos), rows, columns);
	}

	static Vector3 Snap(Vector3 position, float rows, float columns)
	{
		float phi = 0;
		float theta = 0;

		float desired_phi = Mathf.Asin(-position.y) + Mathf.PI/2;

		for(float row = 0; row <= rows + 1; ++row) //going over with off by one errors won't ruin the program...
		{
			float temp_phi = Mathf.Asin(Mathf.Cos(Mathf.PI*row/(rows+1))) + Mathf.PI/2;
			float error = Mathf.Abs(Mathf.DeltaAngle(desired_phi * Mathf.Rad2Deg, temp_phi * Mathf.Rad2Deg));
			float old_error = Mathf.Abs(Mathf.DeltaAngle(desired_phi * Mathf.Rad2Deg, phi * Mathf.Rad2Deg));

			if(error < old_error)
			{
				phi = temp_phi;
			}
		}

		float desired_theta = Mathf.Atan2(position.z, position.x);

		for(float column = 0; column < columns*2; ++column) //... but going under is bad
		{
			float temp_theta = column/columns*Mathf.PI;
			float error = Mathf.Abs(Mathf.DeltaAngle(desired_theta * Mathf.Rad2Deg, temp_theta * Mathf.Rad2Deg));
			float old_error = Mathf.Abs(Mathf.DeltaAngle(desired_theta * Mathf.Rad2Deg, theta * Mathf.Rad2Deg));

			if(error < old_error)
			{
				theta = temp_theta;
			}
		}

		Debug.Log("phi: " + phi + " theta: " + theta + " dphi: " + desired_phi + " dtheta: " + desired_theta);

		return SphereUtility.Position(Vector3.right, Vector3.forward, Vector3.up, phi, theta);
	}

	public static void RotateLeft(SceneView scene_view)
	{
		Transform yawTrans = GameObject.Find("/PivotYaw").transform;

		if(Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.A)
		{
			SceneView.onSceneGUIDelegate -= RotateLeft;
			SceneView.onSceneGUIDelegate += WaitLeft;
		}
		yawTrans.localRotation *= Quaternion.Euler(0f, -0.1f, 0f);
		Align(scene_view);
	}
	
	public static void RotateRight(SceneView scene_view)
	{
		Transform yawTrans = GameObject.Find("/PivotYaw").transform;

		if(Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.D)
		{
			SceneView.onSceneGUIDelegate -= RotateRight;
			SceneView.onSceneGUIDelegate += WaitRight;
		}
		yawTrans.localRotation *= Quaternion.Euler(0f, 0.1f, 0f);
		Align(scene_view);
	}
	
	public static void RotateUp(SceneView scene_view)
	{
		Transform pitchTrans = GameObject.Find("/PivotYaw/PivotPitch").transform;

		if(Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.W)
		{
			SceneView.onSceneGUIDelegate -= RotateUp;
			SceneView.onSceneGUIDelegate += WaitUp;
		}
		pitchTrans.localRotation *= Quaternion.Euler(-0.1f, 0f, 0f);
		Align(scene_view);
	}
	
	public static void RotateDown(SceneView scene_view)
	{
		Transform pitchTrans = GameObject.Find("/PivotYaw/PivotPitch").transform;

		if(Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.S)
		{
			SceneView.onSceneGUIDelegate -= RotateDown;
			SceneView.onSceneGUIDelegate += WaitDown;
		}
		pitchTrans.localRotation *= Quaternion.Euler(0.1f, 0f, 0f);
		Align(scene_view);
	}
	
	public static void WaitLeft(SceneView scene_view)
	{
		if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.A)
		{
			SceneView.onSceneGUIDelegate -= WaitLeft;
			SceneView.onSceneGUIDelegate += RotateLeft;
		}
	}
	
	public static void WaitRight(SceneView scene_view)
	{
		if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.D)
		{
			SceneView.onSceneGUIDelegate -= WaitRight;
			SceneView.onSceneGUIDelegate += RotateRight;
		}
	}
	
	public static void WaitUp(SceneView scene_view)
	{
		if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.W)
		{
			SceneView.onSceneGUIDelegate -= WaitUp;
			SceneView.onSceneGUIDelegate += RotateUp;
		}
	}
	
	public static void WaitDown(SceneView scene_view)
	{
		if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.S)
		{
			SceneView.onSceneGUIDelegate -= WaitDown;
			SceneView.onSceneGUIDelegate += RotateDown;
		}
	}
}
