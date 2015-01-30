using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sorcerer : PlayerBase
{
	private int attackType = 1;
	private float attackStarted = Time.time - 10.0f;
	private float timeButtonHeld;
	private float blizzardDamage = 20.0f;

	/*void Start(){
		base.Start ();
		GetComponentInChildren<Light>().color = new Color(1.0f, 0.6f, 0.6f);
	}*/

	protected override void Update(){
		base.Update();
		manaRegen (2.5f);
	}

	public override void basicAttack(string dir)
	{
		if(dir == "down")
		{
			//Check enemy facing
			Debug.Log ("sorceress attack");
			attackStarted = Time.time;
		}
		float timeSinceAttack = Time.time - attackStarted;
		if (dir == "up")
		{
			//When the attack key is released, check to see how long it was
			//held to determin what attack to do.
			if(timeSinceAttack < 1.0f / attackSpeed || mana < 25.0f)
			{
				//Check with attackType to see which basic attack to use
				if(attackType == 1)
				{
					if(!normal)
						StartCoroutine(Fireball());
				}
				else
				{
					if(!normal)
						StartCoroutine(IceSpike());
				}
			}
			else
			{
				//Check with attackType to see which basic attack to use
				//mana -= 25.0f;
				if(attackType == 1)
				{
					if(!special)
						StartCoroutine(Meteor());
				}
				else
				{
					if(!special)
						StartCoroutine(Blizzard());
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

	public override void classAbility(string dir)
	{
		//When the key is pushed, switch the attack type
		if (dir == "down")
		{
			if(attackType == 1)
			{
				attackType = 0;
				GetComponent<Animator> ().SetTrigger ("IceIdle");
				GetComponentInChildren<Light>().color = new Color(0.6f, 0.6f, 1.0f);
			}
			else
			{
				attackType = 1;
				GetComponent<Animator> ().SetTrigger ("FireIdle");
				GetComponentInChildren<Light>().color = new Color(1.0f, 0.6f, 0.6f);
			}
			Debug.Log ("sorceress class ability");
		}
	}

	private IEnumerator Blizzard(){
		special = true;

		useMana (25.0f);
		GetComponent<Animator> ().SetTrigger ("IceHeavy");
		Quaternion startAngle = Quaternion.AngleAxis (-30, Vector3.up);
		Quaternion stepAngle = Quaternion.AngleAxis (5, Vector3.up);

		Quaternion angle = transform.rotation * startAngle;
		Vector3 direction = angle * Vector3.forward;
		Vector3 pos = transform.position;

		List<GameObject> enemies = new List<GameObject> ();

		//Creates an angle of 90 degrees of Raycasting
		for (int i = 0; i < 13; i++) {
			RaycastHit hit;
			if(Physics.Raycast(pos + new Vector3(0,0.5f,0), direction, out hit, 7, LayerMask.GetMask("Enemy")))
				if(!enemies.Contains(hit.transform.gameObject))
					enemies.Add (hit.transform.gameObject);

			direction = stepAngle * direction;
		}

		for(int i=0; i<enemies.Count; i++)
		{
			enemies[i].SendMessage("takeDamage", blizzardDamage);
			enemies[i].SendMessage("freeze");
		}

		direction = angle * Vector3.forward * 7;

		for (int i = 0; i < 13; i++) {
			Debug.DrawRay(pos + new Vector3(0,0.5f,0), direction, Color.red, 5.0f);
			
			direction = stepAngle * direction;
		}
		//////////////////////////////////////////

		//This is where we create the animation for the Blizzard
		//attack with ice coming out of the ground
		GameObject Bliz = Instantiate (Resources.Load ("Prefabs/Character/Sorceress/SorceressAbilities/Blizzard"), pos, transform.rotation) as GameObject;
		/*foreach (Transform child in Bliz.transform) {
			foreach (Transform c in child)
				if(c.renderer)
					c.renderer.material.color = new Color (255, 0.0f, 0.0f, 0.0f); 
		}*/
		Destroy (Bliz, 5.0f);

		yield return StartCoroutine (Wait (5.0f / attackSpeed));
		special = false;
	}

	private IEnumerator Fireball(){
		normal = true;

		useMana(5.0f);
		GetComponent<Animator> ().SetTrigger ("FireLight");
		Transform pos = transform.Find("shootPos");
		GameObject Fireball = Instantiate (Resources.Load ("Prefabs/Character/Sorceress/SorceressAbilities/Fireball"), pos.position, transform.rotation) as GameObject;

		yield return StartCoroutine (Wait (1.5f / attackSpeed));
		normal = false;
	}

	private IEnumerator Meteor(){
		special = true;

		useMana (25.0f);
		GetComponent<Animator> ().SetTrigger ("FireHeavy");
		Vector3 pos = transform.position;
		GameObject Meteor = Instantiate (Resources.Load ("Prefabs/Character/Sorceress/SorceressAbilities/Meteor"), pos, transform.rotation) as GameObject;

		//Example for alphaing out the texture of the object
		/*GameObject ball = Meteor.transform.FindChild ("Sphere").gameObject;
		Debug.Log (ball);
		ball.renderer.material.color = new Color (255, 0.0f, 0.0f, 0.1f);*/

		yield return StartCoroutine (Wait (5.0f / attackSpeed));
		special = false;
	}

	private IEnumerator IceSpike(){
		normal = true;

		useMana(2.0f);
		GetComponent<Animator> ().SetTrigger ("IceLight");
		Transform pos = transform.Find("shootPos");
		GameObject icicle = Instantiate (Resources.Load ("Prefabs/Character/Sorceress/SorceressAbilities/Icicle_Shot"), pos.position, transform.rotation) as GameObject;
		icicle.transform.up = transform.forward;

		yield return StartCoroutine (Wait (0.5f / attackSpeed));
		normal = false;
	}
	
}
