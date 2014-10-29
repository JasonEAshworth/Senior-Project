using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

	public override void basicAttack(string dir)
	{
		if(dir == "down")
		{
			//Check enemy facing
			Debug.Log ("sorceress attack");
			attackStarted = Time.time;
		}
		float currentTime = Time.time;
		float timeSinceAttack = currentTime - attackStarted;
		if (dir == "up")
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
					Fireball();
					Debug.Log ("Fireball");
				}
				else
				{
					//Cast Ice Bolt
					//animator.Play("SorceressIceBolt");
					IceSpike();
					Debug.Log("Ice Spike");
				}
				Debug.Log ("sorceress basic attack");
			}
			else
			{
				//Check with attackType to see which basic attack to use
				//mana -= 25.0f;
				if(attackType == 1)
				{
					//Cast Firestorm
					//animator.Play("SorceressFirestorm");
					Meteor();
					Debug.Log("Meteor");
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

	public override void classAbility(string dir)
	{
		//When the key is pushed, switch the attack type
		if (dir == "down")
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

		//DEBUG PURPOSES////////////////////////

		for(int i=0; i<enemies.Count; i++)
			Debug.Log (enemies[i]);

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
	}

	private void Fireball(){
		Transform pos = transform.Find("shootPos");

		GameObject Fireball = Instantiate (Resources.Load ("Prefabs/Character/Sorceress/SorceressAbilities/Fireball"), pos.position, transform.rotation) as GameObject;
	}

	private void Meteor(){
		Vector3 pos = transform.position;

		GameObject Meteor = Instantiate (Resources.Load ("Prefabs/Character/Sorceress/SorceressAbilities/Meteor"), pos, transform.rotation) as GameObject;

		//Example for alphaing out the texture of the object
		/*GameObject ball = Meteor.transform.FindChild ("Sphere").gameObject;
		Debug.Log (ball);
		ball.renderer.material.color = new Color (255, 0.0f, 0.0f, 0.1f);*/
	}

	private void IceSpike(){
		Transform pos = transform.Find("shootPos");

		GameObject icicle = Instantiate (Resources.Load ("Prefabs/Character/Sorceress/SorceressAbilities/Icicle_Shot"), pos.position, transform.rotation) as GameObject;
		icicle.transform.up = transform.forward;
	}


}
