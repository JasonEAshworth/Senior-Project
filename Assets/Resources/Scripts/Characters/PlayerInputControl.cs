using UnityEngine;

/// Moves one player character from the device the player joined with (a keyboard layout,
/// a gamepad, or both). Replaces the 2015 Rewired controller, which was a paid package.
[AddComponentMenu("")]
[RequireComponent(typeof(CharacterController))]
public class PlayerInputControl : MonoBehaviour {

	public InputBinding binding = new InputBinding();

	public float moveSpeed = 5.5f;
	public float speedFactor = 1.0f;     // shop upgrades and the haste power-up

	public int maxDist = 10; // Distance the players can be seperated from one another

	private CharacterController cc;
	private Animator animator;
	private Vector3 moveVector;

	// Presses are latched in Update and consumed in FixedUpdate. Read directly in FixedUpdate,
	// a press can be missed or seen twice, because the two loops run at different rates.
	private bool fire;
	private bool fireUp;
	private bool jump;
	private bool classAbility;
	private bool classAbilityUp;
	private bool utilityDown;

	private static PlayerManager plyrMgr;

	private PlayerBase character;
	public float rotationSpeed = 360.0f;

	void Awake() {
		cc = GetComponent<CharacterController>();
		animator = GetComponent<Animator>();
		character = GetComponent<PlayerBase>();
		if(!plyrMgr)
		{
			plyrMgr = PlayerManager.Instance;
		}
	}

	void Update () {
		if (Time.timeScale == 0.0f)
			return;
		GetInput();
	}

	void FixedUpdate () {
		ProcessInput();
		fire = fireUp = jump = classAbility = classAbilityUp = utilityDown = false;
	}

	private void GetInput() {
		Vector2 move = PlayerControls.Move(binding);
		fire |= PlayerControls.Pressed(binding, PlayerAction.Attack);
		fireUp |= PlayerControls.Released(binding, PlayerAction.Attack);
		classAbility |= PlayerControls.Pressed(binding, PlayerAction.Ability);
		classAbilityUp |= PlayerControls.Released(binding, PlayerAction.Ability);
		jump |= PlayerControls.Pressed(binding, PlayerAction.Jump);
		utilityDown |= PlayerControls.Pressed(binding, PlayerAction.Item);

		// Movement follows the screen. The dungeon camera looks along -Z (yaw near 180), so raw
		// axes would make up walk down the screen and left walk right.
		Camera cam = Camera.main;
		float yaw = cam != null ? cam.transform.eulerAngles.y : 0.0f;
		Vector3 world = Quaternion.Euler(0.0f, yaw, 0.0f) * new Vector3(move.x, 0.0f, move.y);
		moveVector.x = world.x;
		moveVector.z = world.z;
	}

	private void ProcessInput() {
		moveVector.y = 0.0f;

		//Handle jumping and add it to the movement vector
		if (jump && !character.dead)
		{
			if(character.canJump)
			{
				animator.SetBool("Jump", true);
				character.canJump = false;
				character.addForce(new Vector3(0.0f, character.jumpForce, 0.0f));
			}
		}
		else if (cc.enabled && cc.isGrounded)
		{
			animator.SetBool("Jump", false);
			character.canJump = true;
			character.forces = new Vector3(character.forces.x, Mathf.Max(0.0f, character.forces.y), character.forces.z);
		}

		character.addForce(new Vector3(0.0f, Physics.gravity.y * 2.0f * Time.deltaTime, 0.0f));

		if (!character.dead)
		{
			// Rotate the character to face in the direction that they will move
			if (new Vector3(moveVector.x, 0.0f, moveVector.z).magnitude > 0.0f)
			{
				transform.rotation = Quaternion.RotateTowards (transform.rotation, Quaternion.LookRotation (new Vector3 (moveVector.x, 0.0f, moveVector.z)), rotationSpeed * Time.deltaTime);
			}

			if (fire) {
				character.basicAttack ("down");
			}

			if(fireUp) {
				character.basicAttack("up");
			}

			if (classAbility) {
				character.classAbility("down");
			}

			if (classAbilityUp) {
				character.classAbility("up");
			}

			if (utilityDown)
			{
				character.itemAbility();
			}

			// Process movement
			if(moveVector.x != 0.0f || moveVector.z != 0.0f || moveVector.y != 0.0f)
			{
				//if the player's distance from the group's center exceeds maxDist on the x or z
				//axis, they are stopped from moving on that axis. If they're already too far away,
				//they are only allowed to move closer to the center.
				Vector3 movement = moveVector * moveSpeed * speedFactor * Time.deltaTime * character.moveMulti;
				Vector3 oldDistVec = character.transform.position - plyrMgr.playersCenter;
				Vector3 newDistVec = oldDistVec + movement;

				float newX = Mathf.Abs(newDistVec.x);
				float oldX = Mathf.Abs(oldDistVec.x);

				float newZ = Mathf.Abs(newDistVec.z);
				float oldZ = Mathf.Abs(oldDistVec.z);

				if(newX > maxDist && newX > oldX)
				{
					movement.x = 0.0f;
				}
				if(newZ > maxDist && newZ > oldZ)
				{
					movement.z = 0.0f;
				}

				if (cc.enabled)
					cc.Move(movement);
			}

			animator.SetBool("Run", moveVector.magnitude > 0.0f);
		}
	}
}
