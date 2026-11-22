using UnityEngine;
using UnityEngine.InputSystem;

/// Moves one player character from a keyboard layout (players 1 and 2) and the gamepad with the
/// same index. Replaces the 2015 Rewired controller, which was a paid package.
[AddComponentMenu("")]
[RequireComponent(typeof(CharacterController))]
public class PlayerInputControl : MonoBehaviour {

	struct KeyLayout {
		public Key up, down, left, right, jump, attack, ability, item;
		public Key altJump, altAttack, altAbility, altItem;
	}

	// Player 2's alternates sit next to the arrow keys for keyboards with no number pad.
	static readonly KeyLayout[] keyLayouts = {
		new KeyLayout { up = Key.W, down = Key.S, left = Key.A, right = Key.D,
			jump = Key.Space, attack = Key.J, ability = Key.K, item = Key.L,
			altJump = Key.None, altAttack = Key.None, altAbility = Key.None, altItem = Key.None },
		new KeyLayout { up = Key.UpArrow, down = Key.DownArrow, left = Key.LeftArrow, right = Key.RightArrow,
			jump = Key.Numpad0, attack = Key.Numpad1, ability = Key.Numpad2, item = Key.Numpad3,
			altJump = Key.RightShift, altAttack = Key.Comma, altAbility = Key.Period, altItem = Key.Slash },
	};

	public int playerId = 0;

	public float moveSpeed = 5.5f;

	public int maxDist = 10; // Distance the players can be seperated from one another

	private CharacterController cc;
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
		character = GetComponent<PlayerBase>();
		playerId = character.playerNum;
		if(!plyrMgr)
		{
			plyrMgr = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
		}
	}

	void Update () {
		GetInput();
	}

	void FixedUpdate () {
		ProcessInput();
		fire = fireUp = jump = classAbility = classAbilityUp = utilityDown = false;
	}

	static float KeyAxis(Keyboard kb, Key negative, Key positive) {
		return (kb[positive].isPressed ? 1.0f : 0.0f) - (kb[negative].isPressed ? 1.0f : 0.0f);
	}

	static bool Pressed(Keyboard kb, Key key, Key alt) {
		return kb[key].wasPressedThisFrame || (alt != Key.None && kb[alt].wasPressedThisFrame);
	}

	static bool Released(Keyboard kb, Key key, Key alt) {
		return kb[key].wasReleasedThisFrame || (alt != Key.None && kb[alt].wasReleasedThisFrame);
	}

	private void GetInput() {
		Vector2 move = Vector2.zero;

		Keyboard kb = Keyboard.current;
		if (kb != null && playerId < keyLayouts.Length) {
			KeyLayout k = keyLayouts[playerId];
			move += new Vector2(KeyAxis(kb, k.left, k.right), KeyAxis(kb, k.down, k.up));
			fire |= Pressed(kb, k.attack, k.altAttack);
			fireUp |= Released(kb, k.attack, k.altAttack);
			classAbility |= Pressed(kb, k.ability, k.altAbility);
			classAbilityUp |= Released(kb, k.ability, k.altAbility);
			jump |= Pressed(kb, k.jump, k.altJump);
			utilityDown |= Pressed(kb, k.item, k.altItem);
		}

		var pads = Gamepad.all;
		if (playerId < pads.Count) {
			Gamepad pad = pads[playerId];
			move += pad.leftStick.ReadValue() + pad.dpad.ReadValue();
			fire |= pad.buttonWest.wasPressedThisFrame;
			fireUp |= pad.buttonWest.wasReleasedThisFrame;
			classAbility |= pad.buttonEast.wasPressedThisFrame;
			classAbilityUp |= pad.buttonEast.wasReleasedThisFrame;
			jump |= pad.buttonSouth.wasPressedThisFrame;
			utilityDown |= pad.buttonNorth.wasPressedThisFrame;
		}

		move = Vector2.ClampMagnitude(move, 1.0f);
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
				character.GetComponent<Animator>().SetBool("Jump", true);
				character.canJump = false;
				character.addForce(new Vector3(0.0f, character.jumpForce, 0.0f));
			}
		}
		else if (cc.isGrounded)
		{
			character.GetComponent<Animator>().SetBool("Jump", false);
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
				Vector3 movement = moveVector * moveSpeed * Time.deltaTime * character.moveMulti;
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

				cc.Move(movement);
			}

			character.GetComponent<Animator>().SetBool("Run", moveVector.magnitude > 0.0f);
		}
	}
}
