using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CollisionDetector : Component
{
	CharacterMotor						charMotor; //FIXME: unwanted coupling
	
	List<SphericalIsoscelesTrapezoid>	colliders;

	bool dirty_bit = true;

	public void Update()
	{
		if(dirty_bit)
		{
			Optional<SphericalIsoscelesTrapezoid> closest = new Optional<SphericalIsoscelesTrapezoid>();
			Optional<float> 					  closestDistance = new Optional<float>();
			
			foreach(SphericalIsoscelesTrapezoid trap in colliders)
			{
				if(trap.Contains(charMotor))
				{
					Optional<float> distance = trap.Distance(charMotor);
					if(distance.HasValue && (!closestDistance.HasValue || distance.Value < closestDistance.Value))
					{
						closestDistance = distance;
						closest = trap;
					}
				}
			}
			charMotor.segment = closest;
			charMotor.curPosition = closest.Value.Evaluate(charMotor);

			dirty_bit = false;
		}
	}

	void OnCollisionEnter(Collision collisions)
	{
		foreach(ContactPoint col in collisions.contacts)
		{
			SphericalIsoscelesTrapezoid trap = col.otherCollider.gameObject.GetComponent<SphericalIsoscelesTrapezoid>();
			if(trap) colliders.Add(trap);
		}
		dirty_bit = true;
	}
	
	void OnCollisionExit(Collision collisions)
	{
		foreach(ContactPoint col in collisions.contacts)
		{
			SphericalIsoscelesTrapezoid trap = col.otherCollider.gameObject.GetComponent<SphericalIsoscelesTrapezoid>();
			if(trap) colliders.Remove(trap);
		}
		dirty_bit = true;
	}
}
