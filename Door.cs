using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour //should I open Pandora's Box and make this an Arc Of Sphere? probably not...
{
	void Start ()
	{
	
	}

	void Update ()
	{
	
	}

	//Doors don't need to follow the Singleton pattern! (there can be two if everything is done properly!)
	//When up or down is pressed near a door, the player walks into the foreground / background
	//When this happens, the game uses LoadLevelAdditiveAsynchronous to load the corresponding level and the level is disabled.
	//After the level finishes loading, the level can be linked (probably bad)
	//Since the door should *not* be dependent on linking... an ArcCast could be used to find the terrain at the other end of the tunnel after the level is enabled.
}