using UnityEngine;
using System.Collections;

public class Woodsman : PlayerBase
{
	private Transform shootPosition;

	private bool canFire = true;
	private bool canSpecial = true;
	private bool specialAttacking = false;
	private bool basicAttacking = false;

	private float basicTimer = 0.5f;
	private float specialTimer = 3.0f;
	private float firstButtonPressTime = 0.0f;
	public float zeroMoveTimer = 0.0f;
	private GameObject hawk;
	private Transform hawkPos;
	private HawkAI2 hawkScripts;
	public float hawkCost;
	public float woodsManaRegen = 2.0f;
	public Animator anim;
	private bool canMove = true;
	private float canMoveTimer = 0.0f;

	public void init()
	{
		anim = GetComponent<Animator> ();
		health = 100;
		maxHealth = health;

		maxMana = 100.0f;
		mana = maxMana;
		hawkCost = 0.15f;

		hawkPos = transform.Find("hawkSpawn");
		hawk = Instantiate(Resources.Load("Prefabs/Character/WoodsMan/Hawk"),hawkPos.position,Quaternion.identity) as GameObject;


		shootPosition = transform.Find("shootPos");
		canMoveTimer = 0.0f;

		hawkScripts = hawk.GetComponent<HawkAI2> ();
	}

	protected override void Update()
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
				specialTimer = 3.0f;
			}
			
		}

		if (mana < maxMana && hawkScripts.mode != 2) 
		{
			manaRegen(woodsManaRegen);
		}

		if(!canMove)
		{
			canMoveTimer += Time.deltaTime;
			if(canMoveTimer >= 0.04f)
			{
				moveMulti = 0.0f;
			}
		}

		Debug.Log (hawkCost);

	}

	public override void basicAttack(string dir)
	{
		//Debug.Log ("warrior basic attack");
		if (dir == "down" && canFire) 
		{
			firstButtonPressTime = Time.time;
			canMove = false;
		}
		if (dir == "up")
		{
			float temp = Time.time - firstButtonPressTime;
			firstButtonPressTime = Time.time;
			canMove = true;
			moveMulti = 1.0f;
			canMoveTimer = 0.0f;
			if(temp > 0.7f && canSpecial)
			{
				specialAttackWoods(temp);
			}
			else if(canFire)
			{
				anim.SetTrigger("Attack");
				GameObject bullet = Instantiate (Resources.Load ("Prefabs/Character/WoodsMan/woodsManBullet"), shootPosition.position, Quaternion.LookRotation(transform.forward)) as GameObject;
				//bullet.transform.up = transform.forward;
				canFire = false;
			}
		}
	}
	
	public void specialAttackWoods(float time)
	{
		anim.SetTrigger("Attack");
		GameObject specialBullet = Instantiate (Resources.Load ("Prefabs/Character/WoodsMan/woodsManSpecial"), shootPosition.position, Quaternion.LookRotation(transform.forward)) as GameObject;
		woodsSpecialBulletScript scr = specialBullet.GetComponent<woodsSpecialBulletScript>();
		scr.heldTime = time;
		canSpecial = false;
	}
	
	public override void classAbility(string dir)
	{
		//Debug.Log ("warrior class ability");
		if (dir == "down") 
		{

			if (hawkScripts.mode != 2 && hawkScripts.mode != 3  && mana > hawkCost) 
			{
				anim.SetTrigger("Hawk");
				hawkScripts.mode = 2;
			}
		}
	}
}
