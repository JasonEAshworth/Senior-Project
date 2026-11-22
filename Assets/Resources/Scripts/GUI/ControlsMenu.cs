using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

/// Shows and rebinds the controls: one column per keyboard layout and one for gamepads. Pick a
/// cell with the arrows or WASD and Space, or click it, then press the new key or button.
/// Menu keys are fixed here, so a bad binding can never lock a player out of this screen.
public class ControlsMenu : MonoBehaviour
{
	public static bool IsOpen { get { return instance != null; } }
	static ControlsMenu instance;

	static readonly string[] RowNames = { "Move up", "Move down", "Move left", "Move right", "Jump", "Attack", "Ability", "Potion", "Pause" };
	static readonly string[] ColumnNames = { "Keyboard, left", "Keyboard, right", "Any gamepad" };
	const int Columns = 3, PadColumn = 2;
	const int ButtonRow = PlayerControls.Slots;   // the last row: Reset and Back
	const float CaptureSeconds = 8.0f;

	System.Action onClose;
	int row, col;
	bool capturing;
	float captureStart;
	int quietUntilFrame;
	string message = "";

	public static void Show(System.Action onClose = null)
	{
		if (instance != null)
			return;
		var go = new GameObject("ControlsMenu");
		instance = go.AddComponent<ControlsMenu>();
		instance.onClose = onClose;
		// The key that opened the menu must not also act in it.
		instance.quietUntilFrame = Time.frameCount + 1;
	}

	/// F1, or a gamepad's Back button: the menus that host this screen open it on either.
	public static bool OpenRequested()
	{
		Keyboard kb = Keyboard.current;
		if (kb != null && kb.f1Key.wasPressedThisFrame)
			return true;
		foreach (Gamepad p in Gamepad.all)
			if (p.selectButton.wasPressedThisFrame)
				return true;
		return false;
	}

	void OnDestroy()
	{
		if (instance == this)
			instance = null;
	}

	static bool Selectable(int r, int c)
	{
		if (r == ButtonRow)
			return c < 2;
		return !(c == PadColumn && r < (int)BindSlot.Jump);   // the stick and d-pad always move
	}

