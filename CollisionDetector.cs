using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//CONSIDER: use CapsuleCollider! imprecise but pretty badass
//CONSIDER2: use expanded CapsuleCollider to account for curvature!
//CONSIDER3: use AABB going from position1 to position2 //preferred //YESH no frame speed limit!
//NOTA BENE: CONSIDER3 has a bug! fix it with (well-optimized) math or logic

public class CollisionDetector : MonoBehaviour
{
	List<ArcOfSphere>	colliders;

	void Awake()
	{
		colliders = new List<ArcOfSphere>();
	}

	//step 0: Character Controller adds the observed SphericalIsoscelesTriangle to a vector in OnCollisionEnter...
	void OnCollisionEnter(Collision collisions)
	{
		foreach(ContactPoint col in collisions.contacts)
		{
			ArcOfSphere arc = col.otherCollider.gameObject.GetComponent<ArcOfSphere>();
			if(arc && !colliders.Contains(arc)) colliders.Add(arc);
		}
	}

	//step 0.5: Character Controller removes the observed SphericalIsoscelesTriangle to a vector in OnCollisionEnter...
	void OnCollisionExit(Collision collisions)
	{
		foreach(ContactPoint col in collisions.contacts)
		{
			ArcOfSphere arc = col.otherCollider.gameObject.GetComponent<ArcOfSphere>();
			if(arc) colliders.Remove(arc);
		}
	}

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
}

/*stating the obvious:

If I can make a cool space partioning scheme for angular space that would allow for average log(n) time for a true ArcCast against all geometry, that would be godly.
Making a true ArcCastAll would be amazing as well. One that can take multiple rotations and lesser circles into account and return a list of all collisions
...and how many times they happen and in what order
ArcCast would use SphereUtility.Intersection with optional<Vector3>

There really needs to be a distinction between Triggers and Colliders, although I can probably just make another class for that

 */
