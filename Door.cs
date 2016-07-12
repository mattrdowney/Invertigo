using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections;
using System.Collections.Generic;

public class Door : Portal
{
    void OnTriggerStay(Collider other)
    {
        if(System.Math.Sign(Input.GetAxis("Vertical")) == direction)
        {
            CharacterMotor motor = other.gameObject.GetComponent<CharacterMotor>();

            if(motor.grounded)
            {
                EnterPortal(motor);
            }
        }
    }

    public override void EnterPortal(CharacterMotor motor)
    {
        GameObject instance = GameObject.Instantiate(Resources.Load("CorridorPrefab")) as GameObject;
        Nexus result = instance.GetComponent<Corridor>();
        result.near_id = this.near_id;
        result.far_id  = this.far_id;
        result.direction = this.direction;
        motor.EnterNexus(result);
    }

	//Since the door should *not* be dependent on linking... an ArcCast could be used to find the terrain at the other end of the tunnel after the level is enabled.
}