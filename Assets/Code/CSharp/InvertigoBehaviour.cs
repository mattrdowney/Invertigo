using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvertigoBehaviour : MonoBehaviour
{
    protected /*sealed virtual*/ void OnTriggerEnter(Collider other) { } // I could be jank and hide these the rude way...
    protected /*sealed virtual*/ void OnTriggerExit(Collider other) { }

    protected /*sealed virtual*/ void OnCollisionEnter(Collision other) { }
    protected /*sealed virtual*/ void OnCollisionStay(Collision other) { }
    protected /*sealed virtual*/ void OnCollisionExit(Collision other) { }

    protected /*sealed*/ virtual void OnTriggerStay(Collider other)
    {
		if (true)
        {
            OnArcEnter(arc_info);
        }
        else if (true)
        {
            OnArcExit(arc_info);
        }
        if (true)
        {
            OnArcStay(arc_info);
        }
        if (true)
        {
            OnZoneEnter(zone_info);
        }
        else if(true)
        {
            OnZoneExit(zone_info);
        }
        if (true)
        {
            OnZoneStay(zone_info);
        }
    }

    protected virtual void OnArcEnter(ArcInfo arc_info) { }
    protected virtual void OnArcStay(ArcInfo arc_info) { }
    protected virtual void OnArcExit(ArcInfo arc_info) { }

    protected virtual void OnZoneEnter(ZoneInfo zone_info) { }
    protected virtual void OnZoneStay(ZoneInfo zone_info) { }
    protected virtual void OnZoneExit(ZoneInfo zone_info) { }
}