	void Update()
	{
		if (Time.frameCount <= quietUntilFrame)
			return;
		if (capturing)
		{
			Capture();
			return;
		}
		Keyboard kb = Keyboard.current;
		int dx = 0, dy = 0;
		bool choose = false, back = false;
		if (kb != null)
		{
			if (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame) dy = -1;
			if (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame) dy = 1;
			if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame) dx = -1;
			if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame) dx = 1;
			choose = kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame;
			back = kb.escapeKey.wasPressedThisFrame || kb.backspaceKey.wasPressedThisFrame;
		}
		foreach (Gamepad p in Gamepad.all)
		{
			if (p.dpad.up.wasPressedThisFrame || p.leftStick.up.wasPressedThisFrame) dy = -1;
			if (p.dpad.down.wasPressedThisFrame || p.leftStick.down.wasPressedThisFrame) dy = 1;
			if (p.dpad.left.wasPressedThisFrame || p.leftStick.left.wasPressedThisFrame) dx = -1;
			if (p.dpad.right.wasPressedThisFrame || p.leftStick.right.wasPressedThisFrame) dx = 1;
			choose |= p.buttonSouth.wasPressedThisFrame;
			back |= p.buttonEast.wasPressedThisFrame || p.selectButton.wasPressedThisFrame;
		}
		if (dy != 0 || dx != 0)
			Step(dx, dy);
		if (back)
			Close();
		else if (choose)
			Choose(row, col);
	}

	void Step(int dx, int dy)
	{
		if (dy != 0)
		{
			row = (row + dy + ButtonRow + 1) % (ButtonRow + 1);
			// The button row and the gamepad's move rows have nothing in the last column.
			if (!Selectable(row, col))
				col = 1;
		}
		if (dx != 0)
			do col = (col + dx + Columns) % Columns; while (!Selectable(row, col));
	}

	void Choose(int r, int c)
	{
		row = r;
		col = c;
		if (r == ButtonRow)
		{
			if (c == 0)
			{
				PlayerControls.ResetToDefaults();
				message = "Controls reset to the defaults.";
			}
			else
				Close();
			return;
		}
		if (!Selectable(r, c))
			return;
		capturing = true;
		captureStart = Time.unscaledTime;
		// The key or click that chose the cell must not be taken as the new binding.
		quietUntilFrame = Time.frameCount + 1;
		message = "";
	}

	void Capture()
	{
		Keyboard kb = Keyboard.current;
		bool cancel = Time.unscaledTime - captureStart > CaptureSeconds
			|| (kb != null && (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame));
		if (cancel)
		{
			capturing = false;
			message = "No change.";
			return;
		}
		if (col == PadColumn)
		{
			foreach (Gamepad p in Gamepad.all)
				foreach (GamepadButton b in PlayerControls.BindableButtons)
					if (p[b].wasPressedThisFrame)
					{
						PlayerAction a = (PlayerAction)(row - (int)BindSlot.Jump);
						PlayerControls.BindPad(a, b);
						Bound(RowNames[row] + " is now " + PlayerControls.ButtonName(b) + " on every gamepad.");
						return;
					}
			return;
		}
		if (kb == null)
			return;
		foreach (KeyControl k in kb.allKeys)
		{
			if (k == null || !k.wasPressedThisFrame || k.keyCode == Key.Enter || k.keyCode == Key.NumpadEnter)
				continue;
			PlayerControls.BindKey(col, (BindSlot)row, k.keyCode);
			Bound(RowNames[row] + " on the " + (col == 0 ? "left" : "right") + " keyboard is now " + PlayerControls.KeyName(k.keyCode) + ".");
			return;
		}
	}

	void Bound(string text)
	{
		capturing = false;
		message = text;
		// Let the new key's own press end before the menu reads it again.
		quietUntilFrame = Time.frameCount + 1;
	}

	void Close()
	{
		Destroy(gameObject);
		if (onClose != null)
			onClose();
	}

	void OnGUI()
	{
		GUI.depth = -200;
		ArcadeUI.Begin();
		float w = ArcadeUI.Width;
		ArcadeUI.Panel(new Rect(0, 0, w, ArcadeUI.VirtualHeight), new Color(0, 0, 0, 0.7f));
		var r = new Rect(w / 2 - 470, 40, 940, 640);
		ArcadeUI.Panel(r, new Color(0.05f, 0.04f, 0.04f, 0.96f));
		ArcadeUI.Frame(r, ArcadeUI.Gold);
		ArcadeUI.Text(new Rect(r.x, r.y + 12, r.width, 50), "CONTROLS", 40, ArcadeUI.Gold, TextAnchor.MiddleCenter, true);

		float labelW = 200, cellW = 230, rowH = 42, x0 = r.x + 30, y0 = r.y + 110;
		for (int c = 0; c < Columns; c++)
			ArcadeUI.Text(new Rect(x0 + labelW + c * cellW, y0 - 36, cellW - 10, 30), ColumnNames[c], 18, ArcadeUI.Dim, TextAnchor.MiddleCenter, true);
		for (int i = 0; i < PlayerControls.Slots; i++)
		{
			float y = y0 + i * rowH;
			ArcadeUI.Text(new Rect(x0, y, labelW, rowH - 6), RowNames[i], 19, Color.white);
			for (int c = 0; c < Columns; c++)
			{
				var cell = new Rect(x0 + labelW + c * cellW, y, cellW - 10, rowH - 6);
				bool selected = i == row && c == col;
				if (!Selectable(i, c))
				{
					ArcadeUI.Text(cell, "Stick or d-pad", 16, ArcadeUI.Dim, TextAnchor.MiddleCenter);
					continue;
				}
				string text = selected && capturing ? (ArcadeUI.Blink ? "Press a " + (c == PadColumn ? "button" : "key") : "")
					: c == PadColumn ? PlayerControls.PadName((PlayerAction)(i - (int)BindSlot.Jump))
					: PlayerControls.KeysName(c, (BindSlot)i);
				if (ArcadeUI.Button(cell, text, 17, selected) && !capturing)
					Choose(i, c);
			}
		}

		float by = y0 + PlayerControls.Slots * rowH + 14;
		if (ArcadeUI.Button(new Rect(r.x + r.width / 2 - 250, by, 240, 44), "Reset to defaults", 19, row == ButtonRow && col == 0) && !capturing)
			Choose(ButtonRow, 0);
		if (ArcadeUI.Button(new Rect(r.x + r.width / 2 + 10, by, 240, 44), "Back", 19, row == ButtonRow && col == 1) && !capturing)
			Choose(ButtonRow, 1);

		string hint = capturing
			? "Press the new " + (col == PadColumn ? "gamepad button" : "key") + " for " + RowNames[row].ToLower() + ".  Enter cancels."
			: message.Length > 0 ? message
			: "Arrows or WASD to pick, Space to change, Esc to go back. Or click.";
		ArcadeUI.Text(new Rect(r.x, r.yMax - 44, r.width, 30), hint, 16, capturing ? Color.white : ArcadeUI.Dim, TextAnchor.MiddleCenter);
	}
}
