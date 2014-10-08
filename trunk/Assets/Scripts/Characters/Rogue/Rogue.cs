using UnityEngine;
using System.Collections;

public class Rogue : PlayerBase
{
	private float attackStarted = Time.time - 10.0f;
	private float energy = 100.0f;
	private bool dash = false;
	public float dashDur = 0.4f;
	private float elapsed = 0.0f;
	
	void start()
	{
		int health = 100;
		int maxHealth = health;
		float visibility = 1.0f;
		float moveSpeed = 6.0f;
	}

	public override void basicAttack()
	{
		if(Input.GetKeyDown (basicAttackKey))
		{
			//Check enemy facing
			Debug.Log ("rogue attack");
			attackStarted = Time.time;
		}
		float currentTime = Time.time;
		float timeSinceAttack = currentTime - attackStarted;
		//When the attack key is released, check to see how long it was
		//held to determine what attack to do.
		if (Input.GetKeyUp (basicAttackKey))
		{
			if(timeSinceAttack < 1.0f)
			{
				Debug.Log ("rogue basic attack");
				//Basic Attack
				//animator.Play("RogueBasicAttack");
			}
			else
			{
				Debug.Log ("rogue special attack");
				//Dash Attack
				dash = true;
				controllable = false;
				//animator.Play("RogueSpecialAttack");
			}
		}
	}
	
	/*public override void itemAbility()
	{
		if(Input.GetKeyDown(itemAbilityKey))
		{
			//Use Item
			Debug.Log ("rogue item");
		}
	}*/
	
	public override void classAbility()
	{
		//Increase the rogue's energy if it is not full and he is visible
		if(visibility == 1.0f && energy < 100.0f)
		{
			energy += 0.01f;
			if(energy > 100.0f)
			{
				energy = 100.0f;
			}
		}
		//Make the rogue invisible
		if(Input.GetKeyDown(classAbilityKey))
		{
			visibility = 0.0f;
		}
		//Remove GetKeyUp if you want the ability to continue even if the user releases the key
		if(Input.GetKeyUp(classAbilityKey))
		{
			visibility = 1.0f;
		}
		//Deplete the rogue's energy if he is invisible
		if(energy > 0.0f && visibility == 0.0f)
		{
			energy -= 0.01f;
			Debug.Log ("rogue class ability");
		}
		//If the rogue runs out of energy, he becomes visible
		else
		{
			visibility = 1.0f;
		}
	}
	
	new void FixedUpdate()
	{
		base.FixedUpdate();
		if (!dead)
		{
			if(dash)
			{
				elapsed += Time.deltaTime;
				Vector3 moveVec = transform.forward * moveSpeed * 4 * Time.deltaTime;
				moveVec = new Vector3(moveVec.x, verticalVelocity, moveVec.z);
				charControl.Move(moveVec);
				
				if(elapsed >= dashDur)
				{
					dash = false;
					controllable = true;
					elapsed = 0.0f;
				}
			}
		}
	}

	void OnTriggerEnter(Collider collider)
	{
		Debug.Log("trigger");
		GameObject go = collider.gameObject;
		if(go.tag == "Enemy")
		{
			if(dash)
			{
				//if the player is cannot go behind the enemy, attempt to go to the right then left
				Debug.Log("hurt enemy");
				dash = false;
				controllable = true;
				elapsed = 0.0f;

				//center the enemy's collider
				Vector3 colCenter = go.GetComponent<Transform>().position;
				colCenter.y = this.transform.position.y;

				//behind facing
				Vector3 colBehind = go.transform.forward;
				colBehind.y = this.transform.forward.y;
				//right facing
				Vector3 colRight = go.transform.right;
				colRight.y = this.transform.forward.y;
				//left facing
				Vector3 colLeft = -1 * colRight;
				colLeft.y = this.transform.forward.y;

				//scales the radius of the enemy's bounding volume3
				Vector3 sc = go.transform.localScale;
				float largest = Mathf.Max(sc.x, sc.y, sc.z);
				float colRad = (go.GetComponent<CharacterController>().radius + 0.1f) * largest;
				//scales the radius of the player's bounding volume
				sc = this.transform.localScale;
				largest = Mathf.Max(sc.x, sc.y, sc.z);
				float plRad = this.GetComponent<CharacterController>().radius * largest;
				colRad += plRad;

				//coordinates to move behind the enemy
				Vector3 moveBehind = -1 * colRad * colBehind;
				moveBehind.y = this.transform.position.y;
				//coordinates to move to the left side of the enemy
				Vector3 moveRight = -1 * colRad * colRight;
				moveRight.y = this.transform.position.y;
				//coordinates to move to the right side of the enemy
				Vector3 moveLeft = -1 * colRad * colLeft;
				moveLeft.y = this.transform.position.y;

				//list of collisions for the potential positions of the player
				Collider[][] colList = {Physics.OverlapSphere(colCenter + moveBehind, plRad),
										Physics.OverlapSphere(colCenter + moveRight, plRad),
										Physics.OverlapSphere(colCenter + moveLeft, plRad)};

				bool move = true;
				int i = 0;
				for(i = 0; i < colList.Length; i++)
				{
					move = true;
					for(int j = 0; j < colList[i].Length; j++)
					{
						if(colList[i][j].name.Contains("wall_") || colList[i][j].tag == "Enemy")
						{
							move = false;
							Debug.Log("something is in the way");
							break;
						}
					}
					if(move)
					{
						break;
					}
				}

				if(move)
				{
					if(i == 0)
					{
						Debug.Log("behind");
						this.transform.forward = colBehind;
						this.transform.position = colCenter;
						this.transform.Translate(moveBehind, Space.World);
					}
					else if(i == 1)
					{
						Debug.Log("right");
						this.transform.forward = colRight;
						this.transform.position = colCenter;
						this.transform.Translate(moveRight, Space.World);
					}
					else if(i == 2)
					{
						Debug.Log("left");
						this.transform.forward = colLeft;
						this.transform.position = colCenter;
						this.transform.Translate(moveLeft, Space.World);
					}
				}
			}
			else
			{
				Debug.Log("collided with an enemy");
			}
		}
		else
		{
			Debug.Log("collided with " + go.name);
		}
	}
}