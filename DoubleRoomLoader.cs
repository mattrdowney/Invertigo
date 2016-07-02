using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

class DoubleRoomLoader : RoomLoader
{
    public override void EnterPortal(Portal link)
    {
       /*GameObject last_room = loaded_rooms[connection.near_id];
       loaded_rooms.Clear();
       loaded_rooms.Add(connection.near_id, last_room);
       if (!loading_rooms.ContainsKey(connection.far_id))
       {
           AsyncOperation request = SceneManager.LoadSceneAsync(connection.far_id, LoadSceneMode.Additive);
           request.allowSceneActivation = true;
           loading_rooms.Add(connection.far_id, request);
       }*/
    }

    public override void ExitNexus(Nexus connection)
    {

    }

    public override void Interpolate()
    {

    }

    public override bool IsDone()
    {
        return true;
    }

    public override void Setup()
    {

    }

    /*public void RequestLoad()
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
    }*/
}
