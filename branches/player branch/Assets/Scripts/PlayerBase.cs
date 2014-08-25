using UnityEngine;
using System.Collections;

public enum playerClass
{
	WARRIOR, 
	SORCERER, 
	ROGUE, 
	ADVENTURER
};

public class PlayerBase : MonoBehaviour 
{
	public PlayerManager manager;

	public int health = 100;
	public int maxHealth;
	public bool dead = false;
	public float respawnTimer = 0.0f;
	public float timeToRespawn = 3.0f;

	public int playerNum = -1;

	public float moveSpeed = 6.0f;
	public float rotationSpeed = 250.0f;
	public float visibility = 1.0f;

	public bool canJump = true;
	public float jumpForce = 0.25f;
	public float verticalVelocity = 0.0f;

	// controls
	public string moveAxisX;
	public string moveAxisZ;
	public KeyCode jumpKey;
	public KeyCode basicAttackKey;
	public KeyCode specialAttackKey;
	public KeyCode classAbilityKey;

	public GameObject item;

	public CharacterController charControl;

	void Start()
	{
		maxHealth = health;
		charControl = GetComponent<CharacterController>();
		manager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
	}

	void FixedUpdate()
	{
		if (Input.GetKeyDown(KeyCode.H))
		{
			killPlayer();
		}

		if (!dead)
		{
			// MOVEMENT
			// Get the horizontal movement from the joystick input and scale it with moveSpeed
			Vector3 moveVec = new Vector3(Input.GetAxis(moveAxisX), 0.0f, Input.GetAxis(moveAxisZ));
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

			// ATTACKS
			if (Input.GetKeyDown(basicAttackKey))
			{
				basicAttack();
			}
			else if (Input.GetKeyDown(specialAttackKey))
			{
				specialAttack();
			}
			else if (Input.GetKeyDown(classAbilityKey))
			{
				classAbility();
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

	public void takeDamage(int amount)
	{
		health -= amount;
		if (health <= 0)
		{
			killPlayer();
		}
	}

	public void killPlayer()
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
	
	public virtual void basicAttack(){}
	public virtual void specialAttack(){}
	public virtual void classAbility(){}
}
