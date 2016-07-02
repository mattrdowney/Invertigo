using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class Door : Portal
{
    float direction;

    void OnTriggerStay()
    {
        if(System.Math.Sign(Input.GetAxis("Vertical")) == direction)
        {
            RoomLoader.Instance.EnterPortal(this); //FIXME: EnterPortal (door-specific code) should be implemented in the Door class too
        }
    }

	//Since the door should *not* be dependent on linking... an ArcCast could be used to find the terrain at the other end of the tunnel after the level is enabled.
}