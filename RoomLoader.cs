using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

static class RoomLoader //use Strategy Pattern
{
    enum MaxRooms { kInfinite = -1, kTwo = 2 };

    static MaxRooms room_limit;



    static Dictionary<int, GameObject> loaded_rooms;
    static Dictionary<int, AsyncOperation> loading_rooms;

	static void EnterNexus(Nexus connection)
    {
        if (room_limit == MaxRooms.kTwo)
        {
            GameObject last_room = loaded_rooms[connection.near_id];
            loaded_rooms.Clear();
            loaded_rooms.Add(connection.near_id, last_room);
            if (!loading_rooms.ContainsKey(connection.far_id))
            {
                AsyncOperation request = SceneManager.LoadSceneAsync(connection.far_id, LoadSceneMode.Additive);
                request.allowSceneActivation = true;
                loading_rooms.Add(connection.far_id, request);
            }
        }
    }

    static void EnterRoom(Nexus connection)
    {
        UpdateThreads();
        //if()
    }

    static void UpdateThreads()
    {
        foreach(AsyncOperation thread in loading_rooms.Values)
        {
            if(thread.isDone)
            {
                //thread.
            }
        }
    }
}
