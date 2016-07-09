using UnityEngine;
using System.Collections;

public abstract class Nexus : MonoBehaviour
{
    public int near_id, far_id;

    public abstract void Move(float delta, Player avatar);

    protected abstract void ExitNexus(Player avatar);
}
