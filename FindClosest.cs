using UnityEngine;
using System.Collections;

public class FindClosest : MonoBehaviour
{
	Vector3[]  blocks;
	char[]     blockTypes;
	GameObject player;
	Vector3    pos;
	Move       script;
	
	int length;
	
	// Assigns positions and types of blocks into an array
	void Start ()
	{
		player = GameObject.Find("/Player");
		script = player.GetComponent<Move>();
		GameObject[] objs = GameObject.FindGameObjectsWithTag("Geometry");
		length = objs.Length;
		
		blocks     = new Vector3[length];
		blockTypes = new char   [length];
		
		for(int i = 0; i < length; ++i)
		{
			blocks    [i] = objs[i].transform.position;
			blockTypes[i] = objs[i].name[0];
		}
	}
	
	// Finds the closest block each frame
	void FixedUpdate ()
	{
		int bit = 0;
		pos = player.transform.position;
		
		int[] index_of_closest = { 0, 0 };
		float[] min = { Mathf.Infinity, Mathf.Infinity };
		
		for(int i = 0; i < length; ++i)
		{
			float temp;
			if((temp = DistanceSquared(blocks[i],pos)) < min[bit])
			{
				index_of_closest[bit] = i;
				min[bit] = temp;
				bit = (++bit)%2; //equivalent to bit = !bit (alternates between 0 and 1)
			}
		}
		
		if(min[0] > min[1]) //swap values so index_of_closest[0] refers to the closest block
		{
			int temp = index_of_closest[0];
			index_of_closest[0] = index_of_closest[1];
			index_of_closest[1] = temp;
		}
		
		script.GameLogic(  blocks[index_of_closest[0]],
						   blocks[index_of_closest[1]],
														blockTypes[index_of_closest[0]],
														blockTypes[index_of_closest[1]]);
	}
	
	float DistanceSquared(Vector3 v1, Vector3 v2)
	{
		Vector3 temp = v2 - v1;
		return Vector3.Dot(temp,temp);
	}
}
