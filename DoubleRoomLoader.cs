using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

class DoubleRoomLoader : IRoomLoader
{
    public void EnterNexus(Nexus connection)
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

    public void EnterRoom(Nexus connection)
    {

    }

    public void Interpolate()
    {

    }

    public bool IsDone()
    {
        return true;
    }

    public void Setup()
    {

    }
}
