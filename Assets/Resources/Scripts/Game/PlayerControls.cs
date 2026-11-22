using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public enum PlayerAction { Jump, Attack, Ability, Item, Pause }

/// Everything a player can rebind on one keyboard layout. The last five match PlayerAction.
public enum BindSlot { Up, Down, Left, Right, Jump, Attack, Ability, Item, Pause }

/// The device a player joined with: one of two keyboard layouts, a gamepad, or both
/// (the title screen gives player 1 the keyboard and the first gamepad).
[Serializable]
public class InputBinding
{
	public int keyboardLayout = -1;   // -1 none, 0 left layout, 1 right layout
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
		string k = keyboardLayout == 0 ? "Keyboard (left)" : keyboardLayout == 1 ? "Keyboard (right)" : "";
		string g = gamepadId >= 0 ? "Gamepad" : "";
		return k.Length > 0 && g.Length > 0 ? k + " + " + g : k + g;
	}
}

/// Reads keyboard layouts and gamepads for gameplay, menus, and joining. Both keyboard layouts
/// and the gamepad buttons can be rebound in the controls menu; the choices are saved.
public static class PlayerControls
{
	public const int Layouts = 2;
	public const int Slots = 9;
	const int FirstAction = (int)BindSlot.Jump;
	const string SaveKey = "controls.v1";

	// Player 2's numpad keys are extra defaults for keyboards that have one; rebinding an action
	// replaces all of its keys with the one chosen.
	static readonly Key[][][] DefaultKeys = {
		new[] {
			new[] { Key.W }, new[] { Key.S }, new[] { Key.A }, new[] { Key.D },
			new[] { Key.Space }, new[] { Key.J }, new[] { Key.K }, new[] { Key.L }, new[] { Key.Escape },
		},
		new[] {
			new[] { Key.UpArrow }, new[] { Key.DownArrow }, new[] { Key.LeftArrow }, new[] { Key.RightArrow },
			new[] { Key.RightShift, Key.Numpad0 }, new[] { Key.Comma, Key.Numpad1 }, new[] { Key.Period, Key.Numpad2 },
			new[] { Key.Slash, Key.Numpad3 }, new[] { Key.Backspace },
		},
	};

	static readonly GamepadButton[] DefaultPad = {
		GamepadButton.South, GamepadButton.West, GamepadButton.East, GamepadButton.North, GamepadButton.Start };

	/// Gamepad buttons an action can use. The stick and d-pad always move.
	public static readonly GamepadButton[] BindableButtons = {
		GamepadButton.South, GamepadButton.East, GamepadButton.West, GamepadButton.North,
		GamepadButton.LeftShoulder, GamepadButton.RightShoulder, GamepadButton.LeftTrigger, GamepadButton.RightTrigger,
		GamepadButton.LeftStick, GamepadButton.RightStick, GamepadButton.Start, GamepadButton.Select };

	static Key[][][] keys;
	static GamepadButton[] pad;

	[Serializable]
	class SavedBindings
	{
		public string[] left, right, pad;
	}

	static PlayerControls()
	{
		Load();
	}

	// ---- bindings ----

	static Key[] KeysFor(int layout, BindSlot s)
	{
		return keys[layout][(int)s];
	}

	static GamepadButton PadFor(PlayerAction a)
	{
		return pad[(int)a];
	}

	public static void BindKey(int layout, BindSlot slot, Key key)
	{
		Key[] old = keys[layout][(int)slot];
		if (old.Length == 1 && old[0] == key)
			return;
		// Both layouts share one keyboard, so a key can only do one thing: the slot that had it
		// takes this slot's old keys.
		for (int l = 0; l < Layouts; l++)
			for (int s = 0; s < Slots; s++)
				if (keys[l][s].Contains(key))
					keys[l][s] = keys[l][s].Length > 1 ? keys[l][s].Where(k => k != key).ToArray() : old.ToArray();
		keys[layout][(int)slot] = new[] { key };
		Save();
	}

	public static void BindPad(PlayerAction action, GamepadButton button)
	{
		int i = Array.IndexOf(pad, button);
		if (i >= 0)
			pad[i] = pad[(int)action];
		pad[(int)action] = button;
		Save();
	}

	public static void ResetToDefaults()
	{
		keys = DefaultKeys.Select(l => l.Select(s => s.ToArray()).ToArray()).ToArray();
		pad = DefaultPad.ToArray();
		PlayerPrefs.DeleteKey(SaveKey);
	}

