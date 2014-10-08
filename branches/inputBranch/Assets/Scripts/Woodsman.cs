using UnityEngine;
using System.Collections;

public class Woodsman : PlayerBase
{
	private Transform shootPosition;

	private bool canFire = true;
	private bool canSpecial = true;

	private float basicTimer = 0.5f;
	private float specialTimer = 10.0f;
	private float firstButtonPressTime = 0.0f;

	public void init()
	{
		int health = 100;
		int maxHealth = health;
		moveSpeed = 4.0f;

		shootPosition = transform.Find("shootPos");

	}

	public override void basicAttack(string dir)
	{
		if (!canFire) 
		{
			basicTimer -= Time.deltaTime;
			if(basicTimer <= 0.0f)
			{
				canFire = true;
				basicTimer = 0.5f;
			}
		}

		if (!canSpecial) 
		{
			specialTimer -= Time.deltaTime;
			if(specialTimer <= 0.0f)
			{
				canSpecial = true;
				specialTimer = 10.0f;
			}
				
		}
		//Debug.Log ("warrior basic attack");
		if (dir == "down" && canFire) 
		{
			firstButtonPressTime = Time.time;

		}
		if (dir == "up" && canSpecial) {
			float temp = Time.time - firstButtonPressTime;
			//Debug.Log (temp);
			if (temp > 0.4f && canSpecial) {
					Debug.Log ("SPECIAL ATTACK");
					specialAttackWoods (temp);
					//canSpecial = false;
			} 
			else if (canFire) {
					//Debug.Log ("WE got in here..bad news");
					GameObject bullet = Instantiate (Resources.Load ("Prefabs/items/woodsManBullet"), shootPosition.position, Quaternion.LookRotation (transform.forward)) as GameObject;
					bullet.transform.up = transform.forward;
					//canFire = false;
			}
		}

	}
	
	public void specialAttackWoods(float time)
	{
		//Debug.Log ("warrior special attack");
		GameObject bullet = Instantiate (Resources.Load ("Prefabs/items/woodsManSpecial"), shootPosition.position, Quaternion.LookRotation(transform.forward)) as GameObject;
		woodsSpecialBulletScript woodsSpecial = bullet.GetComponent<woodsSpecialBulletScript> ();
		woodsSpecial.heldTime = time;
		bullet.transform.up = transform.forward;
	}
	
	public override void classAbility(string dir)
	{
		Debug.Log ("woodsman class ability");
	}
}
