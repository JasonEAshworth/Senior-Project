using UnityEngine;

/// Title screen and lobby. Each player presses jump (or start) on their own keyboard layout or
/// gamepad to take a slot, picks a hero with left and right, and anyone presses Enter or start
/// to begin. With nobody joined it runs an attract loop over a dungeon room.
public class TitleMenu : MonoBehaviour
{
	const float PanelSeconds = 8.0f;

	float joinedFrame = -1;

	void Start()
	{
		Time.timeScale = 1.0f;
		MusicPlayer.Play("Music/title");
		// Coming back to the title ends the run; seals stay saved.
		foreach (PlayerSlot s in GameSession.Slots)
			GameSession.Leave(s);
		GameSession.RunStarted = false;
		Backdrop.Build("Map_04_S_N", new Color(0.22f, 0.2f, 0.2f));
	}

	void Update()
	{
		if (SceneFlow.Busy)
			return;

		InputBinding joining = PlayerControls.DetectJoin();
		if (joining == null && GameSession.JoinedCount == 0 && PlayerControls.AnyConfirm())
			joining = new InputBinding { keyboardLayout = 0 };
		if (joining != null && GameSession.Join(joining) != null)
			joinedFrame = Time.frameCount;

		foreach (PlayerSlot s in GameSession.Slots)
		{
			if (!s.joined)
				continue;
			int step = PlayerControls.NavX(s.binding);
			if (step != 0)
				s.cls = GameSession.NextClass(s.cls, step);
			if (Time.frameCount != joinedFrame && PlayerControls.Pressed(s.binding, PlayerAction.Ability))
				GameSession.Leave(s);
		}

		if (Time.frameCount != joinedFrame && GameSession.JoinedCount > 0 && PlayerControls.AnyConfirm())
			StartRun();
	}

	void StartRun()
	{
		GameSession.NewRun();
		SceneFlow.Load("Hub", "The Dungeon", "Four chapters. Four seals. One king.");
	}

	void OnGUI()
	{
		ArcadeUI.Begin();
		float w = ArcadeUI.Width;

		ArcadeUI.Text(new Rect(0, 40, w, 90), "RISE OF THE RESTLESS", 68, ArcadeUI.Gold, TextAnchor.MiddleCenter, true);
		ArcadeUI.Text(new Rect(0, 120, w, 30), "A co-op dungeon crawler for 1 to 4 players", 20, ArcadeUI.Dim, TextAnchor.MiddleCenter);

		float cardW = 270, gap = 18;
		float x0 = (w - (cardW * 4 + gap * 3)) / 2;
		for (int i = 0; i < GameSession.MaxPlayers; i++)
			DrawCard(new Rect(x0 + i * (cardW + gap), 430, cardW, 190), GameSession.Slots[i]);

		if (GameSession.JoinedCount > 0)
		{
			if (ArcadeUI.Button(new Rect(w / 2 - 150, 640, 300, 56), "START  (Enter / Start)", 22, true))
				StartRun();
		}
		else
		{
			if (ArcadeUI.Blink)
				ArcadeUI.Text(new Rect(0, 640, w, 50), "PRESS JUMP OR START TO JOIN", 30, Color.white, TextAnchor.MiddleCenter, true);
		}

		DrawAttractPanel(new Rect(w / 2 - 330, 175, 660, 235));
	}

	void DrawCard(Rect r, PlayerSlot s)
	{
		ArcadeUI.Panel(r);
		ArcadeUI.Frame(r, s.joined ? s.Color : new Color(0.3f, 0.3f, 0.3f));
		ArcadeUI.Text(new Rect(r.x + 14, r.y + 8, 80, 40), s.Label, 32, s.Color, TextAnchor.MiddleLeft, true);
		if (!s.joined)
		{
			if (ArcadeUI.Blink)
				ArcadeUI.Text(new Rect(r.x, r.y + 70, r.width, 50), "Press jump\nor start", 22, Color.white, TextAnchor.MiddleCenter);
			return;
		}
		ArcadeUI.Text(new Rect(r.x + 90, r.y + 8, r.width - 100, 40), s.binding.Describe(), 14, ArcadeUI.Dim, TextAnchor.MiddleRight);
		if (GUI.Button(new Rect(r.x + 14, r.y + 62, 34, 44), "<"))
			s.cls = GameSession.NextClass(s.cls, -1);
		ArcadeUI.Text(new Rect(r.x + 50, r.y + 60, r.width - 100, 48), GameSession.ClassName(s.cls), 30, Color.white, TextAnchor.MiddleCenter, true);
		if (GUI.Button(new Rect(r.xMax - 48, r.y + 62, 34, 44), ">"))
			s.cls = GameSession.NextClass(s.cls, 1);
		ArcadeUI.Text(new Rect(r.x + 14, r.y + 116, r.width - 28, 66), ClassBlurb(s.cls) + "\nLeft/right: hero    Ability: leave", 14, ArcadeUI.Dim, TextAnchor.UpperLeft);
	}

