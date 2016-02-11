using UnityEngine;
using System.Collections;

public class LevelData : MonoBehaviour //credit to http://clearcutgames.net/home/?p=437
{
	public float playerRadius;

	public static LevelData Instance { get; private set; }
	
	void Awake()
	{
		if(Instance != null && Instance != this) // First we check if there are any other instances conflicting
		{
			Destroy(gameObject); // If that is the case, we destroy other instances
		}

		Instance = this; // Here we save our singleton instance
		 
		switch(Application.loadedLevel) //TODO: Make this into an XML loader
		{
			default:
				playerRadius = .01f;
				break;
		}
	}
}