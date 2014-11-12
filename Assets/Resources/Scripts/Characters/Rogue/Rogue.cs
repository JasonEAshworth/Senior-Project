using UnityEngine;
using System.Collections;

public class Rogue : PlayerBase
{
	private float attackStarted = Time.time - 10.0f;
	private bool dash = false;
	public float dashDur = 0.4f;
	private float elapsed = 0.0f;
	private bool canAttack = true;
	private float normalAttackDamage = 10.0f;

	public override void basicAttack(string dir)
	{
		Debug.Log ("attacking");
		if(dir == "down")
		{
			//Check enemy facing
			Debug.Log ("rogue attack");
			attackStarted = Time.time;
		}
		float currentTime = Time.time;
		float timeSinceAttack = currentTime - attackStarted;
		//When the attack key is released, check to see how long it was
		//held to determine what attack to do.
		if (dir == "up" && canAttack)
		{
			if(timeSinceAttack < 1.0f)
			{
				Debug.Log ("rogue basic attack");
				//Basic Attack
				//animator.Play("RogueBasicAttack");
				canAttack = false;
				GetComponent<Animator>().SetTrigger("Attack");
			}
			else
			{
				Debug.Log ("rogue special attack");
				//Dash Attack
				dash = true;
				controllable = false;
				//animator.Play("RogueSpecialAttack");
				specialAttack();
				GetComponent<Animator>().SetTrigger("Dash");
			}
		}
	}

	public override void specialAttack()
	{
		StartCoroutine(Dash());
	}
	
	public IEnumerator Dash()
	{
		while(elapsed < dashDur)
		{
			if (!dead)
			{
				if(dash)
				{
					Collider[] hit = Physics.OverlapSphere(transform.position + transform.forward, 0.5f, LayerMask.GetMask("Enemy"));
					if(hit.Length > 0)
					{
//						foreach (Collider c in hit)
//						{
//							c.GetComponent<EnemyBase>().takeDamage(normalAttackDamage);
//						}
						GameObject go = hit[0].gameObject;

						//if the player cannot go behind the enemy, attempt to go to the right then left
						Debug.Log("hurt enemy");
						dash = false;
						controllable = true;
						elapsed = dashDur;
						
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
						GetComponent<Animator>().SetTrigger("Attack");
					}
					else
					{
						Debug.Log ("dashing");
						elapsed += Time.deltaTime;
						Vector3 moveVec = transform.forward * moveSpeed * 4 * Time.deltaTime;
						moveVec = new Vector3(moveVec.x, verticalVelocity, moveVec.z);
						cc.Move(moveVec);
						yield return new WaitForFixedUpdate();
					}
				}
			}
		}
		dash = false;
		controllable = true;
		elapsed = 0.0f;
		GetComponent<Animator>().SetTrigger("Idle");
	}
	
	public override void classAbility(string dir)
	{
		//Make the rogue invisible
		if(dir == "down")
		{
			visibility = 0.0f;
			GetComponent<Animator>().SetTrigger("Sneak");
		}
		//Remove GetKeyUp if you want the ability to continue even if the user releases the key
		else if(dir == "up")
		{
			visibility = 1.0f;
			GetComponent<Animator>().SetTrigger("Idle");
		}
	}

	// Called by an animation event at the end of each attack animation
	public void notifyAttackEnd()
	{
		canAttack = true;
	}
	
	// Called by an animation event at the start of Attack1 and 2 animation
	public void triggerNormalAttack()
	{
		Collider[] hit = Physics.OverlapSphere(transform.position + transform.forward, 0.5f, LayerMask.GetMask("Enemy"));
		foreach (Collider c in hit)
		{
			c.GetComponent<EnemyBase>().takeDamage(normalAttackDamage);
		}
	}
	
	new void FixedUpdate()
	{
		base.FixedUpdate();
		//Increase the rogue's energy if it is not full and he is visible
		if(visibility == 1.0f && mana < 100.0f)
		{
			addMana(1.0f);
			if(mana > 100.0f)
			{
				mana = 100.0f;
			}
		}
		//Deplete the rogue's energy if he is invisible
		else if(mana > 0.0f && visibility == 0.0f)
		{
			useMana(1.0f);
			//If the rogue runs out of energy, he becomes visible
			if(mana <= 0.0f)
			{
				mana = 0.0f;
				visibility = 1.0f;
				GetComponent<Animator>().SetTrigger("Idle");
			}
			Debug.Log ("rogue class ability");
		}
	}

	void OnTriggerEnter(Collider collider)
	{
		Debug.Log("ote");
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
				GetComponent<Animator>().SetTrigger("Attack");
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