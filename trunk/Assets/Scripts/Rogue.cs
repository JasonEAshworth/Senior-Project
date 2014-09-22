using UnityEngine;
using System.Collections;

public class Rogue : PlayerBase
{
	private float attackStarted = Time.time - 10.0f;
	private float energy = 100.0f;
	private bool dash = false;
	private float dashDur = 0.4f;
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
			if (controllable)
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
						elapsed = 0.0f;
					}
				}
			}
		}
	}

	void OnTriggerEnter(Collider collider)
	{
		Debug.Log("collision1");
		GameObject go = collider.gameObject;
		if(go.tag == "Enemy")
		{
			if(dash)
			{
				//if the player is cannot go behind the enemy, attempt to go to the right then left
				Debug.Log("hurt enemy");
				dash = false;
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

				//size of the enemy's mesh
				Vector3 colMesh = go.GetComponent<MeshRenderer>().bounds.size;
				//size of the player's mesh
				Vector3 plMesh = this.GetComponent<MeshRenderer>().bounds.size;
				colMesh += plMesh;

				//coordinates to move behind the enemy
				Vector3 moveBehind = -0.5f * Vector3.Scale(colMesh, colBehind);
				moveBehind.y = this.transform.position.y;
				//coordinates to move to the left side of the enemy
				Vector3 moveRight = -0.5f * Vector3.Scale(colMesh, colRight);
				moveRight.y = this.transform.position.y;
				//coordinates to move to the right side of the enemy
				Vector3 moveLeft = -0.5f * Vector3.Scale(colMesh, colLeft);
				moveLeft.y = this.transform.position.y;


				//plMesh sizes change with rotation
				float temp = (plMesh.x + plMesh.z) / 2;
				Debug.Log(plMesh.x);
				Debug.Log(plMesh.z);
				Debug.Log(temp);
				Collider[][] colList = {Physics.OverlapSphere(colCenter + moveBehind, temp),
										Physics.OverlapSphere(colCenter + moveRight, temp),
										Physics.OverlapSphere(colCenter + moveLeft, temp)};


				bool move = true;
				int i = 0;
				for(i = 0; i < colList.Length; i++)
				{
					move = true;
					for(int j = 0; j < colList[i].Length; j++)
					{
						if(colList[i][j].name == "wall_2" || colList[i][j].name == "wall_4")
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
		else if(go.tag == "Untagged")
		{
			Debug.Log("hit the wall?");
		}
	}
}