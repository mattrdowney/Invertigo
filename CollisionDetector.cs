using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CollisionDetector : Component
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
			ArcOfSphere trap = col.otherCollider.gameObject.GetComponent<ArcOfSphere>();
			if(trap) colliders.Add(trap);
		}
	}

	//step 0.5: Character Controller removes the observed SphericalIsoscelesTriangle to a vector in OnCollisionEnter...
	void OnCollisionExit(Collision collisions)
	{
		foreach(ContactPoint col in collisions.contacts)
		{
			ArcOfSphere trap = col.otherCollider.gameObject.GetComponent<ArcOfSphere>();
			if(trap) colliders.Remove(trap);
		}
	}

	public optional<ArcOfSphere> Update(Vector3 desiredPos, Vector3 curPos, float radius)
	{
		optional<ArcOfSphere> closest = new optional<ArcOfSphere>();
		optional<float> closestDistance = new optional<float>();
		
		//Step 1: go through each colliding segment
		foreach(ArcOfSphere trap in colliders)
		{
			//step 2: Character Controller asks the block if a collision actually occuring in Spherical coordinates
			if(trap.Contains(desiredPos, radius))
			{
				//step 3: if a collision is happening, a list of TTCs (time till collision) are sorted to find the closest collision.
				optional<float> distance = trap.Distance(desiredPos, curPos, radius);
				if(distance.exists && (!closestDistance.exists || distance.data < closestDistance.data))
				{
					closestDistance = distance;
					closest = trap;
				}
			}
		}
		
		//step 4: the player moves in contact with the object and performs camera transitions accordingly if there was a collision.
		return closest; //charMotor.Traverse(closest, desiredPos, curPosition);
	}
}
