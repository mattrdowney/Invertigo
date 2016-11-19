using UnityEngine;
using System.Collections;

public abstract class Portal : MonoBehaviour
{
    public int near_id, far_id;
    public int direction;

    public abstract void EnterPortal(CharacterMotor motor); //Abstract Factory Pattern
}
