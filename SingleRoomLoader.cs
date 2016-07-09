using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

class SingleRoomLoader : RoomLoader
{
    public override bool IsDone()
    {
        return true;
    }

    public override void Setup()
    {

    }
}
