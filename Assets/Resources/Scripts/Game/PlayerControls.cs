using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerAction { Jump, Attack, Ability, Item, Pause }

/// The device a player joined with: one of two keyboard layouts, a gamepad, or both
/// (the title screen gives player 1 the keyboard and the first gamepad).
[System.Serializable]
public class InputBinding
{
	public int keyboardLayout = -1;   // -1 none, 0 WASD layout, 1 arrows layout
	public int gamepadId = -1;        // InputDevice.deviceId, -1 none

	public Gamepad Pad
	{
		get { return gamepadId >= 0 ? InputSystem.GetDeviceById(gamepadId) as Gamepad : null; }
	}

	public bool Uses(InputBinding other)
	{
		return (keyboardLayout >= 0 && keyboardLayout == other.keyboardLayout)
			|| (gamepadId >= 0 && gamepadId == other.gamepadId);
	}

	public string Describe()
	{
		string k = keyboardLayout == 0 ? "Keyboard (WASD)" : keyboardLayout == 1 ? "Keyboard (arrows)" : "";
		string g = gamepadId >= 0 ? "Gamepad" : "";
		return k.Length > 0 && g.Length > 0 ? k + " + " + g : k + g;
	}
}

/// Reads keyboard layouts and gamepads for gameplay, menus, and joining.
public static class PlayerControls
{
	struct KeyLayout
	{
		public Key up, down, left, right;
		public Key[] jump, attack, ability, item, pause;
	}

	// Player 2's alternates sit next to the arrow keys for keyboards with no number pad.
	static readonly KeyLayout[] layouts = {
		new KeyLayout { up = Key.W, down = Key.S, left = Key.A, right = Key.D,
			jump = new[] { Key.Space }, attack = new[] { Key.J }, ability = new[] { Key.K }, item = new[] { Key.L },
			pause = new[] { Key.Escape } },
		new KeyLayout { up = Key.UpArrow, down = Key.DownArrow, left = Key.LeftArrow, right = Key.RightArrow,
			jump = new[] { Key.RightShift, Key.Numpad0 }, attack = new[] { Key.Comma, Key.Numpad1 },
			ability = new[] { Key.Period, Key.Numpad2 }, item = new[] { Key.Slash, Key.Numpad3 },
			pause = new[] { Key.Backspace } },
	};

	public static string[] LayoutHelp = {
		"WASD move, Space jump, J attack, K ability, L potion, Esc pause",
		"Arrows move, Right Shift jump, comma attack, period ability, slash potion, Backspace pause",
	};

	static Key[] KeysFor(KeyLayout l, PlayerAction a)
	{
		switch (a)
		{
			case PlayerAction.Jump: return l.jump;
			case PlayerAction.Attack: return l.attack;
			case PlayerAction.Ability: return l.ability;
			case PlayerAction.Item: return l.item;
			default: return l.pause;
		}
	}

	static UnityEngine.InputSystem.Controls.ButtonControl PadButton(Gamepad p, PlayerAction a)
	{
		switch (a)
		{
			case PlayerAction.Jump: return p.buttonSouth;
			case PlayerAction.Attack: return p.buttonWest;
			case PlayerAction.Ability: return p.buttonEast;
			case PlayerAction.Item: return p.buttonNorth;
			default: return p.startButton;
		}
	}

	public static Vector2 Move(InputBinding b)
	{
		Vector2 move = Vector2.zero;
		Keyboard kb = Keyboard.current;
		if (kb != null && b.keyboardLayout >= 0)
		{
			KeyLayout l = layouts[b.keyboardLayout];
			move.x += (kb[l.right].isPressed ? 1 : 0) - (kb[l.left].isPressed ? 1 : 0);
			move.y += (kb[l.up].isPressed ? 1 : 0) - (kb[l.down].isPressed ? 1 : 0);
		}
		Gamepad pad = b.Pad;
		if (pad != null)
			move += pad.leftStick.ReadValue() + pad.dpad.ReadValue();
		return Vector2.ClampMagnitude(move, 1.0f);
	}

