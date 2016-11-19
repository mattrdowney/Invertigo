using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//CONSIDER: use AABB going from position1 to position2 //no frame speed limit!
//NOTA BENE: has a bug! fix it with (well-optimized) math or logic

public class CollisionDetector : MonoBehaviour
{
	public List<ArcOfSphere>	colliders;

	void Awake()
	{
		colliders = new List<ArcOfSphere>();
        Activate();
	}

    //step 0: Character Controller adds the observed SphericalIsoscelesTriangle to a vector in OnTriggerEnter...
    void OnTriggerEnter(Collider col) //idk why I did collision, tbh
	{
		ArcOfSphere arc = col.gameObject.GetComponent<ArcOfSphere>();
        if (arc && !colliders.Contains(arc))
        {
            colliders.Add(arc);
        }
	}

    //step 0.5: Character Controller removes the observed SphericalIsoscelesTriangle from a vector in OnTriggerExit...
    void OnTriggerExit(Collider col) //FIXME: not deleting
    {
		ArcOfSphere arc = col.gameObject.GetComponent<ArcOfSphere>();
        if(arc)
        {
            colliders.Remove(arc);
        }
	}

    public void Activate()
    {
        SphereCollider region = GetComponent<SphereCollider>();

        Collider[] arc_objects = Physics.OverlapSphere(region.transform.position + region.center, region.transform.localScale.x * region.radius);

        region.enabled = true;

        foreach (Collider arc_object in arc_objects)
        {
            ArcOfSphere arc = arc_object.gameObject.GetComponent<ArcOfSphere>();
            if (arc)
            {
                colliders.Add(arc);
            }
        }
    }

    public void Deactivate()
    {
        SphereCollider region = GetComponent<SphereCollider>();

        region.enabled = false;

        colliders.Clear();
    }

    //CONSIDER: Can you "inversion of control" ArcCast and BalloonCast?
    public optional<ArcOfSphere> ArcCast(Vector3 desired_position, Vector3 curPos, float radius) //Not actually a true ArcCast, I'm not planned on spending 3 months on R&D'ing it either
	{
		optional<ArcOfSphere> closest = new optional<ArcOfSphere>();
		optional<float> closest_distance = new optional<float>();
		
		//Step 1: go through each colliding arc
		foreach(ArcOfSphere arc in colliders)
		{
			//step 2: Character Controller asks the block if a collision actually occuring in Spherical coordinates
			if(arc.Contains(desired_position, radius))
			{
				//step 3: if a collision is happening, a list of TTCs (time till collision) are sorted to find the closest collision.
				optional<float> distance = arc.Distance(desired_position, curPos, radius);
				if(distance.exists && (!closest_distance.exists || distance.data < closest_distance.data))
				{
					closest_distance = distance;
					closest = arc;
				}
			}
		}
		
		//step 4: the player moves in contact with the object and performs camera transitions accordingly if there was a collision.
		return closest; //charMotor.Traverse(closest, desiredPos, curPosition);
	}

    public optional<ArcOfSphere> BalloonCast(Vector3 desired_position, float max_radius) //TODO: see if ArcCast can be combined to reduce redundancy //HACK: may not work for all or even most cases
    {
        optional<ArcOfSphere> closest = new optional<ArcOfSphere>();
        optional<Vector3> closest_point = new optional<Vector3>();

        //Step 1: go through each colliding arc
        foreach (ArcOfSphere arc in colliders)
        {
            //step 2: ask the block if a collision actually occuring in Spherical coordinates
            if (arc.Contains(desired_position, max_radius))
            {
                //step 3: if a collision is happening, find the closest collision point and compare it to the closest so far
                Vector3 point = arc.ClosestPoint(desired_position);
                if (!closest_point.exists || (point - desired_position).sqrMagnitude < (closest_point.data - desired_position).sqrMagnitude)
                {
                    closest_point = point;
                    closest = arc;
                }
            }
        }

        //step 4: the player moves in contact with the object and performs camera transitions accordingly if there was a collision.
        return closest;
    }
}

/*stating the obvious:

If I can make a cool space partioning scheme for angular space that would allow for average log(n) time for a true ArcCast against all geometry, that would be godly.
Making a true ArcCastAll would be amazing as well. One that can take multiple rotations and lesser circles into account and return a list of all collisions
...and how many times they happen and in what order
ArcCast would use SphereUtility.Intersection with optional<Vector3>

There really needs to be a distinction between Triggers and Colliders, although I can probably just make another class for that

 */
