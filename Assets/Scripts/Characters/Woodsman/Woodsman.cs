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
	private GameObject hawk;
	private Transform hawkPos;

	public void init()
	{
		int health = 100;
		int maxHealth = health;
		moveSpeed = 4.0f;

		hawkPos = transform.Find("hawkSpawn");
		hawk = Instantiate(Resources.Load("Prefabs/Character/WoodsMan/Hawk"),hawkPos.position,Quaternion.identity) as GameObject;


		shootPosition = transform.Find("shootPos");

	}

	void Update()
	{
		base.Update();
		hawk.transform.position = new Vector3 (hawk.transform.position.x, hawkPos.position.y, hawk.transform.position.z);
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
	}

	public override void basicAttack(string dir)
	{
		//Debug.Log ("warrior basic attack");
		if (dir == "down" && canFire) 
		{
			firstButtonPressTime = Time.time;
		}
		if (dir == "up")
		{
			float temp = Time.time - firstButtonPressTime;
			firstButtonPressTime = Time.time;
			if(temp > 0.7f && canSpecial)
			{
				specialAttackWoods(temp);
			}
			else if(canFire)
			{
				GameObject bullet = Instantiate (Resources.Load ("Prefabs/Character/WoodsMan/woodsManBullet"), shootPosition.position, Quaternion.LookRotation(transform.forward)) as GameObject;

				//bullet.transform.up = transform.forward;
				canFire = false;
			}
		}
	}
	
	public void specialAttackWoods(float time)
	{
		GameObject specialBullet = Instantiate (Resources.Load ("Prefabs/Character/WoodsMan/woodsManSpecial"), shootPosition.position, Quaternion.LookRotation(transform.forward)) as GameObject;
		woodsSpecialBulletScript scr = specialBullet.GetComponent<woodsSpecialBulletScript>();
		scr.heldTime = time;
		specialBullet.transform.up = transform.forward;
		canSpecial = false;
	}
	
	public override void classAbility(string dir)
	{
		//Debug.Log ("warrior class ability");
		if (dir == "down") 
		{
			HawkAI2 hawkScripts = hawk.GetComponent<HawkAI2> ();
			if (hawkScripts.mode != 2 && hawkScripts.mode != 3) 
			{
				hawkScripts.mode = 2;
			}
		}
	}
}
