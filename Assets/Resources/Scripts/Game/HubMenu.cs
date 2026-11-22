using UnityEngine;

/// The hub between chapters: pick any uncleared chapter (or replay one), see the seals, spend
/// gold on upgrades, and let more players drop in. The throne room opens with all four seals.
public class HubMenu : MonoBehaviour
{
	struct Upgrade { public string name, blurb; public int baseCost, max; }

	static readonly Upgrade[] upgrades = {
		new Upgrade { name = "Vitality", blurb = "+20% health", baseCost = 100, max = 3 },
		new Upgrade { name = "Might", blurb = "+15% damage", baseCost = 120, max = 3 },
		new Upgrade { name = "Swiftness", blurb = "+8% speed", baseCost = 80, max = 3 },
		new Upgrade { name = "Extra life", blurb = "+1 life", baseCost = 250, max = 9 },
	};

	int selected;
	readonly int[] shopCursor = new int[GameSession.MaxPlayers];
	readonly float[] leaveHeld = new float[GameSession.MaxPlayers];
	string message = "";
	float messageUntil;
	bool confirmReset;

	int CardCount { get { return GameSession.Chapters.Count + 1; } }

	void Start()
	{
		Time.timeScale = 1.0f;
		MusicPlayer.Play("Music/hub");
		Backdrop.Build("Map_00_T_N", new Color(0.24f, 0.22f, 0.2f));
		if (!GameSession.RunStarted)
		{
			// Opened directly in the editor: give player 1 the keyboard and first gamepad.
			if (GameSession.JoinedCount == 0)
				GameSession.Join(new InputBinding { keyboardLayout = 0, gamepadId = Gamepad0() });
			GameSession.NewRun();
		}
		selected = FirstUncleared();
	}

	static int Gamepad0()
	{
		var pads = UnityEngine.InputSystem.Gamepad.all;
		return pads.Count > 0 ? pads[0].deviceId : -1;
	}

	int FirstUncleared()
	{
		for (int i = 0; i < GameSession.Chapters.Count; i++)
			if (!GameSession.HasSeal(GameSession.Chapters[i].sealIndex))
				return i;
		return GameSession.Chapters.Count;
	}

	void Say(string text)
	{
		message = text;
		messageUntil = Time.unscaledTime + 3.0f;
	}

	void Update()
	{
		if (SceneFlow.Busy)
			return;

		InputBinding joining = PlayerControls.DetectJoin();
		if (joining != null)
		{
			PlayerSlot s = GameSession.Join(joining);
			if (s != null)
				Say(s.Label + " joins as the " + GameSession.ClassName(s.cls) + "!");
		}

		foreach (PlayerSlot s in GameSession.Slots)
		{
			if (!s.joined)
				continue;
			int dx = PlayerControls.NavX(s.binding);
			if (dx != 0)
				selected = (selected + dx + CardCount) % CardCount;
			if (PlayerControls.Pressed(s.binding, PlayerAction.Jump) || PlayerControls.Pressed(s.binding, PlayerAction.Attack))
				Enter(selected);

			int dy = PlayerControls.NavY(s.binding);
			if (dy != 0)
				shopCursor[s.index] = (shopCursor[s.index] + dy + upgrades.Length) % upgrades.Length;
			if (PlayerControls.Pressed(s.binding, PlayerAction.Item))
				Buy(s, shopCursor[s.index]);

			// Leaving takes a one-second hold, so a stray press does not drop anyone.
			leaveHeld[s.index] = PlayerControls.Held(s.binding, PlayerAction.Ability) ? leaveHeld[s.index] + Time.unscaledDeltaTime : 0.0f;
			if (leaveHeld[s.index] > 1.0f && GameSession.JoinedCount > 1)
			{
				Say(s.Label + " leaves the party.");
				GameSession.Leave(s);
			}
		}
	}

	void Enter(int card)
	{
		ChapterDefinition ch = card < GameSession.Chapters.Count ? GameSession.Chapters[card] : GameSession.FinalChapter;
		if (ch == null)
			return;
		if (ch.isFinal && !GameSession.FinalUnlocked)
		{
			Say("The throne room is sealed. Clear every chapter first.");
			return;
		}
		GameSession.Current = ch;
		GameSession.ReviveAllForChapter();
		SceneFlow.Load("MainScene", ch.title, ch.tagline);
	}

