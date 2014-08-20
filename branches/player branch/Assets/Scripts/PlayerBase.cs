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
	private playerClass pClass;

	private int health = 100;
	private int maxHealth;

	private float moveSpeed = 6.0f;
	private float visibility = 1.0f;

	private bool canJump = true;
	private float jumpForce = 0.25f;
	private float verticalVelocity = 0.0f;

	// controls
	public string moveAxisX;
	public string moveAxisZ;
	public KeyCode jumpKey;
	public KeyCode basicAttackKey;
	public KeyCode specialAttackKey;
	public KeyCode classAbilityKey;

	private GameObject item;

	private CharacterController charControl;

	void Start()
	{
		maxHealth = health;
		charControl = GetComponent<CharacterController>();
	}

	void FixedUpdate()
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
			transform.rotation = Quaternion.RotateTowards (transform.rotation, Quaternion.LookRotation (new Vector3 (moveVec.x, 0.0f, moveVec.z)), 180.0f * Time.deltaTime);
		}

		// ATTACKS (coming soon)
	}
}