	static string ClassBlurb(playerClass c)
	{
		switch (c)
		{
			case playerClass.WARRIOR: return "Melee combos and a whirlwind.";
			case playerClass.SORCERER: return "Ice shots, fireballs, and a meteor.";
			case playerClass.ROGUE: return "Fast blades, dashes, and stealth.";
			default: return "Arrows, bombs, and a hawk.";
		}
	}

	void DrawAttractPanel(Rect r)
	{
		int panel = GameSession.JoinedCount > 0 ? 0 : Mathf.FloorToInt(Time.unscaledTime / PanelSeconds) % 3;
		ArcadeUI.Panel(r, new Color(0, 0, 0, 0.6f));
		if (panel == 0)
		{
			ArcadeUI.Text(new Rect(r.x + 20, r.y + 10, r.width - 40, 30), "CONTROLS", 22, ArcadeUI.Gold, TextAnchor.MiddleCenter, true);
			string text =
				"<b>Up to 4 players.</b> Two can share the keyboard; the rest use gamepads.\n" +
				"<b>Keyboard, left:</b> " + PlayerControls.LayoutHelp[0] + "\n" +
				"<b>Keyboard, right:</b> " + PlayerControls.LayoutHelp[1] + "\n" +
				"<b>Any gamepad:</b> stick move, A jump, X attack, B ability, Y potion, Start pause\n\n" +
				"Clear all four chapters, in any order, to open the throne room.";
			ArcadeUI.Text(new Rect(r.x + 24, r.y + 46, r.width - 48, r.height - 56), text, 16, Color.white, TextAnchor.UpperLeft);
		}
		else if (panel == 1)
		{
			ArcadeUI.Text(new Rect(r.x + 20, r.y + 10, r.width - 40, 30), "HIGH SCORES", 22, ArcadeUI.Gold, TextAnchor.MiddleCenter, true);
			var scores = GameSession.HighScores;
			if (scores.Count == 0)
				ArcadeUI.Text(new Rect(r.x, r.y + 90, r.width, 40), "No scores yet. Be the first.", 18, ArcadeUI.Dim, TextAnchor.MiddleCenter);
			for (int i = 0; i < scores.Count; i++)
			{
				float y = r.y + 46 + i * 18;
				ArcadeUI.Text(new Rect(r.x + 120, y, 60, 18), (i + 1) + ".", 15, ArcadeUI.Dim, TextAnchor.MiddleRight);
				ArcadeUI.Text(new Rect(r.x + 200, y, 80, 18), scores[i].initials, 15, Color.white);
				ArcadeUI.Text(new Rect(r.x + 280, y, 120, 18), scores[i].points.ToString("N0"), 15, ArcadeUI.Gold, TextAnchor.MiddleRight);
				ArcadeUI.Text(new Rect(r.x + 420, y, 200, 18), scores[i].reached, 15, ArcadeUI.Dim);
			}
		}
		else
		{
			ArcadeUI.Text(new Rect(r.x + 20, r.y + 10, r.width - 40, 30), "ABOUT", 22, ArcadeUI.Gold, TextAnchor.MiddleCenter, true);
			string text =
				"Rise of the Restless began as Senior Project 14, a team capstone at Shawnee State " +
				"University, built in Unity 4.6 in 2014 and 2015. The git history lists everyone who worked on it.\n\n" +
				"Upgraded to Unity 6 and finished as an arcade game in 2026.\n" +
				"Seals cleared: " + GameSession.SealCount + " of " + GameSession.Chapters.Count;
			ArcadeUI.Text(new Rect(r.x + 24, r.y + 46, r.width - 48, r.height - 56), text, 16, Color.white, TextAnchor.UpperLeft);
		}
	}
}
