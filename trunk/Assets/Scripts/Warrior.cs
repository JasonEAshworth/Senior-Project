using UnityEngine;
using System.Collections;

public class Warrior : PlayerBase
{
	
	private bool blockProjectiles = false;
	private int count = 0;
	private int attackType = 1;
	private float attackStarted = Time.time - 10.0f;
	private float comboStarted = Time.time - 10.0f;
	private float rage = 100.0f;

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
			if(count == 0)
			{
				comboStarted = Time.time;
			}
		}
		float currentTime = Time.time;
		float timeSinceAttack = currentTime - attackStarted;
		float timeSinceCombo = currentTime - comboStarted;
		if (Input.GetKeyUp (basicAttackKey))
		{
			//If there is too long of time between basic attacks, the combo is reset
			if (timeSinceCombo > 3.0f)
			{
				count = 0;
			}
			//When the attack key is released, check to see how long it was
			//held to determin what attack to do.
			if (count < 3 && timeSinceAttack < 1.0f)
			{
				//Check and see if the user has done the basic attack in time
				//to continue with the combo
				if (timeSinceCombo <= 3.0f)
				{
					//animator.Play("WarriorBasicAttack");
					Debug.Log ("Basic Warrior Attack");
					count++;
				}
				else
				{
					//animator.Play("WarriorBasicAttack");
					Debug.Log ("Basic Warrior Attack/Combo reset");
					count = 0;
				}
			}
			//If the combo is complete, do the smash attack
			else if (count == 3 && timeSinceAttack < 1.0f)
			{
				//animator.Play("WarriorCleaveAttack");
				Debug.Log ("Cleave Warrior Attack");
				count = 0;
			}
			//If the warrior still has rage, he uses his special; Otherwise, he just does a basic attack
			else if(rage >= 25.0f)
			{
				Debug.Log ("warrior special attack");
				attacking = true;
				rage -= 25.0f;
				/*
				if(!animation.isPlaying)
				{
					animation.Play("WarriorCleaveAttack");
				}*/
			}
			else
			{
				//animator.Play("WarriorBasicAttack");
				Debug.Log ("Basic Warrior Attack");
			}
		}
			
			
			//Old Warrior Logic
			/*if (Input.GetKeyDown (basicAttackKey) && !attacking) 
		{
			float curTimeAttack = Time.time;
			float timeSinceLastAttack = curTimeAttack - lastTimeAttack;
			lastTimeAttack = curTimeAttack;
			Debug.Log (timeSinceLastAttack);
			if(timeSinceLastAttack < 1.0f)
			{
				count++;
			}
			else
			{
				count = 0;
			}
			if(count < 3  && !attacking)
			{
				//animator.Play("WarriorBasicAttack");
				Debug.Log ("Basic Warrior Attack");
			}
			else if(count == 3)
			{
				//animator.Play("WarriorCleaveAttack");
				Debug.Log ("Cleave Warrior Attack");
				count = 0;
			}
		}*/
		}
		
		/*public override void itemAbility()
		{
			
			if (Input.GetKeyDown (itemAbilityKey) && !attacking) 
			{
				Debug.Log ("warrior item");
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