using UnityEngine;

public class EscapeToQuit : MonoBehaviour
{
	void Update ()
    {
	    if (Input.GetKeyDown(KeyCode.Escape))
        {
            #if UNITY_WEBGL
            #else
            Application.Quit();
            #endif
        }     	
	}
}
