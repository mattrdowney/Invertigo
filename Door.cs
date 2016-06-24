using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class Door : Portal
{
    public int far_id;
    /*DoorManager manager;

    void Start()
    {
        mode = Mode.kIdle;
    }


	void Update ()
	{
        
	}

    public void RequestLoad()
    {
        if (mode == Mode.kIdle)
        {
            BeginRequest();
        }
        else if (mode == Mode.kLoadToCancel) //This is a really clunky idea of loading to cancel, maybe I could unload based on a priority system
        {
            mode = Mode.kLoad;
            request.priority = 1;
        }
    }

    void BeginRequest()
    {
        request = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
        request.allowSceneActivation = true;
        request.priority = 1;
    }

    void EndRequest()
    {
        FindAndClose(near);
    }

    public void RequestCancel()
    {
        if (request.isDone)
        {
            EndRequest();
        }
        else
        {
            mode = Mode.kLoadToCancel;
            request.priority = 0;
        }
    }

    void CancelRequest()
    {
        FindAndClose(far);
        mode = Mode.kIdle;
        request = null;
    }

    static void FindAndClose(string name)
    {
        GameObject root = GameObject.Find(name);
    }

	//Doors don't need to follow the Singleton pattern! (there can be two if everything is done properly!)
	//When up or down is pressed near a door, the player walks into the foreground / background
	//When this happens, the game uses LoadLevelAdditiveAsynchronous to load the corresponding level and the level is disabled.
	//After the level finishes loading, the level can be linked (probably bad)
	//Since the door should *not* be dependent on linking... an ArcCast could be used to find the terrain at the other end of the tunnel after the level is enabled.
*/
}