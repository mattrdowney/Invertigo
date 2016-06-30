using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

class RoomLoader : MonoBehaviour //use Strategy Pattern
{
    IRoomLoader strategy;

    public void Start()
    {
        strategy = new BasicRoomLoader();

        strategy.Setup();

        StartCoroutine(Wait());
    }

    IEnumerator Wait()
    {
        yield return new WaitUntil(() => strategy.IsDone());
    }
}
