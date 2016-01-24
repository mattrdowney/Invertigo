using UnityEngine;
using System.Collections;

public class Enemy : Character {

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}


//enemy concepts:
//patrol in direction and reflect at boundary
//slime that traverses in direction regardless of gravity (impermeable to certain damage types)
//ninja that jumps between waypoints and throws shurikens
//bats, navigate towards player
//bone bats, navigate towards player and resurrect when out of line of sight (of the player not the bird's eye's view)
//enemies that always face the player and can only be killed by "bombchus"
//turtles that are impermeable to wires and electricity but are easy to kill. Only way to cross electric gap is to frighten them into their shell and jump on their back
//groundhog, only damage if standing on their location (and apply pushback)

//traps and interactable environment props:
//sweeping trap, indestructible, travels in circle indefinitely
//(indestructible?) enemies that gravitate towards grounded enemies on same surface, slow down otherwise
//"stalactites" that can be shot

//bosses:
//core demon, use mortars to kill enemy while dodging projectiles from above
//water demon, changes the currents to force the player into bad situations
//grapple demon, changes grapple points on the map to force the player to think on their feet while dodging
//field demon, changes gravity in one of (4?) directions to push the player into attacks
//wall demon, one attack splits the enemy into two that eventually rejoin, only way to dodge is to go to the ceiling and drop down
//hidden, piranha, fucking huge
//hidden, plasma "metroid",
