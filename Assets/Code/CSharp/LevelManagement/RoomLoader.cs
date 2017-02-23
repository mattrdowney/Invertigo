using UnityEngine;

using System.Collections;

abstract class RoomLoader : MonoBehaviour //use Strategy Pattern //FIXME: Entire file is JANK
{
    int initial_level = 1;

    // Static singleton instance // credit for this implementation of the Singleton pattern to http://clearcutgames.net/home/?p=437
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
        if(instance)
        {
            Debug.Log("Fatal Err0r");
        }
        else
        {
            instance = this;
        }

        Setup();

        StartCoroutine(Wait());
    }

    public abstract bool IsDone();
    public abstract void Setup();

    private void ToggleRoom(int level, bool state)
    {
        GameObject geometry_root = GameObject.Find("/" + level.ToString());
        GameObject graphics_root = GameObject.Find("/" + level.ToString() + "g");


        // geometry
        for (int child_id = 0; child_id < geometry_root.transform.childCount; child_id++)
        {
            geometry_root.transform.GetChild(child_id).gameObject.SetActive(state);
        }

        // graphics
        for (int child_id = 0; child_id < graphics_root.transform.childCount; child_id++)
        {
            graphics_root.transform.GetChild(child_id).gameObject.SetActive(state);
        }
    }

    public void LoadRoom(int level)
    {
        ToggleRoom(level, true);
    }

    public void UnloadRoom(int level)
    {
        ToggleRoom(level, false);
    }

    IEnumerator Wait()
    {
        yield return new WaitUntil(() => IsDone());

        LoadRoom(initial_level);

        Time.timeScale = 1;
    }
}