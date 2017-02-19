using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections;
using System.Collections.Generic;

public class Door : Portal
{
    public float start_size, end_size;

    void OnTriggerStay(Collider other)
    {
        if(System.Math.Sign(Input.GetAxis("Vertical")) == direction)
        {
            CharacterMotor motor = other.gameObject.GetComponent<CharacterMotor>();

            if(motor && motor.grounded)
            {
                motor.Jump();
                RoomLoader.Instance.UnloadRoom(1);
                RoomLoader.Instance.LoadRoom(2);
                //RoomManager.RequestLoad()EnterPortal(motor);
            }
        }
    }

    public override void EnterPortal(CharacterMotor motor)
    {
        GameObject instance = GameObject.Instantiate(Resources.Load("CorridorPrefab")) as GameObject;
        Corridor result = instance.GetComponent<Corridor>();
        Vector3 up = motor.arc.EvaluateNormal(motor.angle);
        Vector3 forward = motor.arc.Evaluate(motor.angle);
        result.up = up;
        result.forward = forward;
        result.near_id = this.near_id;
        result.far_id  = this.far_id;
        result.direction = this.direction;
        result.start_size = this.start_size;
        result.end_size = this.end_size;
        motor.EnterNexus(result);
    }

	//Since the door should *not* be dependent on linking... a BalloonCast should be used to find the terrain at the other end of the tunnel after the level is enabled.
}