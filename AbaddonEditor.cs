using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(Abaddon))]
public class AbaddonEditor : Editor
{
	Abaddon							abaddon;

	void SubUpdate(SceneView sceneview)
	{
		//SceneView.lastActiveSceneView.LookAt(SceneView.lastActiveSceneView.pivot, Quaternion.LookRotation(Vector3.forward));
		//SceneView.lastActiveSceneView.AlignViewToObject(transform);
		//SceneView.lastActiveSceneView.camera.transform.position = Vector3.zero; //XXX: should be the one I want, then I just use LookAt
	}

	void GridUpdate(SceneView sceneview)
	{
		//GameObject zero = GameObject.Find("Zero");
		//sceneview.AlignViewToObject(zero.transform);
		//sceneview.Repaint();
		Event e = Event.current;

		Ray r = Camera.current.ScreenPointToRay(new Vector3(e.mousePosition.x, -e.mousePosition.y + Camera.current.pixelHeight));
		Vector3 mousePos = r.origin;
		
		if (e.isKey && e.character == 'a')
		{
			GameObject obj;
			Object prefab = PrefabUtility.GetPrefabParent(Selection.activeObject);
			
			if (prefab)
			{
				//Undo.IncrementCurrentEventIndex();
				obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
				PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
				Vector3 aligned = new Vector3(Mathf.Floor(mousePos.x/abaddon.width )*abaddon.width  + abaddon.width /2.0f,
				                              Mathf.Floor(mousePos.y/abaddon.height)*abaddon.height + abaddon.height/2.0f, 0.0f);
				obj.transform.position = aligned;
				Undo.RegisterCreatedObjectUndo(obj, "Create " + obj.name);
			}
		}
		else if (e.isKey && e.character == 'd')
		{
			//Undo.IncrementCurrentEventIndex();
			Undo.SetCurrentGroupName("Delete object set");
			foreach (GameObject obj in Selection.gameObjects)
			{
				Undo.DestroyObjectImmediate(obj);
				DestroyImmediate(obj);
			}
		}

		//if(e.isKey && e.character == ' ')
		//{
		//	SceneView.onSceneGUIDelegate -= SubUpdate;
		//	SceneView.onSceneGUIDelegate += SubUpdate;
		//} else if(e.isKey && up && e.character == ' ') {
		//	SceneView.onSceneGUIDelegate -= SubUpdate;
		//}
	}
	
	public void OnEnable()
	{
		abaddon = target as Abaddon;
		SceneView.onSceneGUIDelegate -= GridUpdate;
		SceneView.onSceneGUIDelegate += GridUpdate;
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
