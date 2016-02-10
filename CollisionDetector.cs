using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CollisionDetector : Component
{
	List<SphericalIsoscelesTrapezoid>	colliders;

	//step 0: Character Controller adds the observed SphericalIsoscelesTriangle to a vector in OnCollisionEnter...
	void OnCollisionEnter(Collision collisions)
	{
		foreach(ContactPoint col in collisions.contacts)
		{
			SphericalIsoscelesTrapezoid trap = col.otherCollider.gameObject.GetComponent<SphericalIsoscelesTrapezoid>();
			if(trap) colliders.Add(trap);
		}
	}

	//step 0.5: Character Controller removes the observed SphericalIsoscelesTriangle to a vector in OnCollisionEnter...
	void OnCollisionExit(Collision collisions)
	{
		foreach(ContactPoint col in collisions.contacts)
		{
			SphericalIsoscelesTrapezoid trap = col.otherCollider.gameObject.GetComponent<SphericalIsoscelesTrapezoid>();
			if(trap) colliders.Remove(trap);
		}
	}

	public Optional<SphericalIsoscelesTrapezoid> Update(Vector3 desiredPos, Vector3 curPos)
	{
		Optional<SphericalIsoscelesTrapezoid> closest = new Optional<SphericalIsoscelesTrapezoid>();
		Optional<float> 					  closestDistance = new Optional<float>();
		
		//Step 1: go through each colliding segment
		foreach(SphericalIsoscelesTrapezoid trap in colliders)
		{
			//step 2: Character Controller asks the block if a collision actually occuring in Spherical coordinates
			if(trap.Contains(desiredPos))
			{
				//step 3: if a collision is happening, a list of TTCs (time till collision) are sorted to find the closest collision.
				Optional<float> distance = trap.Distance(desiredPos, curPos);
				if(distance.HasValue && (!closestDistance.HasValue || distance.Value < closestDistance.Value))
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
