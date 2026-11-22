using System.Linq;
using UnityEngine;

/// After King Restless falls: final scores, initials for the high-score table, then credits.
/// Leaving starts a new game, so the seals are cleared here.
public class EndingScreen : MonoBehaviour
{
	enum Phase { Scores, Initials, Credits }
	Phase phase = Phase.Scores;
	float phaseStart;

	void Start()
	{
		Time.timeScale = 1.0f;
		MusicPlayer.Play("Music/ending");
		Backdrop.Build("Map_03_B_E", new Color(0.35f, 0.3f, 0.22f));
		phaseStart = Time.unscaledTime;
	}

	void Update()
	{
		bool pressed = PlayerControls.AnyConfirm() || GameSession.Joined.Any(s => PlayerControls.Pressed(s.binding, PlayerAction.Jump));
		if (phase == Phase.Scores && (pressed && Time.unscaledTime - phaseStart > 1.5f))
		{
			phase = Phase.Initials;
			InitialsEntry.Begin(GameSession.Joined.ToList(), "Defeated the king", () => { phase = Phase.Credits; phaseStart = Time.unscaledTime; });
		}
		else if (phase == Phase.Credits && pressed && Time.unscaledTime - phaseStart > 2.0f)
		{
			GameSession.ResetSeals();
			SceneFlow.Load("Title");
		}
	}

	void OnGUI()
	{
		ArcadeUI.Begin();
		float w = ArcadeUI.Width;
		ArcadeUI.Panel(new Rect(0, 0, w, ArcadeUI.VirtualHeight), new Color(0, 0, 0, 0.45f));
		if (phase == Phase.Scores)
		{
			ArcadeUI.Text(new Rect(0, 70, w, 80), "VICTORY", 80, ArcadeUI.Gold, TextAnchor.MiddleCenter, true);
			ArcadeUI.Text(new Rect(0, 150, w, 30), "King Restless is defeated. All four seals are yours.", 22, Color.white, TextAnchor.MiddleCenter);
			int row = 0;
			foreach (PlayerSlot s in GameSession.Joined)
			{
				float y = 230 + row * 56;
				ArcadeUI.Text(new Rect(w / 2 - 300, y, 300, 44), s.Label + "  " + GameSession.ClassName(s.cls), 26, s.Color, TextAnchor.MiddleLeft, true);
				ArcadeUI.Text(new Rect(w / 2, y, 300, 44), s.points.ToString("N0"), 30, Color.white, TextAnchor.MiddleRight, true);
				row++;
			}
			int team = GameSession.Joined.Sum(s => s.points);
			ArcadeUI.Text(new Rect(0, 500, w, 40), "Team total " + team.ToString("N0"), 26, ArcadeUI.Gold, TextAnchor.MiddleCenter, true);
			if (ArcadeUI.Blink)
				ArcadeUI.Text(new Rect(0, 600, w, 40), "Press jump", 24, Color.white, TextAnchor.MiddleCenter);
		}
		else if (phase == Phase.Credits)
		{
			float scroll = (Time.unscaledTime - phaseStart) * 40.0f;
			string credits =
				"<b>SENIOR PROJECT 14</b>\n\n" +
				"A team capstone at Shawnee State University\n2014 and 2015\n\n" +
				"Everyone who worked on it is listed in the git history.\n\n\n" +
				"<b>2026</b>\nUpgraded to Unity 6 and finished as an arcade game\n\n\n" +
				"Thank you for playing.";
			ArcadeUI.Text(new Rect(0, ArcadeUI.VirtualHeight - scroll, w, 600), credits, 26, Color.white, TextAnchor.UpperCenter);
			if (Time.unscaledTime - phaseStart > 2.0f && ArcadeUI.Blink)
				ArcadeUI.Text(new Rect(0, 660, w, 40), "Press jump for the title screen", 18, ArcadeUI.Dim, TextAnchor.MiddleCenter);
		}
	}
}
