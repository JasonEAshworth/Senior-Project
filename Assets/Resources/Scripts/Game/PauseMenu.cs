using UnityEngine;

/// Pause: any joined player's pause button (Esc, Backspace, or start). Offers resume, restart,
/// volume, dropping a player out, and leaving to the hub or the title.
public class PauseMenu : MonoBehaviour
{
	public static bool Paused { get; private set; }
	int highlight;
	const int Items = 4;

	void OnDestroy()
	{
		if (Paused)
			Time.timeScale = 1.0f;
		Paused = false;
	}

	void Update()
	{
		if (SceneFlow.Busy || (Time.timeScale == 0.0f && !Paused))
			return;   // results or game over own the screen
		foreach (PlayerSlot s in GameSession.Joined)
		{
			if (s.outOfLives)
				continue;
			if (PlayerControls.Pressed(s.binding, PlayerAction.Pause))
			{
				Toggle();
				return;
			}
			if (Paused)
			{
				int dy = PlayerControls.NavY(s.binding);
				if (dy != 0)
					highlight = (highlight + dy + Items) % Items;
				if (PlayerControls.Pressed(s.binding, PlayerAction.Jump))
					Choose(highlight);
			}
		}
	}

	void Toggle()
	{
		Paused = !Paused;
		Time.timeScale = Paused ? 0.0f : 1.0f;
		AudioListener.pause = Paused;
		highlight = 0;
	}

	void Choose(int i)
	{
		switch (i)
		{
			case 0: Toggle(); break;
			case 1: Leave("MainScene", true); break;
			case 2: Leave("Hub", true); break;
			case 3: Leave("Title", true); break;
		}
	}

	void Leave(string scene, bool restore)
	{
		if (restore && ChapterDirector.Instance != null)
			ChapterDirector.Instance.RestoreSnapshot();
		Paused = false;
		AudioListener.pause = false;
		ChapterDefinition ch = GameSession.Current;
		if (scene == "MainScene" && ch != null)
			SceneFlow.Load(scene, ch.title, ch.tagline);
		else
			SceneFlow.Load(scene);
	}

	void OnGUI()
	{
		if (!Paused)
			return;
		GUI.depth = -100;
		ArcadeUI.Begin();
		float w = ArcadeUI.Width;
		ArcadeUI.Panel(new Rect(0, 0, w, ArcadeUI.VirtualHeight), new Color(0, 0, 0, 0.55f));
		var r = new Rect(w / 2 - 260, 90, 520, 540);
		ArcadeUI.Panel(r, new Color(0.05f, 0.04f, 0.04f, 0.95f));
		ArcadeUI.Frame(r, ArcadeUI.Gold);
		ArcadeUI.Text(new Rect(r.x, r.y + 14, r.width, 50), "PAUSED", 44, ArcadeUI.Gold, TextAnchor.MiddleCenter, true);

		string[] labels = { "Resume", "Restart chapter", "Quit to hub", "Quit to title" };
		for (int i = 0; i < labels.Length; i++)
			if (ArcadeUI.Button(new Rect(r.x + 110, r.y + 80 + i * 56, 300, 46), labels[i], 20, i == highlight))
				Choose(i);

		float y = r.y + 316;
		ArcadeUI.Text(new Rect(r.x + 40, y, 140, 24), "Music", 18, Color.white);
		MusicPlayer.MusicVolume = GUI.HorizontalSlider(new Rect(r.x + 180, y + 6, 280, 20), MusicPlayer.MusicVolume, 0.0f, 1.0f);
		ArcadeUI.Text(new Rect(r.x + 40, y + 34, 140, 24), "Sound", 18, Color.white);
		float master = GUI.HorizontalSlider(new Rect(r.x + 180, y + 40, 280, 20), MusicPlayer.MasterVolume, 0.0f, 1.0f);
		if (!Mathf.Approximately(master, MusicPlayer.MasterVolume))
			MusicPlayer.MasterVolume = master;

		ArcadeUI.Text(new Rect(r.x + 40, y + 76, 300, 24), "Drop out:", 16, ArcadeUI.Dim);
		int col = 0;
		foreach (PlayerSlot s in GameSession.Joined)
		{
			if (GameSession.JoinedCount <= 1)
				break;
			if (ArcadeUI.Button(new Rect(r.x + 40 + col * 112, y + 104, 100, 36), s.Label, 18))
			{
				if (PlayerManager.Instance != null)
					PlayerManager.Instance.RemovePlayer(s);
				GameSession.Leave(s);
				break;
			}
			col++;
		}
		ArcadeUI.Text(new Rect(r.x, r.yMax - 34, r.width, 24), "Up/down + jump, or click", 14, ArcadeUI.Dim, TextAnchor.MiddleCenter);
	}
}
