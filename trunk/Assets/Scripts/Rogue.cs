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
		//held to determin what attack to do.
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
	
	public void FixedUpdate()
	{
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
				else
				{
					// MOVEMENT
					// Get the horizontal movement from the joystick input and scale it with moveSpeed
					Vector3 xMovement = Input.GetAxis(moveAxisX) * new Vector3(Camera.main.transform.right.x, 0.0f, Camera.main.transform.right.z);
					Vector3 zMovement = Input.GetAxis(moveAxisZ) * new Vector3(Camera.main.transform.forward.x, 0.0f, Camera.main.transform.forward.z);
					Vector3 moveVec = Vector3.ClampMagnitude(xMovement + zMovement, 1.0f);
					
					moveVec *= moveSpeed * Time.deltaTime;
					
					// Handle jumping and add it to the movement vector
					if (canJump && Input.GetKeyDown(jumpKey))
					{
						verticalVelocity = jumpForce;
						canJump = false;
					}
					else if (charControl.isGrounded)
					{
						verticalVelocity = 0.0f;
						canJump = true;
					}
					else
					{
						verticalVelocity += Physics.gravity.y * 0.1f * Time.deltaTime;
					}
					
					moveVec = new Vector3(moveVec.x, verticalVelocity, moveVec.z);
					
					charControl.Move(moveVec);
					
					// Rotate the character to face in the direction that they will move
					if (new Vector3(moveVec.x, 0.0f, moveVec.z).magnitude > 0.01f)
					{
						transform.rotation = Quaternion.RotateTowards (transform.rotation, Quaternion.LookRotation (new Vector3 (moveVec.x, 0.0f, moveVec.z)), rotationSpeed * Time.deltaTime);
					}
					
					basicAttack();
					classAbility();
					itemAbility();
				}
			}
		}
		else
		{
			respawnTimer -= Time.deltaTime;
			if (respawnTimer <= 0.0f)
			{
				respawn();
			}
		}
	}

	void OnTriggerEnter(Collider collision)
	{
		Debug.Log("collision1");
		GameObject go = collision.gameObject;
		if(collision.gameObject.tag == "Enemy")
		{
			if(dash)
			{
				//still need to make sure that this doesn't put the player in a wall
				Debug.Log("hurt enemy");
				dash = false;
				elapsed = 0.0f;

				Vector3 c = go.GetComponent<Transform>().position;
				c.y = this.transform.position.y;
				this.transform.position = c;

				Vector3 f = go.transform.forward;
				f.y = this.transform.forward.y;
				this.transform.forward = f;

				Vector3 m = go.GetComponent<MeshRenderer>().bounds.size;
				m += this.GetComponent<MeshRenderer>().bounds.size;

				Vector3 moveTo = -0.5f * Vector3.Scale(m, f);
				moveTo.y = 0.0f;

				this.transform.Translate(moveTo, Space.World);
			}
			else
			{
				Debug.Log("collided with an enemy");
			}
		}
	}
}