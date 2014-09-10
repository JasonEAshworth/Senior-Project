using UnityEngine;
using System.Collections;

public class Rogue : PlayerBase
{
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
			Debug.Log ("rogue basic attack");
		}
	}
	
	public override void specialAttack()
	{
		if(Input.GetKeyDown (basicAttackKey))
		{
			dash = true;
			//Dash Attack
			Debug.Log ("rogue special attack");
		}
	}
	
	public override void classAbility()
	{
		if(Input.GetKeyDown (classAbilityKey))
		{
			visibility = 0.0f;
			Debug.Log ("rogue class ability");
		}
	}

	public void FixedUpdate()
	{
		if (!dead)
		{
			if(dash)
			{
				Debug.Log("dashing");
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
				specialAttack();
				classAbility();
				itemAbility();
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
}
