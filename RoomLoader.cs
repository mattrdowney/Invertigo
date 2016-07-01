using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

class RoomLoader : MonoBehaviour //use Strategy Pattern
{
    IRoomLoader strategy;

    int initial_level = 1;

    public void Start()
    {
        strategy = new BasicRoomLoader();

        strategy.Setup();

        StartCoroutine(Wait());
    }

    private void LoadRoom(int level)
    {
        GameObject root = GameObject.Find("/" + level.ToString());

        root.SetActiveRecursively(true);
    }

    private void UnloadRoom(int level)
    {
        GameObject root = GameObject.Find("/" + level.ToString());

        root.SetActiveRecursively(false); // make all objects stop updating inside room
        root.SetActive(true); //keep the root object loaded so it can be found with GameObject.Find
    }

    IEnumerator Wait()
    {
        yield return new WaitUntil(() => strategy.IsDone());

        LoadRoom(initial_level);

        Time.timeScale = 1;
    }
}
