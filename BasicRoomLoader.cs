using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

class BasicRoomLoader : RoomLoader
{
    Queue<AsyncOperation> level_requests;

    public override void EnterPortal(Portal link)
    {

    }

    public override void ExitNexus(Nexus connection)
    {

    }

    public override void Interpolate()
    {

    }

    public override bool IsDone()
    {
        while(level_requests.Count != 0)
        {
            Debug.Log(level_requests.Count);

            AsyncOperation request = level_requests.Dequeue();

            Debug.Log(request.progress);

            if(!request.isDone)
            {
                level_requests.Enqueue(request);
                Debug.Log("returning false");
                return false;
            }
        }

        level_requests = null;
        Debug.Log("returning true");
        return true;
    }

    public override void Setup()
    {
        Debug.Log("Setup begin");

        level_requests = new Queue<AsyncOperation>();

        int max_levels = 1;

        for (int level = 1; level <= max_levels; ++level)
        {
            AsyncOperation request = SceneManager.LoadSceneAsync(level, LoadSceneMode.Additive);
            request.allowSceneActivation = true;
            request.priority = 1;
            level_requests.Enqueue(request);
        }

        Debug.Log("Setup end");
    }
}
