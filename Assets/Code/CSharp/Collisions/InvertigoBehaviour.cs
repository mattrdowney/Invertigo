using UnityEngine;

public class InvertigoBehaviour : MonoBehaviour
{
    float radius;
    const int geometry_mask = 98304; //0b1'1000'0000'0000'0000;//(1 << LayerMask.NameToLayer("Geometry Normal")) & (1 << LayerMask.NameToLayer("Geometry Transition"));

    CollisionDetector arc_detector;
    ZoneDetector zone_detector;

    protected /*sealed*/ virtual void OnCollisionEnter(Collision other) { }
    protected /*sealed*/ virtual void OnCollisionStay(Collision other) { }
    protected /*sealed*/ virtual void OnCollisionExit(Collision other) { }

    protected /*sealed*/ virtual void OnTriggerStay()
    {
        arc_detector.OnEnter(OnArcEnter);
        arc_detector.OnExit(OnArcExit);
        arc_detector.OnStay(OnArcStay);

        ZoneInfo zone_info;

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

    protected /*sealed*/ virtual void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & geometry_mask) != 0)
        {
            arc_detector.OnTriggerEnter(other);
        }
        else
        {
            zone_detector.OnTriggerEnter(other);
        }
    }

    protected /*sealed*/ virtual void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & geometry_mask) != 0)
        {
            arc_detector.OnTriggerExit(other);
        }
        else
        {
            zone_detector.OnTriggerExit(other);
        }
    }

    protected virtual void OnArcEnter(ArcInfo arc_info) { }
    protected virtual void OnArcStay(ArcInfo arc_info) { }
    protected virtual void OnArcExit(ArcInfo arc_info) { }

    protected virtual void OnZoneEnter(ZoneInfo zone_info) { }
    protected virtual void OnZoneStay(ZoneInfo zone_info) { }
    protected virtual void OnZoneExit(ZoneInfo zone_info) { }
}