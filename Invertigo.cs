using UnityEngine;
using System.Collections;

public class Invertigo : MonoBehaviour
{
	public int rows = 8;
	public int columns = 16;

	void OnDrawGizmos()
	{
		UnityEditor.Handles.color = Color.white;

		for(float row = 0f; row <= 1f; row += 1f/columns)
		{
			UnityEditor.Handles.DrawWireDisc(Vector3.down*Mathf.Cos(Mathf.PI*row), Vector3.up, Mathf.Sin(Mathf.PI*row));
		}
		
		for(float column = 0f; column <= 1f; column += 1f/rows)
		{
			UnityEditor.Handles.DrawWireDisc(Vector3.zero,
			                                 Vector3.forward*Mathf.Cos(Mathf.PI*column) +
			                                 Vector3.right  *Mathf.Sin(Mathf.PI*column),
			                                 1);
		}
	}
}