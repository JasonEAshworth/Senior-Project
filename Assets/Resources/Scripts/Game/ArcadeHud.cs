using System.Collections.Generic;
using UnityEngine;

/// The arcade layer over the 2015 health bars: player colors, lives, score, combo, power-ups,
/// join prompts, continues, revive bars, the boss bar, damage numbers, and announcements.
public class ArcadeHud : MonoBehaviour
{
	struct Floater { public Vector3 world; public string text; public Color color; public float born, life; public int size; }
	struct Banner { public string text; public Color color; public float born; }

	static readonly List<Floater> floaters = new List<Floater>();
	static readonly List<Banner> banners = new List<Banner>();

	public static void Damage(Vector3 at, float amount, bool toPlayer)
	{
		if (amount < 0.5f)
			return;
		floaters.Add(new Floater
		{
			world = at + Vector3.up * 2.0f + Random.insideUnitSphere * 0.4f,
			text = Mathf.RoundToInt(amount).ToString(),
			color = toPlayer ? new Color(1.0f, 0.35f, 0.3f) : Color.white,
			born = Time.time, life = 0.8f, size = toPlayer ? 22 : 18,
		});
	}

	public static void Popup(Vector3 at, string text, Color color)
	{
		floaters.Add(new Floater { world = at + Vector3.up * 2.6f, text = text, color = color, born = Time.time, life = 1.3f, size = 20 });
	}

	public static void Announce(string text, Color color)
	{
		banners.Add(new Banner { text = text, color = color, born = Time.unscaledTime });
		if (banners.Count > 3)
			banners.RemoveAt(0);
	}

	void OnDestroy()
	{
		floaters.Clear();
		banners.Clear();
	}

	void OnGUI()
	{
		GUI.depth = 10;
		Camera cam = Camera.main;
		ArcadeUI.Begin();
		float w = ArcadeUI.Width;
		float s = ArcadeUI.Scale;

		DrawChapterLabel();
		DrawBossBar(w);

		if (cam != null)
		{
			DrawEnemyBars(cam, s);
			DrawDowned(cam, s);
			DrawFloaters(cam, s);
		}

		float quarter = w / GameSession.MaxPlayers;
		for (int i = 0; i < GameSession.MaxPlayers; i++)
			DrawSlot(new Rect(i * quarter + 8, ArcadeUI.VirtualHeight - 172, quarter - 16, 34), GameSession.Slots[i]);

		DrawBanners(w);
	}

	static Vector2 ToGui(Camera cam, Vector3 world, float s, out bool visible)
	{
		Vector3 p = cam.WorldToScreenPoint(world);
		visible = p.z > 0;
		return new Vector2(p.x / s, (Screen.height - p.y) / s);
	}

	void DrawChapterLabel()
	{
		ChapterDirector d = ChapterDirector.Instance;
		if (d == null || GameSession.Current == null)
			return;
		string room = d.RoomsTotal > 1 ? "   Room " + d.RoomsReached + " of " + d.RoomsTotal : "";
		ArcadeUI.Text(new Rect(16, 10, 700, 26), "<b>" + GameSession.Current.title + "</b>" + room, 18, ArcadeUI.Gold);
		ArcadeUI.Text(new Rect(16, 34, 700, 22), "Continues: " + GameSession.Continues, 14, ArcadeUI.Dim);
	}

	void DrawBossBar(float w)
	{
		// Only once the party reaches the boss: every room is built at the start of a chapter.
		if (ChapterDirector.Instance != null && !ChapterDirector.Instance.BossEngaged)
			return;
		EnemyBase boss = null;
		foreach (EnemyBase e in EnemyBase.All)
			if (e != null && e.IsBoss && !e.dead && e.gameObject.activeInHierarchy)
				boss = e;
		if (boss == null)
			return;
		var r = new Rect(w / 2 - 320, 18, 640, 22);
		ArcadeUI.Text(new Rect(r.x, r.y - 2, r.width, 22), boss.bossName.ToUpper(), 18, ArcadeUI.Gold, TextAnchor.MiddleCenter, true);
		ArcadeUI.Bar(new Rect(r.x, r.y + 24, r.width, 14), boss.health / Mathf.Max(1.0f, boss.maxHealth), new Color(0.85f, 0.15f, 0.1f), new Color(0, 0, 0, 0.7f));
		ArcadeUI.Frame(new Rect(r.x, r.y + 24, r.width, 14), new Color(0.9f, 0.75f, 0.4f), 2);
	}

	void DrawEnemyBars(Camera cam, float s)
	{
		foreach (EnemyBase e in EnemyBase.All)
		{
			if (e == null || e.dead || e.IsBoss || Time.time - e.lastHitTime > 3.0f)
				continue;
			Vector2 g = ToGui(cam, e.transform.position + Vector3.up * (2.2f * e.transform.lossyScale.y), s, out bool vis);
			if (!vis)
				continue;
			ArcadeUI.Bar(new Rect(g.x - 24, g.y, 48, 6), e.health / Mathf.Max(1.0f, e.maxHealth), new Color(0.9f, 0.2f, 0.15f), new Color(0, 0, 0, 0.6f));
			if (!string.IsNullOrEmpty(e.displayName))
				ArcadeUI.Text(new Rect(g.x - 80, g.y - 18, 160, 16), e.displayName, 12, Color.white, TextAnchor.MiddleCenter);
		}
	}

