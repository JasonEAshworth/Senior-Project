using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// Title screen: choose how many people play (1 to 4) and each one's class, then start.
/// The dungeon keeps every player within a fixed distance of the group, so spawning heroes
/// that nobody controls would pin the real players in place.
public class TitleMenu : MonoBehaviour
{
	static readonly playerClass[] classOrder = { playerClass.WARRIOR, playerClass.SORCERER, playerClass.ROGUE, playerClass.WOODSMAN };
	static readonly string[] controls = {
		"WASD move, Space jump, J attack, K ability, L potion, or gamepad 1",
		"Arrows move, Right Shift jump, comma attack, period ability, slash potion (or Numpad 0-3), or gamepad 2",
		"Gamepad 3",
		"Gamepad 4",
	};

	int playerCount = 1;
	readonly int[] choice = { 0, 1, 2, 3 };
	GUIStyle title, label, button;

	void Update()
	{
		bool start = (Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame))
			|| (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame);
		if (start)
			StartGame();
	}

	void StartGame()
	{
		var classes = new List<playerClass>();
		for (int i = 0; i < playerCount; i++)
			classes.Add(classOrder[choice[i]]);
		GameSetup.classes = classes;
		SceneManager.LoadScene("MainScene");
	}

	void OnGUI()
	{
		float scale = Screen.height / 720.0f;
		GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1.0f));
		float width = Screen.width / scale;

		if (title == null)
		{
			title = new GUIStyle(GUI.skin.label) { fontSize = 56, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
			label = new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.MiddleLeft, wordWrap = true };
			button = new GUIStyle(GUI.skin.button) { fontSize = 22 };
		}

		float x = width / 2 - 320;
		GUI.Label(new Rect(0, 60, width, 80), "Senior Project 14", title);
		GUI.Label(new Rect(x, 150, 640, 30), "A co-op dungeon crawler. How many players?", label);

		for (int n = 1; n <= 4; n++)
		{
			bool selected = n == playerCount;
			GUI.color = selected ? new Color(1.0f, 0.85f, 0.4f) : Color.white;
			if (GUI.Button(new Rect(x + (n - 1) * 160, 190, 150, 50), n + (n == 1 ? " player" : " players"), button))
				playerCount = n;
		}
		GUI.color = Color.white;

		for (int i = 0; i < playerCount; i++)
		{
			float y = 270 + i * 70;
			GUI.Label(new Rect(x, y, 110, 50), "Player " + (i + 1), label);
			if (GUI.Button(new Rect(x + 110, y, 180, 50), ClassName(classOrder[choice[i]]), button))
				choice[i] = (choice[i] + 1) % classOrder.Length;
			GUI.Label(new Rect(x + 305, y - 5, 335, 66), controls[i], label);
		}

		if (GUI.Button(new Rect(x + 220, 270 + 4 * 70 + 20, 200, 60), "Start (Enter)", button))
			StartGame();
	}

	static string ClassName(playerClass c)
	{
		switch (c)
		{
			case playerClass.WARRIOR: return "Warrior";
			case playerClass.SORCERER: return "Sorcerer";
			case playerClass.ROGUE: return "Rogue";
			default: return "Woodsman";
		}
	}
}
