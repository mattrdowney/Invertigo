using UnityEngine;
using System.Collections;

public abstract class Nexus : MonoBehaviour
{
    public int near_id, far_id;
    public int direction;

    public abstract void Move(float delta, CharacterMotor motor);

    protected abstract void ExitNexus(CharacterMotor motor);
}