using UnityEngine;
using System.Diagnostics;

public class Invertigo : MonoBehaviour
{
	public int rows = 15; //equator drawn when rows is odd
	public int columns = 16; //RENAME?: misleading //always draws 2*columns lines from the North to South pole

    [Conditional("UNITY_EDITOR")]
	void OnDrawGizmos()
	{
        #if UNITY_EDITOR
		UnityEditor.Handles.color = Color.white;

		for(float row = 1; row <= rows; ++row)
		{
			UnityEditor.Handles.DrawWireDisc(Vector3.down*Mathf.Cos(Mathf.PI*row/(rows+1)),
			                                 Vector3.up,
			                                 Mathf.Sin(Mathf.PI*row/(rows+1)));
		}
		
		for(float column = 0; column < columns; ++column)
		{
			UnityEditor.Handles.DrawWireDisc(Vector3.zero,
			                                 Vector3.forward*Mathf.Cos(Mathf.PI*column/columns) +
			                                 Vector3.right  *Mathf.Sin(Mathf.PI*column/columns),
			                                 1);
		}
        #endif
	}
}