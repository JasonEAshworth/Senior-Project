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
		Debug.Log (hawkPos.position);
		hawk = Instantiate(Resources.Load("Prefabs/Character/Hawk"),hawkPos.position,Quaternion.identity) as GameObject;


		shootPosition = transform.Find("shootPos");

	}

	public override void basicAttack()
	{

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
		//Debug.Log ("warrior basic attack");
		if (Input.GetKeyDown (basicAttackKey) && canFire) 
		{
			firstButtonPressTime = Time.time;
		}
		if (Input.GetKeyUp (basicAttackKey))
		{
			float temp = Time.time - firstButtonPressTime;
			if(temp > 0.4f && canSpecial)
			{
				specialAttackWoods(temp);
			}
			else if(canFire)
			{
				GameObject bullet = Instantiate (Resources.Load ("Prefabs/items/woodsManBullet"), shootPosition.position, Quaternion.LookRotation(transform.forward)) as GameObject;
				bullet.transform.up = transform.forward;
				canFire = false;
			}
		}

	}
	
	public void specialAttackWoods(float time)
	{
		//Debug.Log ("warrior special attack");

	}
	
	public override void classAbility()
	{
		//Debug.Log ("warrior class ability");
		if (Input.GetKeyDown (classAbilityKey)) 
		{
			HawkAI hawkScripts = hawk.GetComponent<HawkAI> ();
			if (hawkScripts.mode != 2) 
			{
				hawkScripts.mode = 2;
			}
		}
	}
}
