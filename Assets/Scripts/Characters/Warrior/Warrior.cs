using UnityEngine;
using System.Collections;

public class Warrior : PlayerBase
{
	private bool blockProjectiles = false;
	private int count = 0;
	private int attackType = 1;
	private float attackStarted = Time.time - 10.0f;
	private float comboStarted = Time.time - 10.0f;
	private bool attacking = false;

	/*public void init()
	{
		moveSpeed = 4.0f;
		blockProjectiles = false;
	}*/
	
	public override void basicAttack(string dir)
	{
		if (dir == "down")
		{
			//Check enemy facing
			//Debug.Log ("warrior attack");
			attackStarted = Time.time;
			if(count == 0)
			{
				comboStarted = Time.time;
			}
		}
		float currentTime = Time.time;
		float timeSinceAttack = currentTime - attackStarted;
		float timeSinceCombo = currentTime - comboStarted;
		if (dir == "up") {

			//If there is too long of time between basic attacks, the combo is reset
			if (timeSinceCombo > 3.0f){
				count = 0;
			}

			//When the attack key is released, check to see how long it was
			//held to determin what attack to do.
			if (count < 3 && timeSinceAttack < 1.0f && !normal){
				//Check and see if the user has done the basic attack in time
				//to continue with the combo

				if (timeSinceCombo <= 3.0f){
					count++;
					addMana(5.0f);
					Debug.Log("Warrior Basic Attack, Count: " + count);
				} 
				else{
					count = 0;
					addMana(5.0f);
					Debug.Log("Warrior Basic Attack, Count: " + count);
				}
			}

			//If the combo is complete, do the smash attack
			else if (count == 3 && timeSinceAttack < 1.0f){
				Debug.Log("Warrior Combo Attack");
				count = 0;
				if(!normal)
					StartCoroutine(comboAttack());
			}

			//If the warrior still has rage, he uses his special; Otherwise, he just does a basic attack
			else if(mana >= 25.0f){
				Debug.Log("Warrior Special Attack");
				if(!special)
					StartCoroutine(specialAttack());
			}
			else{

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

	private IEnumerator comboAttack(){
		normal = true;
		
		//do attack shit here
		addMana (10.0f);
		yield return StartCoroutine (Wait (0.5f));
		normal = false;
	}

	private IEnumerator specialAttack(){
		special = true;

		useMana(25.0f);
		//do special attack shit

		yield return StartCoroutine (Wait (0.5f));
		special = false;
	}
	
	public override void classAbility(string dir)
	{
		if (dir == "down" && !attacking) 
		{
			Debug.Log ("warrior class ability");
			// animator.Play("WarriorClassAbility");
			moveSpeed = moveSpeed / 2;
			blockProjectiles = true;
		} 
		else if (dir == "up") 
		{
			// animator.Play("NormalWalkingWarrior");
			moveSpeed = moveSpeed *2;
			blockProjectiles = false;
		}
	}
		
	}