using UnityEngine;
using System.Collections;

public static class BlockUtility
{
	public static Vector3 Plastic(SphericalIsoscelesTrapezoid trap, CharacterMotor charMotor)
	{
		float traction = Vector3.Dot(charMotor.gravity, charMotor.normal);
		Vector3 gravity = Vector3.ProjectOnPlane(charMotor.gravity, charMotor.normal);

		charMotor.velocity += gravity + traction*charMotor.right;
		//thought experiment, what if there is a vertical orbit that doesn't go through the North/South pole. In that case, the gravity will swap from up to down in the middle of the orbit. How do you handle: 1) derailing 2) velocity projection
	}

	public static Vector3 Metal(CharacterMotor charMotor)
	{
		
	}

}
