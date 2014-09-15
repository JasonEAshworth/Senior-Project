using UnityEngine;
using System.Collections;

public class Rogue : PlayerBase
{
	private float attackStarted = Time.time - 10.0f;
	private float energy = 100.0f;
	void start()
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
			Debug.Log ("rogue attack");
			attackStarted = Time.time;
		}
		float currentTime = Time.time;
		float timeSinceAttack = currentTime - attackStarted;
		//When the attack key is released, check to see how long it was
		//held to determin what attack to do.
		if (Input.GetKeyUp (basicAttackKey))
		{
			if(timeSinceAttack < 1.0f)
			{
				Debug.Log ("rogue basic attack");
				//Basic Attack
				//animator.Play("RogueBasicAttack");
			}
			else
			{
				Debug.Log ("rogue special attack");
				//Envenom
				//animator.Play("RogueSpecialAttack");
			}
		}
	}
	
	/*public override void itemAbility()
	{
		if(Input.GetKeyDown(itemAbilityKey))
		{
			//Use Item
			Debug.Log ("rogue item");
		}
	}*/
	
	public override void classAbility()
	{
		//Increase the rogue's energy if it is not full and he is visible
		if(visibility == 1.0f && energy < 100.0f)
		{
			energy += 0.01f;
			if(energy > 100.0f)
			{
				energy = 100.0f;
			}
		}
		//Make the rogue invisible
		if(Input.GetKeyDown(classAbilityKey))
		{
			visibility = 0.0f;
		}
		//Remove GetKeyUp if you want the ability to continue even if the user releases the key
		if(Input.GetKeyUp(classAbilityKey))
		{
			visibility = 1.0f;
		}
		//Deplete the rogue's energy if he is invisible
		if(energy > 0.0f && visibility == 0.0f)
		{
			energy -= 0.01f;
			Debug.Log ("rogue class ability");
		}
		//If the rogue runs out of energy, he becomes visible
		else
		{
			visibility = 1.0f;
		}
	}
}