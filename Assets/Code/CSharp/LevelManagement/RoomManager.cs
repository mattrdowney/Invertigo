using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

class RoomManager : MonoBehaviour
{
    Dictionary<int, AsyncOperation> levels;

    public AsyncOperation RequestLoad(int level_index)
    {
        if(levels.ContainsKey(level_index))
        {
            return levels[level_index];
        }
        AsyncOperation request = SceneManager.LoadSceneAsync(level_index, LoadSceneMode.Additive);
        request.allowSceneActivation = true;
        request.priority = 1;

        levels.Add(level_index, request);

        return request;
    }

    void UnloadRegion()
    {

    }
}