	static int Cost(Upgrade u, int level) { return u.baseCost * (level + 1); }

	static int Level(PlayerSlot s, int i)
	{
		switch (i)
		{
			case 0: return s.healthLevel;
			case 1: return s.attackLevel;
			case 2: return s.speedLevel;
			default: return s.lives;
		}
	}

	void Buy(PlayerSlot s, int i)
	{
		Upgrade u = upgrades[i];
		int level = Level(s, i);
		int cost = i == 3 ? u.baseCost : Cost(u, level);
		if (level >= u.max)
		{
			Say(s.Label + ": " + u.name + " is maxed out.");
			return;
		}
		if (s.gold < cost)
		{
			Say(s.Label + " needs " + (cost - s.gold) + " more gold for " + u.name + ".");
			return;
		}
		s.gold -= cost;
		switch (i)
		{
			case 0: s.healthLevel++; break;
			case 1: s.attackLevel++; break;
			case 2: s.speedLevel++; break;
			default: s.lives++; break;
		}
		Say(s.Label + " bought " + u.name + "!");
	}

	void OnGUI()
	{
		ArcadeUI.Begin();
		float w = ArcadeUI.Width;

		ArcadeUI.Text(new Rect(0, 18, w, 60), "THE DUNGEON", 52, ArcadeUI.Gold, TextAnchor.MiddleCenter, true);
		DrawSeals(new Rect(w / 2 - 130, 80, 260, 34));

		int n = CardCount;
		float cardW = Mathf.Min(230, (w - 60 - (n - 1) * 14) / n), gap = 14;
		float x0 = (w - (cardW * n + gap * (n - 1))) / 2;
		for (int i = 0; i < n; i++)
			DrawCard(new Rect(x0 + i * (cardW + gap), 130, cardW, 250), i);

		float panelW = Mathf.Min(290, (w - 40 - 12 * (GameSession.MaxPlayers - 1)) / GameSession.MaxPlayers);
		float px0 = (w - (panelW * GameSession.MaxPlayers + 12 * (GameSession.MaxPlayers - 1))) / 2;
		for (int i = 0; i < GameSession.MaxPlayers; i++)
			DrawShop(new Rect(px0 + i * (panelW + 12), 400, panelW, 230), GameSession.Slots[i]);

		if (Time.unscaledTime < messageUntil)
			ArcadeUI.Text(new Rect(0, 636, w, 30), message, 20, Color.white, TextAnchor.MiddleCenter, true);
		else
			ArcadeUI.Text(new Rect(0, 636, w, 30), "Left/right + jump: enter a chapter    Up/down + potion: buy    Hold ability: leave", 15, ArcadeUI.Dim, TextAnchor.MiddleCenter);

		if (ArcadeUI.Button(new Rect(20, 672, 170, 36), "Back to title", 16))
			SceneFlow.Load("Title");
		if (!confirmReset)
		{
			if (ArcadeUI.Button(new Rect(w - 190, 672, 170, 36), "Reset seals", 16))
				confirmReset = true;
		}
		else
		{
			if (ArcadeUI.Button(new Rect(w - 370, 672, 170, 36), "Yes, reset", 16)) { GameSession.ResetSeals(); confirmReset = false; Say("All seals reset."); }
			if (ArcadeUI.Button(new Rect(w - 190, 672, 170, 36), "Keep them", 16)) confirmReset = false;
		}
	}

	void DrawSeals(Rect r)
	{
		int count = GameSession.Chapters.Count;
		for (int i = 0; i < count; i++)
		{
			var cell = new Rect(r.x + i * (r.width / count) + 10, r.y, r.width / count - 20, r.height);
			bool has = GameSession.HasSeal(GameSession.Chapters[i].sealIndex);
			ArcadeUI.Panel(cell, has ? ArcadeUI.Gold : new Color(0.2f, 0.2f, 0.2f, 0.9f));
			ArcadeUI.Text(cell, has ? "SEAL" : "-", 14, has ? Color.black : ArcadeUI.Dim, TextAnchor.MiddleCenter, true);
		}
	}

