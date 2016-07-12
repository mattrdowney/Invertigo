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
        optional<ArcOfSphere> closest_arc = detector.BaloonCast(motor.current_position, motor.radius + 0.01f); //FIXME: magic number
        if(!closest_arc.exists)
        {
            Debug.Log("Something is very wrong here");
        }
        motor.Traverse(closest_arc.data, closest_arc.data.ClosestPoint(motor.current_position));

        Destroy(this.gameObject);
    }
}
