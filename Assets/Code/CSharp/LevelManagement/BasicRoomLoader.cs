using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

class BasicRoomLoader : RoomLoader
{
    Queue<AsyncOperation> level_requests;

    public override bool IsDone()
    {
        while(level_requests.Count != 0)
        {
            AsyncOperation request = level_requests.Dequeue();

            if(!request.isDone)
            {
                level_requests.Enqueue(request);
                return false;
            }
        }

        level_requests = null;
        return true;
    }

    public override void Setup()
    {
        level_requests = new Queue<AsyncOperation>();

        int max_levels = 2;

        for (int level = 1; level <= max_levels; ++level)
        {
            AsyncOperation request = SceneManager.LoadSceneAsync(level, LoadSceneMode.Additive);
            request.allowSceneActivation = true;
            request.priority = 1;
            level_requests.Enqueue(request);
        }
    }
}
