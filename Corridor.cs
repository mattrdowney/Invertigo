using UnityEngine;
using System.Collections;

public class Corridor : Nexus
{
    public float interpolation_distance;
    public float start_size, end_size;

    public Vector3 up, forward;

    public override void Move(float delta, CharacterMotor motor)
    {
        interpolation_distance += delta*direction*Time.deltaTime;

        float exponent = Mathf.Clamp01(interpolation_distance / 10);
        motor.radius = Mathf.Pow(start_size, 1 - exponent) * Mathf.Pow(end_size, exponent);

        Vector2 phi_theta = new Vector2(motor.radius, 0f);
        motor.transform.position = SphereUtility.SphereToCartesian(phi_theta, up, up, forward);

        if(interpolation_distance < 0)
        {
            ExitNexus(motor);
        }
        else if(interpolation_distance > 10f)
        {
            RoomLoader.Instance.UnloadRoom(near_id);
            RoomLoader.Instance.LoadRoom(far_id);
            ExitNexus(motor);
        }
    }

    protected override void ExitNexus(CharacterMotor motor) //JANK naming scheme (Note: motor.ExitNexus())
    {
        //TODO: check if function is complete
        motor.ExitNexus();

        CollisionDetector detector = motor.GetComponent<CollisionDetector>();
        detector.Activate();
        optional<ArcOfSphere> closest_arc = detector.BalloonCast(motor.current_position, motor.radius + 0.01f); //FIXME: magic number
        if(!closest_arc.exists)
        {
            Debug.Log("Something is very wrong here");
        }
        motor.Traverse(closest_arc.data, closest_arc.data.ClosestPoint(motor.current_position));

        Destroy(this.gameObject);
    }
}
