using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class Door : Portal
{
    public float direction;

    void OnTriggerStay()
    {
        Debug.Log("Inside");
        if(System.Math.Sign(Input.GetAxis("Vertical")) == direction)
        {
            Debug.Log("Entering");
            Nexus connection = EnterPortal();

        }
    }

    public override Nexus EnterPortal()
    {
        Nexus result = new Corridor(); //FIXME: new being used to instantiate MonoBehaviour!!!
        result.near_id = this.near_id;
        result.far_id = this.far_id;
        return result;
    }

	//Since the door should *not* be dependent on linking... an ArcCast could be used to find the terrain at the other end of the tunnel after the level is enabled.
}