	void DrawDowned(Camera cam, float s)
	{
		PlayerManager pm = PlayerManager.Instance;
		if (pm == null)
			return;
		foreach (GameObject p in pm.players)
		{
			if (p == null || !p.activeInHierarchy)
				continue;
			PlayerBase pb = p.GetComponent<PlayerBase>();
			if (!pb.downed || pb.slot == null)
				continue;
			Vector2 g = ToGui(cam, p.transform.position + Vector3.up * 2.4f, s, out bool vis);
			if (!vis)
				continue;
			ArcadeUI.Text(new Rect(g.x - 90, g.y - 26, 180, 20), pb.slot.Label + " DOWN " + Mathf.CeilToInt(pb.downedTimer), 15, pb.slot.Color, TextAnchor.MiddleCenter, true);
			ArcadeUI.Bar(new Rect(g.x - 40, g.y - 4, 80, 8), pb.reviveProgress / PlayerBase.ReviveSeconds, Color.white, new Color(0, 0, 0, 0.7f));
			ArcadeUI.Text(new Rect(g.x - 90, g.y + 6, 180, 16), "Stand close to revive", 11, ArcadeUI.Dim, TextAnchor.MiddleCenter);
		}
	}

	void DrawFloaters(Camera cam, float s)
	{
		for (int i = floaters.Count - 1; i >= 0; i--)
		{
			Floater f = floaters[i];
			float k = (Time.time - f.born) / f.life;
			if (k >= 1.0f)
			{
				floaters.RemoveAt(i);
				continue;
			}
			Vector2 g = ToGui(cam, f.world + Vector3.up * k * 1.2f, s, out bool vis);
			if (!vis)
				continue;
			Color c = new Color(f.color.r, f.color.g, f.color.b, 1.0f - k * k);
			ArcadeUI.Text(new Rect(g.x - 100, g.y - 12, 200, 24), f.text, f.size, c, TextAnchor.MiddleCenter, true);
		}
	}

	void DrawSlot(Rect r, PlayerSlot slot)
	{
		ArcadeUI.Panel(r, new Color(0, 0, 0, 0.55f));
		ArcadeUI.Panel(new Rect(r.x, r.y, 5, r.height), slot.Color);
		ArcadeUI.Text(new Rect(r.x + 10, r.y, 40, r.height), slot.Label, 18, slot.Color, TextAnchor.MiddleLeft, true);
		if (!slot.joined)
		{
			if (ArcadeUI.Blink)
				ArcadeUI.Text(new Rect(r.x + 50, r.y, r.width - 56, r.height), "PRESS JUMP OR START", 14, Color.white, TextAnchor.MiddleLeft, true);
			return;
		}
		if (slot.outOfLives)
		{
			float left = ChapterDirector.Instance != null ? ChapterDirector.Instance.ContinueSecondsLeft(slot) : 0;
			string text = left > 0 && GameSession.Continues > 0
				? "CONTINUE? " + Mathf.CeilToInt(left) + "  (jump)"
				: "OUT";
			ArcadeUI.Text(new Rect(r.x + 50, r.y, r.width - 56, r.height), text, 15, ArcadeUI.Blink ? Color.white : slot.Color, TextAnchor.MiddleLeft, true);
			return;
		}
		ArcadeUI.Text(new Rect(r.x + 48, r.y, 70, r.height), "♥ x" + slot.lives, 15, new Color(1.0f, 0.45f, 0.45f), TextAnchor.MiddleLeft, true);
		ArcadeUI.Text(new Rect(r.x + 112, r.y, r.width - 118, r.height), slot.points.ToString("N0"), 18, Color.white, TextAnchor.MiddleRight, true);

		GameObject go = PlayerManager.Instance != null ? PlayerManager.Instance.PlayerFor(slot.index) : null;
		string extra = "";
		if (slot.combo > 1 && Time.time - slot.lastKillTime <= GameSession.ComboWindow)
			extra += "COMBO x" + slot.ComboMultiplier.ToString("0.0") + "  ";
		if (go != null)
		{
			PlayerBase pb = go.GetComponent<PlayerBase>();
			if (Time.time < pb.furyUntil) extra += "FURY " + Mathf.CeilToInt(pb.furyUntil - Time.time) + "  ";
			if (Time.time < pb.hasteUntil) extra += "HASTE " + Mathf.CeilToInt(pb.hasteUntil - Time.time) + "  ";
			if (pb.Shielded) extra += "SHIELD  ";
		}
		if (extra.Length > 0)
			ArcadeUI.Text(new Rect(r.x, r.y - 22, r.width, 20), extra, 13, ArcadeUI.Gold, TextAnchor.MiddleLeft, true);
	}

	void DrawBanners(float w)
	{
		for (int i = banners.Count - 1; i >= 0; i--)
		{
			Banner b = banners[i];
			float k = (Time.unscaledTime - b.born) / 2.2f;
			if (k >= 1.0f)
			{
				banners.RemoveAt(i);
				continue;
			}
			float a = k < 0.8f ? 1.0f : 1.0f - (k - 0.8f) / 0.2f;
			int index = banners.Count - 1 - i;
			ArcadeUI.Text(new Rect(0, 210 + index * 50, w, 50), b.text, 40, new Color(b.color.r, b.color.g, b.color.b, a), TextAnchor.MiddleCenter, true);
		}
	}
}
