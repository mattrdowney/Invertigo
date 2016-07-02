using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

abstract class RoomLoader : MonoBehaviour //use Strategy Pattern //FIXME: Entire file is JANK
{
    int initial_level = 1;

    // Static singleton instance //all credit for Singleton pattern to http://clearcutgames.net/home/?p=437
    private static RoomLoader instance;

    // Static singleton property
    public static RoomLoader Instance
    {
        // Here we use the ?? operator, to return 'instance' if 'instance' does not equal null
        // otherwise we assign instance to a new component and return that
        get { return instance ?? (instance = new GameObject("RoomLoader").AddComponent<BasicRoomLoader>()); }
    }

    public void Start()
    {
        Setup();

        StartCoroutine(Wait());
    }

    public abstract void EnterPortal(Portal link);
    public abstract void ExitNexus(Nexus connection);
    public abstract void Interpolate();
    public abstract bool IsDone();
    public abstract void Setup();

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
        yield return new WaitUntil(() => IsDone());

        LoadRoom(initial_level);

        Time.timeScale = 1;
    }
}
