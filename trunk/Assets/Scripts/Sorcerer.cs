using UnityEngine;
using System.Collections;

public class Sorcerer : PlayerBase
{
	int attackType = 1;
	private float timeButtonHeld;
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
				if(attackType == 1)
				{
					Debug.Log ("FireBall");
				}
				else
				{
					Debug.Log ("ICE");
				}
			}
		}
	}
	
	public override void specialAttack()
	{
		if(attackType == 1)
		{
			Debug.Log("FIRESTORMMMM");
		}
		else
		{
			Debug.Log("BLIZZARD");
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
		}
	}
}
