/*
 S1: Log     two closest objects and their indexes
 S2: Log old two closest objects and their indexes
 S3: There are only nine advacent slots
           use a data structure filling these objects into a size-9 array
           which contains block types.
           
           Problem #1: up, left, right, and down are all vague terms since the player is moving and is never locked directly in the center of the screen.
           
 S4: Logic based on object continuity
 S5: Logic based on dot product of normals and move_dir, closer to zero is better
 S6: (Optional) create priority system for different block types
*/
/*
using UnityEngine;
using System.Collections;

public class Move : MonoBehaviour
{
	Transform trans;
	Vector3   delta_position = Vector3.zero;
	float     speed = 6f;
	float     gravity = -5f;
	float     velocity = 0f;
	float     sqrtHalf = Mathf.Sqrt(0.5f);
	
	Vector3 oldClosePos;
	Vector3 oldOtherPos;
	
	char oldType1;
	char oldType2;
	
	char[] nine = new char[9];  // top    left, top    mid, top    right //
								// center left, center mid, center right //center mid is mostly for elevators
								// bottom left, bottom mid, bottom right //
	char[] oldNine = new char[9];
	
	// Use this for initialization
	void Start ()
	{
		trans = this.gameObject.transform;
		EraseNine();
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		delta_position = Vector3.zero;
		ApplyGravityCorrectly();
		delta_position += Vector3.right*Input.GetAxis("Horizontal")*speed*Time.deltaTime;
		trans.Translate(delta_position);
	}
	
	public void GameLogic(Vector3 closePos, Vector3 otherPos, char type1, char type2)
	{
		
		LogNine(closePos, otherPos, type1, type2);
		
		/*
		switch(type)
		{
			case 'g':
				Glass (pos);
				break;
			case 'm':
				Metal (pos);
				break;
			case 'p':
				Plastic (pos);
				break;
			case 'r':
				Rust (pos);
				break;
		}
		*//*
		
		oldClosePos = closePos;
		oldOtherPos = otherPos;
		
		oldType1 = type1;
		oldType2 = type2;
	}
	
	void Glass(Vector3 pos)
	{
		
	}
	
	void Metal(Vector3 pos, Vector2 e)
	{
		float x = trans.position.x - pos.x;
		float y = trans.position.y - pos.y;
		
		bool both = true;
		
		//if(Mathf.Abs(x) > sqrtHalf + e.Height) both = !both; // sqrt(2)/2 + 1/2
		//if(Mathf.Abs(y) > sqrtHalf + e.Width ) both = !both;
		
		if(both)
		{
			
		}
	}
	
	void Plastic(Vector3 pos)
	{
		float x = trans.position.x - pos.x;
		float y = trans.position.y - pos.y;
		
		bool both = true;
		
		//if(Mathf.Abs(x) > sqrtHalf + e.Height) both = !both; // sqrt(2)/2 + 1/2
		//if(Mathf.Abs(y) > sqrtHalf + e.Height) both = !both;
		
		//Vector Reject and Allow movement/jump iff floor
		
		if(both)
		{
			
		}
	}
	
	void Rust(Vector3 pos)
	{
		
	}
	
	void ApplyGravityCorrectly() //   http://www.niksula.hut.fi/~hkankaan/Homepages/gravity.html
	{
		velocity       += gravity *Time.deltaTime*0.5f;
		delta_position += velocity*Time.deltaTime*Vector3.up;
		velocity       += gravity *Time.deltaTime*0.5f;	
		
		if     (velocity < -10f) velocity = -10f; //terminal velocity
		else if(velocity >  10f) velocity =  10f;
	}
	
	void VectorReject(ref Vector3 v1, ref Vector3 v2) //velocity vector, wall vector
	{
		v1 -= v2*Vector3.Dot (v1,v2); //wall is normalized so this is complete
	}
	
	void LogNine(Vector3 closePos, Vector3 otherPos, char type1, char type2)
	{
		CopyNine ();
		EraseNine();
		
		int x = (int) (1f + (closePos.x - trans.position.x) % 1);
		int y = (int) (1f + (closePos.y - trans.position.y) % 1);
		
		if(0 <= x && x <= 2 &&
		   0 <= y && y <= 2   ) nine[3*y + x] = type1;
		
		x = (int) (1f + (otherPos.x - trans.position.x) % 1);
		y = (int) (1f + (otherPos.y - trans.position.y) % 1);
		
		if(0 <= x && x <= 2 &&
		   0 <= y && y <= 2   ) nine[3*y + x] = type2;
	}
	
	void EraseNine()
	{
		for(int i = 0; i < 9; ++i)
		{
			nine[i] = 'n';
		}
	}
	
	void CopyNine()
	{
		for(int i = 0; i < 9; ++i)
		{
			oldNine[i] = nine[i];	
		}
	}
}
*/