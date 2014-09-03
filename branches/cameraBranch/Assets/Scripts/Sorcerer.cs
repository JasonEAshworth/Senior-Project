using UnityEngine;
using System.Collections;

public class Sorcerer : PlayerBase
{
	int attackType = 1;

	void start()
	{
		int health = 100;
		int maxHealth = health;
		moveSpeed = 4.0f;
	}
	
	public override void basicAttack()
	{
		if(Input.GetKeyDown(basicAttackKey))
		{
			if(attackType == 1)
			{
				//Cast Fireball
			}
			else
			{
				//Cast Ice Bolt
			}
			Debug.Log ("warrior basic attack");
		}
	}
	
	public override void specialAttack()
	{
		if(Input.GetKeyDown(specialAttackKey))
		{
			if(attackType == 1)
			{
				//Cast Firestorm
			}
			else
			{
				//Cast Blizzard
			}
			Debug.Log ("warrior special attack");
		}
	}
	
	public override void classAbility()
	{
		if(Input.GetKeyDown(classAbilityKey))
		{
			if(attackType == 1)
			{
				attackType = 0;
			}
			else
			{
				attackType = 1;
			}
			Debug.Log ("warrior class ability");
		}
	}
}