	public static bool Pressed(InputBinding b, PlayerAction a)
	{
		Keyboard kb = Keyboard.current;
		if (kb != null && b.keyboardLayout >= 0)
			foreach (Key k in KeysFor(layouts[b.keyboardLayout], a))
				if (kb[k].wasPressedThisFrame)
					return true;
		Gamepad pad = b.Pad;
		return pad != null && PadButton(pad, a).wasPressedThisFrame;
	}

	public static bool Released(InputBinding b, PlayerAction a)
	{
		Keyboard kb = Keyboard.current;
		if (kb != null && b.keyboardLayout >= 0)
			foreach (Key k in KeysFor(layouts[b.keyboardLayout], a))
				if (kb[k].wasReleasedThisFrame)
					return true;
		Gamepad pad = b.Pad;
		return pad != null && PadButton(pad, a).wasReleasedThisFrame;
	}

	public static bool Held(InputBinding b, PlayerAction a)
	{
		Keyboard kb = Keyboard.current;
		if (kb != null && b.keyboardLayout >= 0)
			foreach (Key k in KeysFor(layouts[b.keyboardLayout], a))
				if (kb[k].isPressed)
					return true;
		Gamepad pad = b.Pad;
		return pad != null && PadButton(pad, a).isPressed;
	}

	/// Menu steps: -1 or +1 on the frame a direction is first pushed, else 0.
	public static int NavX(InputBinding b)
	{
		Keyboard kb = Keyboard.current;
		if (kb != null && b.keyboardLayout >= 0)
		{
			KeyLayout l = layouts[b.keyboardLayout];
			if (kb[l.left].wasPressedThisFrame) return -1;
			if (kb[l.right].wasPressedThisFrame) return 1;
		}
		Gamepad pad = b.Pad;
		if (pad != null)
		{
			if (pad.dpad.left.wasPressedThisFrame || pad.leftStick.left.wasPressedThisFrame) return -1;
			if (pad.dpad.right.wasPressedThisFrame || pad.leftStick.right.wasPressedThisFrame) return 1;
		}
		return 0;
	}

	public static int NavY(InputBinding b)
	{
		Keyboard kb = Keyboard.current;
		if (kb != null && b.keyboardLayout >= 0)
		{
			KeyLayout l = layouts[b.keyboardLayout];
			if (kb[l.up].wasPressedThisFrame) return -1;
			if (kb[l.down].wasPressedThisFrame) return 1;
		}
		Gamepad pad = b.Pad;
		if (pad != null)
		{
			if (pad.dpad.up.wasPressedThisFrame || pad.leftStick.up.wasPressedThisFrame) return -1;
			if (pad.dpad.down.wasPressedThisFrame || pad.leftStick.down.wasPressedThisFrame) return 1;
		}
		return 0;
	}

	/// A device that is not bound to any joined player and just pressed its join button
	/// (jump, attack, or start). Returns null when nobody is trying to join.
	public static InputBinding DetectJoin()
	{
		Keyboard kb = Keyboard.current;
		if (kb != null)
		{
			for (int l = 0; l < layouts.Length; l++)
			{
				var b = new InputBinding { keyboardLayout = l };
				if (GameSession.IsBound(b))
					continue;
				foreach (Key k in layouts[l].jump)
					if (kb[k].wasPressedThisFrame) return b;
				foreach (Key k in layouts[l].attack)
					if (kb[k].wasPressedThisFrame) return b;
			}
		}
		foreach (Gamepad pad in Gamepad.all)
		{
			var b = new InputBinding { gamepadId = pad.deviceId };
			if (GameSession.IsBound(b))
				continue;
			if (pad.buttonSouth.wasPressedThisFrame || pad.startButton.wasPressedThisFrame)
				return b;
		}
		return null;
	}

	/// Enter on any keyboard, or start on any joined gamepad.
	public static bool AnyConfirm()
	{
		Keyboard kb = Keyboard.current;
		if (kb != null && (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame))
			return true;
		foreach (Gamepad pad in Gamepad.all)
			if (pad.startButton.wasPressedThisFrame)
				return true;
		return false;
	}
}
