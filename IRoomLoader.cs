using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

interface IRoomLoader //use Strategy Pattern
{
    void EnterNexus(Nexus connection);
    void EnterRoom(Nexus connection);
    void Interpolate();
    bool IsDone();
    void Setup();
}
