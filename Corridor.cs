using UnityEngine;
using System.Collections;

public class Corridor : Nexus
{
    public float interpolation_distance;

    public override void Move(float delta, CharacterMotor motor)
    {
        interpolation_distance += delta*direction*Time.deltaTime;

        if(interpolation_distance < 0)
        {
            motor.GetComponent<SphereCollider>().enabled = true;
            Destroy(this.gameObject);
            motor.ExitNexus();
        }
        else if(interpolation_distance > 10f)
        {
            RoomLoader.Instance.UnloadRoom(near_id);
            RoomLoader.Instance.LoadRoom(far_id);
            motor.GetComponent<SphereCollider>().enabled = true;
            Destroy(this.gameObject);
            motor.ExitNexus();
        }
    }

    protected override void ExitNexus(CharacterMotor motor)
    {
        //TODO:
    }
}
