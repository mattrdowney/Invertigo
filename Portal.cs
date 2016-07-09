using UnityEngine;
using System.Collections;

public abstract class Portal : MonoBehaviour
{
    public int near_id, far_id;

    public abstract Nexus EnterPortal(); //Abstract Factory Pattern
}