	void DrawCard(Rect r, int i)
	{
		bool isFinal = i >= GameSession.Chapters.Count;
		ChapterDefinition ch = isFinal ? GameSession.FinalChapter : GameSession.Chapters[i];
		if (ch == null)
			return;
		bool cleared = !isFinal && GameSession.HasSeal(ch.sealIndex);
		bool locked = isFinal && !GameSession.FinalUnlocked;

		ArcadeUI.Panel(r);
		ArcadeUI.Frame(r, i == selected ? ArcadeUI.Gold : new Color(0.35f, 0.3f, 0.25f), i == selected ? 4 : 2);
		ArcadeUI.Text(new Rect(r.x + 10, r.y + 10, r.width - 20, 30), isFinal ? "FINAL" : "CHAPTER " + (i + 1), 14, ArcadeUI.Dim, TextAnchor.MiddleCenter);
		ArcadeUI.Text(new Rect(r.x + 10, r.y + 38, r.width - 20, 60), ch.title, 24, locked ? ArcadeUI.Dim : Color.white, TextAnchor.MiddleCenter, true);
		ArcadeUI.Text(new Rect(r.x + 12, r.y + 100, r.width - 24, 70), ch.tagline, 14, ArcadeUI.Dim, TextAnchor.UpperCenter);
		string status = locked ? "Sealed: " + GameSession.SealCount + " of " + GameSession.Chapters.Count
			: cleared ? "CLEARED" : isFinal ? "OPEN" : "Boss: " + ch.miniBoss.name;
		ArcadeUI.Text(new Rect(r.x + 10, r.y + 168, r.width - 20, 24), status, 15, cleared ? ArcadeUI.Gold : Color.white, TextAnchor.MiddleCenter, true);
		int best = GameSession.BestChapterScore(ch.id);
		if (best > 0)
			ArcadeUI.Text(new Rect(r.x + 10, r.y + 190, r.width - 20, 20), "Best " + best.ToString("N0"), 13, ArcadeUI.Dim, TextAnchor.MiddleCenter);
		if (ArcadeUI.Button(new Rect(r.x + 20, r.yMax - 44, r.width - 40, 34), locked ? "Locked" : cleared ? "Replay" : "Enter", 17, i == selected))
		{
			selected = i;
			Enter(i);
		}
	}

	void DrawShop(Rect r, PlayerSlot s)
	{
		ArcadeUI.Panel(r);
		ArcadeUI.Frame(r, s.joined ? s.Color : new Color(0.3f, 0.3f, 0.3f), 2);
		ArcadeUI.Text(new Rect(r.x + 10, r.y + 6, 60, 30), s.Label, 24, s.Color, TextAnchor.MiddleLeft, true);
		if (!s.joined)
		{
			if (ArcadeUI.Blink)
				ArcadeUI.Text(new Rect(r.x, r.y + 90, r.width, 40), "Press jump or start\nto join", 18, Color.white, TextAnchor.MiddleCenter);
			return;
		}
		ArcadeUI.Text(new Rect(r.x + 70, r.y + 6, r.width - 80, 30), GameSession.ClassName(s.cls) + "   " + s.gold + " gold   " + s.lives + " lives", 15, Color.white, TextAnchor.MiddleRight);
		for (int i = 0; i < upgrades.Length; i++)
		{
			Upgrade u = upgrades[i];
			int level = Level(s, i);
			int cost = i == 3 ? u.baseCost : Cost(u, level);
			var row = new Rect(r.x + 8, r.y + 44 + i * 44, r.width - 16, 40);
			if (i == shopCursor[s.index])
				ArcadeUI.Panel(row, new Color(0.3f, 0.25f, 0.1f, 0.8f));
			string lv = i == 3 ? "" : "  Lv " + level + "/" + u.max;
			ArcadeUI.Text(new Rect(row.x + 6, row.y, 170, 22), "<b>" + u.name + "</b>" + lv, 15, Color.white);
			ArcadeUI.Text(new Rect(row.x + 6, row.y + 20, 170, 18), u.blurb, 12, ArcadeUI.Dim);
			bool maxed = level >= u.max;
			if (GUI.Button(new Rect(row.xMax - 96, row.y + 6, 90, 28), maxed ? "MAX" : cost + "g"))
			{
				shopCursor[s.index] = i;
				Buy(s, i);
			}
		}
	}
}
