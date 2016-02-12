using UnityEngine;
using System.Collections;

public class Abaddon : MonoBehaviour //http://code.tutsplus.com/tutorials/how-to-add-your-own-tools-to-unitys-editor--active-10047
{
	public Object 		prefab;

	private void OnDrawGizmos()
	{
		//UnityEditor.Handles.color = Color.yellow;

		//UnityEditor.Handles.DrawWireArc(Vector3.zero, Vector3.back, Vector3.up, 30f, 1f);
	}
}