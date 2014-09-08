using UnityEngine;
using System.Collections;

public class Warrior : PlayerBase
{

	private bool blockProjectiles = false;
	private int count = 0;
	private int attackType = 1;
	private float attackStarted = Time.time - 10.0f;
	public void init()
	{
		maxHealth = 120;
		health = maxHealth;
		moveSpeed = 4.0f;
		blockProjectiles = false;
	}

	public override void basicAttack()
	{
		if(Input.GetKeyDown (basicAttackKey))
		{
			//Check enemy facing
			Debug.Log ("warrior attack");
			attackStarted = Time.time;
		}
		float currentTime = Time.time;
		float timeSinceAttack = currentTime - attackStarted;
		if (Input.GetKeyUp (basicAttackKey))
		{
			if(count < 3  && timeSinceAttack < 1.0f)
			{
				//animator.Play("WarriorBasicAttack");
				Debug.Log ("Basic Warrior Attack");
				if(timeSinceLastAttack < 1.0f)
				{
					count++;
				}
				else
				{
					count = 0;
				}
			}
			else if(count == 3 && timeSinceAttack < 1.0f)
			{
				//animator.Play("WarriorCleaveAttack");
				Debug.Log ("Cleave Warrior Attack");
				count = 0;
			}
			else
			{
				Debug.Log ("warrior special attack");
				attacking = true;
				/*
				if(!animation.isPlaying)
				{
					animation.Play("WarriorCleaveAttack");
				}*/
			}


		//Old Warrior Logic
		/*if (Input.GetKeyDown (basicAttackKey) && !attacking) 
		{
			float curTimeAttack = Time.time;
			float timeSinceLastAttack = curTimeAttack - lastTimeAttack;
			lastTimeAttack = curTimeAttack;
			Debug.Log (timeSinceLastAttack);
			if(count < 3  && !attacking)
			{
				//animator.Play("WarriorBasicAttack");
				Debug.Log ("Basic Warrior Attack");
				if(timeSinceLastAttack < 1.0f)
				{
					count++;
				}
				else
				{
					count = 0;
				}
			}
			else if(count == 3 && !attacking)
			{
				//animator.Play("WarriorCleaveAttack");
				Debug.Log ("Cleave Warrior Attack");
				count = 0;
			}

		}*/
	}

	/*public override void item()
	{
		Debug.Log ("warrior item");
		if (Input.GetKeyDown (specialAttackKey) && !attacking) 
		{
			//Use Item
		}
	}*/

	public override void classAbility()
	{
		if (Input.GetKeyDown (classAbilityKey) && !attacking) 
		{
			Debug.Log ("warrior class ability");
			// animator.Play("WarriorClassAbility");
			moveSpeed = moveSpeed / 2;
			blockProjectiles = true;
		} 
		else if (Input.GetKeyUp (classAbilityKey)) 
		{
			// animator.Play("NormalWalkingWarrior");
			moveSpeed = moveSpeed *2;
			blockProjectiles = false;
		}
	}
	
}
