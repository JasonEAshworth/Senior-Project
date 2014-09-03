using UnityEngine;
using System.Collections;

public class Warrior : PlayerBase
{

	private bool blockProjectiles = false;
	private int count = 0;
	private float lastTimeAttack = 0.0f;
	private float specialAttackTimer;
	public void init()
	{
		maxHealth = 120;
		health = maxHealth;
		moveSpeed = 4.0f;
		blockProjectiles = false;
	}

	public override void basicAttack()
	{
		if (Input.GetKeyDown (basicAttackKey) && !attacking) 
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
		}
	}

	public override void specialAttack()
	{
		//Debug.Log ("warrior special attack");
		if (Input.GetKeyDown (basicAttackKey) && !attacking) 
		{
			attacking = true;
			/*
			if(!animation.isPlaying)
			{
				animation.Play("WarriorCleaveAttack");
			}*/
		}
	}

	public override void classAbility()
	{
		if (Input.GetKeyDown (classAbilityKey) && !attacking) 
		{
			//Debug.Log ("warrior class ability");
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
