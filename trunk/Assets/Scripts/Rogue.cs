using UnityEngine;
using System.Collections;

public class Rogue : PlayerBase
{
	private bool invisible = false;
	private float timeButtonHeld;
	void init()
	{
		int health = 100;
		int maxHealth = health;
		float visibility = 1.0f;
		float moveSpeed = 6.0f;
	}

	public override void basicAttack()
	{
		if(Input.GetKeyDown (basicAttackKey))
		{
			//Check enemy facing
			timeButtonHeld = Time.time;
		}
		if (Input.GetKeyUp (basicAttackKey)) 
		{
			float temp = Time.time - timeButtonHeld;
			if(temp > 0.6f)
			{
				specialAttack();
			}
			else
			{
				// call animation for normal attack
				Debug.Log("Rogue Basic");
			}
		}
	}
	
	public override void specialAttack()
	{
		//Envenom
		Debug.Log ("Rogue Special Attack");
	}
	
	public override void classAbility()
	{
		if(Input.GetKeyDown (classAbilityKey))
		{
			visibility = 0.0f;
			Debug.Log ("rogue class ability");
		}
	}

	public void onCollisionEnter(Collision collision)
	{
		Debug.Log("collision");
		Vector3 temp = collision.gameObject.GetComponent<MeshRenderer>().bounds.size;
		Debug.Log(temp);
		if(collision.gameObject.tag == "Enemy")
		{
			Debug.Log("collided with an enemy");
		}
	}
}
