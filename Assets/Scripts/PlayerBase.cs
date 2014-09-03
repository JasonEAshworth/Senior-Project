﻿using UnityEngine;
using System.Collections;

public enum playerClass
{
	WARRIOR, 
	SORCERER, 
	ROGUE, 
	ADVENTURER
};

public class PlayerBase : CharacterBase 
{
	public float respawnTimer = 0.0f;
	public float timeToRespawn = 3.0f;

	public int playerNum = -1;

	public bool canJump = true;
	public float jumpForce = 0.25f;
	public float verticalVelocity = 0.0f;
	public bool attacking = false;

	public GameObject item;

	// controls
	public string moveAxisX;
	public string moveAxisZ;
	public KeyCode jumpKey;
	public KeyCode basicAttackKey;
	public KeyCode specialAttackKey;
	public KeyCode classAbilityKey;

	public PlayerManager manager;
	public CharacterController charControl;

	void Start()
	{
		health = maxHealth;
		charControl = GetComponent<CharacterController>();
		manager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
	}

	void FixedUpdate()
	{

		if (item) 
		{
			if (Input.GetKeyDown("space"))
			{
				switch(item.tag)
				{
				case "Potion":
					//Debug.Log (item.tag);
					health += item.GetComponent<Potion>().potionValue;
					item = null;
					break;
				case "AttackPotion":
					item = null;
					break;
				case "DefensePotion":
					item = null;
					break;
				}
				
			}
		}

		if (!dead)
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

	public override void kill()
	{
		dead = true;
		respawnTimer = timeToRespawn;
		// temp code for testing
		renderer.material.color = Color.red;
	}

	public void respawn()
	{
		transform.position = manager.getRespawnPoint();
		transform.rotation = Quaternion.identity;

		verticalVelocity = 0.0f;

		dead = false;
	}

	void addItem(GameObject p)
	{
		item = p;
	}
	
	public virtual void basicAttack(){}
	public virtual void specialAttack(){}
	public virtual void classAbility(){}
}
