using UnityEngine;
using UnityEditor;
using System.Collections;

abstract public class Block : Component
{
	BlockMotor						motor;

	static int						guid = 0;

	public virtual Vector3 Evaluate(CharacterMotor charMotor)
	{
		return Vector3.zero;
	}

	public static GameObject Spawn()
	{
		GameObject prefab = (GameObject) Resources.Load("BlockPrefab");
		
		#if UNITY_EDITOR
		GameObject obj = UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as GameObject;
		#else
		GameObject obj = Instantiate(prefab) as GameObject;
		#endif

		Undo.RegisterCreatedObjectUndo(obj, "Created block");
		
		obj.name = guid.ToString();
		
		guid++;
		
		return obj;
	}
}

//metal
//plastic
//glass
//electrified metal (aka wire)
//corroded metal //requires MonoBehaviour
//tread
//pushable plastic block
//magnetic

//block->metal

//metal->plastic
//metal->corroded metal
//metal->wire
//metal->magnetic (old mud block)

//plastic->tread
//plastic->pushable plastic
//plastic->glass