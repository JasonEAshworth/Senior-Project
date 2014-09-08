using UnityEngine;
using System.Collections;

public class Sorcerer : PlayerBase
{
	private int attackType = 1;
	private float attackStarted = Time.time - 10.0f;

	void start()
	{
		int health = 100;
		int maxHealth = health;
		moveSpeed = 4.0f;
	}
	
	public override void basicAttack()
	{
		if(Input.GetKeyDown (basicAttackKey))
		{
			//Check enemy facing
			Debug.Log ("sorceress attack");
			attackStarted = Time.time;
		}
		float currentTime = Time.time;
		float timeSinceAttack = currentTime - attackStarted;
		if (Input.GetKeyUp (basicAttackKey))
		{
			if(timeSinceAttack < 1.0f)
			{
				if(attackType == 1)
				{
					//Cast Fireball
					//animator.Play("SorceressFireball");
				}
				else
				{
					//Cast Ice Bolt
					//animator.Play("SorceressIceBolt");
				}
				Debug.Log ("sorceress basic attack");
			}
			else
			{
				if(attackType == 1)
				{
					//Cast Firestorm
					//animator.Play("SorceressFirestorm");
				}
				else
				{
					//Cast Blizzard
					//animator.Play("SorceressBlizzard");
				}
				Debug.Log ("sorceress special attack");
			}
		}
	}
	
	/*public override void item()
	{
		if(Input.GetKeyDown(itemKey))
		{
			//Use Item
		}
	}*/
	
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
			Debug.Log ("sorceress class ability");
		}
	}
}