	static void Load()
	{
		ResetToDefaults();
		string json = PlayerPrefs.GetString(SaveKey, "");
		if (string.IsNullOrEmpty(json))
			return;
		try
		{
			SavedBindings saved = JsonUtility.FromJson<SavedBindings>(json);
			string[][] layouts = { saved.left, saved.right };
			for (int l = 0; l < Layouts; l++)
				for (int s = 0; s < Slots && layouts[l] != null && s < layouts[l].Length; s++)
				{
					Key[] parsed = layouts[l][s].Split('|').Select(n => Enum.TryParse(n, out Key k) ? k : Key.None).Where(k => k != Key.None).ToArray();
					if (parsed.Length > 0)
						keys[l][s] = parsed;
				}
			for (int a = 0; saved.pad != null && a < saved.pad.Length && a < pad.Length; a++)
				if (Enum.TryParse(saved.pad[a], out GamepadButton b))
					pad[a] = b;
		}
		catch (Exception e)
		{
			Debug.LogWarning("Saved controls could not be read; using the defaults. " + e.Message);
			ResetToDefaults();
		}
	}

	static void Save()
	{
		var saved = new SavedBindings
		{
			left = keys[0].Select(s => string.Join("|", s)).ToArray(),
			right = keys[1].Select(s => string.Join("|", s)).ToArray(),
			pad = pad.Select(b => b.ToString()).ToArray(),
		};
		PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(saved));
		PlayerPrefs.Save();
	}

	// ---- names for menus ----

	/// A short name a player can read: "Comma" rather than ",", "Esc" rather than "Escape".
	/// Punctuation is named from the key's label on this keyboard layout, not its position.
	public static string KeyName(Key k)
	{
		switch (k)
		{
			case Key.Escape: return "Esc";
			case Key.LeftShift: return "L Shift";
			case Key.RightShift: return "R Shift";
			case Key.LeftCtrl: return "L Ctrl";
			case Key.RightCtrl: return "R Ctrl";
			case Key.LeftAlt: return "L Alt";
			case Key.RightAlt: return "R Alt";
		}
		Keyboard kb = Keyboard.current;
		string name = kb != null && kb[k] != null ? kb[k].displayName : k.ToString();
		if (string.IsNullOrEmpty(name))
			return k.ToString();
		switch (name)
		{
			case ",": return "Comma";
			case ".": return "Period";
			case "/": return "Slash";
			case "\\": return "Backslash";
			case ";": return "Semicolon";
			case "'": return "Quote";
			case "`": return "Backquote";
			case "-": return "Minus";
			case "=": return "Equals";
			case "[": return "Left Bracket";
			case "]": return "Right Bracket";
			default: return name;
		}
	}

	public static string KeysName(int layout, BindSlot s)
	{
		return string.Join(" / ", KeysFor(layout, s).Select(KeyName));
	}

	public static string ButtonName(GamepadButton b)
	{
		switch (b)
		{
			case GamepadButton.South: return "A";
			case GamepadButton.East: return "B";
			case GamepadButton.West: return "X";
			case GamepadButton.North: return "Y";
			case GamepadButton.LeftShoulder: return "LB";
			case GamepadButton.RightShoulder: return "RB";
			case GamepadButton.LeftTrigger: return "LT";
			case GamepadButton.RightTrigger: return "RT";
			case GamepadButton.LeftStick: return "L3";
			case GamepadButton.RightStick: return "R3";
			case GamepadButton.Select: return "Back";
			default: return b.ToString();
		}
	}

	public static string PadName(PlayerAction a)
	{
		return ButtonName(PadFor(a));
	}

	public static string Help(int layout)
	{
		string first = KeysFor(layout, BindSlot.Up)[0] == Key.W && KeysFor(layout, BindSlot.Left)[0] == Key.A ? "WASD"
			: KeysFor(layout, BindSlot.Up)[0] == Key.UpArrow && KeysFor(layout, BindSlot.Left)[0] == Key.LeftArrow ? "Arrows"
			: KeyName(KeysFor(layout, BindSlot.Up)[0]) + KeyName(KeysFor(layout, BindSlot.Left)[0]) +
			  KeyName(KeysFor(layout, BindSlot.Down)[0]) + KeyName(KeysFor(layout, BindSlot.Right)[0]);
		return first + " move, " + KeyName(KeysFor(layout, BindSlot.Jump)[0]) + " jump, " +
			KeyName(KeysFor(layout, BindSlot.Attack)[0]) + " attack, " + KeyName(KeysFor(layout, BindSlot.Ability)[0]) + " ability, " +
			KeyName(KeysFor(layout, BindSlot.Item)[0]) + " potion, " + KeyName(KeysFor(layout, BindSlot.Pause)[0]) + " pause";
	}

	public static string PadHelp()
	{
		return "stick move, " + PadName(PlayerAction.Jump) + " jump, " + PadName(PlayerAction.Attack) + " attack, " +
			PadName(PlayerAction.Ability) + " ability, " + PadName(PlayerAction.Item) + " potion, " + PadName(PlayerAction.Pause) + " pause";
	}

	// ---- reading input ----

	static bool AnyPressed(Keyboard kb, Key[] ks)
	{
		foreach (Key k in ks)
			if (kb[k].isPressed)
				return true;
		return false;
	}

	static bool AnyDown(Keyboard kb, Key[] ks)
	{
		foreach (Key k in ks)
			if (kb[k].wasPressedThisFrame)
				return true;
		return false;
	}

	public static Vector2 Move(InputBinding b)
	{
		Vector2 move = Vector2.zero;
		Keyboard kb = Keyboard.current;
		if (kb != null && b.keyboardLayout >= 0)
		{
			int l = b.keyboardLayout;
			move.x += (AnyPressed(kb, KeysFor(l, BindSlot.Right)) ? 1 : 0) - (AnyPressed(kb, KeysFor(l, BindSlot.Left)) ? 1 : 0);
			move.y += (AnyPressed(kb, KeysFor(l, BindSlot.Up)) ? 1 : 0) - (AnyPressed(kb, KeysFor(l, BindSlot.Down)) ? 1 : 0);
		}
		Gamepad p = b.Pad;
		if (p != null)
			move += p.leftStick.ReadValue() + p.dpad.ReadValue();
		return Vector2.ClampMagnitude(move, 1.0f);
	}

	static BindSlot SlotOf(PlayerAction a)
	{
		return (BindSlot)(FirstAction + (int)a);
	}

	public static bool Pressed(InputBinding b, PlayerAction a)
	{
		Keyboard kb = Keyboard.current;
		if (kb != null && b.keyboardLayout >= 0 && AnyDown(kb, KeysFor(b.keyboardLayout, SlotOf(a))))
			return true;
		Gamepad p = b.Pad;
		return p != null && p[PadFor(a)].wasPressedThisFrame;
	}

	public static bool Released(InputBinding b, PlayerAction a)
	{
		Keyboard kb = Keyboard.current;
		if (kb != null && b.keyboardLayout >= 0)
			foreach (Key k in KeysFor(b.keyboardLayout, SlotOf(a)))
				if (kb[k].wasReleasedThisFrame)
					return true;
		Gamepad p = b.Pad;
		return p != null && p[PadFor(a)].wasReleasedThisFrame;
	}

	public static bool Held(InputBinding b, PlayerAction a)
	{
		Keyboard kb = Keyboard.current;
		if (kb != null && b.keyboardLayout >= 0 && AnyPressed(kb, KeysFor(b.keyboardLayout, SlotOf(a))))
			return true;
		Gamepad p = b.Pad;
		return p != null && p[PadFor(a)].isPressed;
	}

	/// Menu steps: -1 or +1 on the frame a direction is first pushed, else 0.
	public static int NavX(InputBinding b)
	{
		Keyboard kb = Keyboard.current;
		if (kb != null && b.keyboardLayout >= 0)
		{
			if (AnyDown(kb, KeysFor(b.keyboardLayout, BindSlot.Left))) return -1;
			if (AnyDown(kb, KeysFor(b.keyboardLayout, BindSlot.Right))) return 1;
		}
		Gamepad p = b.Pad;
		if (p != null)
		{
			if (p.dpad.left.wasPressedThisFrame || p.leftStick.left.wasPressedThisFrame) return -1;
			if (p.dpad.right.wasPressedThisFrame || p.leftStick.right.wasPressedThisFrame) return 1;
		}
		return 0;
	}

	public static int NavY(InputBinding b)
	{
		Keyboard kb = Keyboard.current;
		if (kb != null && b.keyboardLayout >= 0)
		{
			if (AnyDown(kb, KeysFor(b.keyboardLayout, BindSlot.Up))) return -1;
			if (AnyDown(kb, KeysFor(b.keyboardLayout, BindSlot.Down))) return 1;
		}
		Gamepad p = b.Pad;
		if (p != null)
		{
			if (p.dpad.up.wasPressedThisFrame || p.leftStick.up.wasPressedThisFrame) return -1;
			if (p.dpad.down.wasPressedThisFrame || p.leftStick.down.wasPressedThisFrame) return 1;
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
			for (int l = 0; l < Layouts; l++)
			{
				var b = new InputBinding { keyboardLayout = l };
				if (GameSession.IsBound(b))
					continue;
				if (AnyDown(kb, KeysFor(l, BindSlot.Jump)) || AnyDown(kb, KeysFor(l, BindSlot.Attack)))
					return b;
			}
		}
		foreach (Gamepad p in Gamepad.all)
		{
			var b = new InputBinding { gamepadId = p.deviceId };
			if (GameSession.IsBound(b))
				continue;
			if (p[PadFor(PlayerAction.Jump)].wasPressedThisFrame || p.startButton.wasPressedThisFrame)
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
		foreach (Gamepad p in Gamepad.all)
			if (p.startButton.wasPressedThisFrame)
				return true;
		return false;
	}
}
