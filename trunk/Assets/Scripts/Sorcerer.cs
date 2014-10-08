using UnityEngine;
using System.Collections;

public class Sorcerer : PlayerBase
{
	private int attackType = 1;
	private float attackStarted = Time.time - 10.0f;
	private float mana = 100.0f;
	private float timeButtonHeld;

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
			//When the attack key is released, check to see how long it was
			//held to determin what attack to do.
			if(timeSinceAttack < 1.0f || mana < 25.0f)
			{
				//Check with attackType to see which basic attack to use
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
				//Check with attackType to see which basic attack to use
				mana -= 25.0f;
				if(attackType == 1)
				{
					//Cast Firestorm
					//animator.Play("SorceressFirestorm");
					Firestorm();
					Debug.Log ("Firestorm");
				}
				else
				{
					//Cast Blizzard
					//animator.Play("SorceressBlizzard");
					Blizzard();
					Debug.Log("Blizzard");

				}
				Debug.Log("sorceress special attack");
			}
		}
	}
	
	/*public override void itemAbility()
	{
		if(Input.GetKeyDown(itemAbilityKey))
		{
			Debug.Log ("sorceress item");
			//Use Item
		}
	}*/

	public override void classAbility()
	{
		//When the key is pushed, switch the attack type
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

	private void Blizzard(){
		Quaternion startAngle = Quaternion.AngleAxis (-30, Vector3.up);
		Quaternion stepAngle = Quaternion.AngleAxis (5, Vector3.up);

		Quaternion angle = transform.rotation * startAngle;
		Vector3 direction = angle * Vector3.forward * 5;
		Vector3 pos = transform.position;

		//Creates an angle of 90 degrees of Raycasting
		for (int i = 0; i < 13; i++) {
			Debug.DrawRay(pos, direction, Color.red, 5.0f);

			direction = stepAngle * direction;
		}

		//This is where we create the animation for the Blizzard
		//attack with ice coming out of the ground
		GameObject Bliz = Instantiate (Resources.Load ("Prefabs/Character/Sorceress/Blizzard"), pos, transform.rotation) as GameObject;
		Destroy (Bliz, 5.0f);
	}

	private void Firestorm(){
		Transform pos = transform.Find("shootPos");

		GameObject Fireball = Instantiate (Resources.Load ("Prefabs/Character/Sorceress/Fireball"), pos.position, transform.rotation) as GameObject;
	}
}
