using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// One of the four player slots for the whole run, kept across scenes.
public class PlayerSlot
{
	public int index;
	public bool joined;
	public playerClass cls;
	public InputBinding binding = new InputBinding();

	public int lives;
	public bool outOfLives;
	public int points;
	public int gold;
	public int kills;
	public int deaths;

	// Hub shop levels. Each level is applied when the hero spawns.
	public int healthLevel, attackLevel, speedLevel;

	// Combo: kills credited within ComboWindow seconds of each other.
	public int combo;
	public float lastKillTime = -100.0f;

	public float ComboMultiplier { get { return 1.0f + Mathf.Min(combo, 15) * 0.2f; } }

	public static readonly Color[] Colors = {
		new Color(0.95f, 0.25f, 0.2f), new Color(0.25f, 0.55f, 1.0f),
		new Color(0.3f, 0.85f, 0.3f), new Color(1.0f, 0.85f, 0.2f) };
	public Color Color { get { return Colors[index]; } }
	public string Label { get { return "P" + (index + 1); } }
}

[System.Serializable]
public class HighScore
{
	public string initials;
	public int points;
	public string reached;
}

[System.Serializable]
class HighScoreTable { public List<HighScore> entries = new List<HighScore>(); }

/// The state of the run: who is playing, on which device, with which hero, their lives and
/// score, the seals earned (saved between visits), and the local high-score table.
public static class GameSession
{
	public const int MaxPlayers = 4;
	public const int StartingLives = 3;
	public const int StartingContinues = 3;
	public const float ComboWindow = 4.0f;
	public const int HighScoreCount = 10;

	public static readonly PlayerSlot[] Slots = Enumerable.Range(0, MaxPlayers).Select(i => new PlayerSlot { index = i }).ToArray();
	public static ChapterDefinition Current;
	public static int Continues = StartingContinues;
	public static bool RunStarted;

	public static IEnumerable<PlayerSlot> Joined { get { return Slots.Where(s => s.joined); } }
	public static int JoinedCount { get { return Slots.Count(s => s.joined); } }

	public static bool IsBound(InputBinding b)
	{
		return Slots.Any(s => s.joined && s.binding.Uses(b));
	}

	/// Puts a device into the first free slot with a hero nobody else is playing.
	/// Returns the slot, or null when all four are taken.
	public static PlayerSlot Join(InputBinding binding)
	{
		PlayerSlot slot = Slots.FirstOrDefault(s => !s.joined);
		if (slot == null)
			return null;
		slot.joined = true;
		slot.binding = binding;
		slot.cls = FreeClass(slot.index);
		ResetSlot(slot);
		return slot;
	}

	public static void Leave(PlayerSlot slot)
	{
		slot.joined = false;
		slot.binding = new InputBinding();
	}

	static readonly playerClass[] classOrder = { playerClass.WARRIOR, playerClass.SORCERER, playerClass.ROGUE, playerClass.WOODSMAN };

	public static playerClass FreeClass(int forSlot)
	{
		foreach (playerClass c in classOrder)
			if (!Slots.Any(s => s.joined && s.index != forSlot && s.cls == c))
				return c;
		return classOrder[forSlot % classOrder.Length];
	}

	public static playerClass NextClass(playerClass c, int step)
	{
		int i = System.Array.IndexOf(classOrder, c);
		return classOrder[(i + step + classOrder.Length) % classOrder.Length];
	}

	public static string ClassName(playerClass c)
	{
		switch (c)
		{
			case playerClass.WARRIOR: return "Warrior";
			case playerClass.SORCERER: return "Sorcerer";
			case playerClass.ROGUE: return "Rogue";
			default: return "Woodsman";
		}
	}

	static void ResetSlot(PlayerSlot s)
	{
		s.lives = StartingLives;
		s.outOfLives = false;
		s.points = 0;
		s.gold = 0;
		s.kills = 0;
		s.deaths = 0;
		s.healthLevel = s.attackLevel = s.speedLevel = 0;
		s.combo = 0;
	}

	/// A fresh run for everyone who has joined. Seals are kept: a game over sends the party
	/// back to the hub without losing the chapters it has already cleared.
	public static void NewRun()
	{
		foreach (PlayerSlot s in Joined)
			ResetSlot(s);
		Continues = StartingContinues;
		RunStarted = true;
	}

	/// Refills lives for a new chapter attempt without touching score, gold, or upgrades.
	public static void ReviveAllForChapter()
	{
		foreach (PlayerSlot s in Joined)
		{
			if (s.outOfLives || s.lives <= 0)
				s.lives = 1;
			s.outOfLives = false;
			s.combo = 0;
		}
	}

	// ---- Chapters and seals ----

	static List<ChapterDefinition> chapters;
	static ChapterDefinition finalChapter;

	static void LoadChapters()
	{
		if (chapters != null)
			return;
		ChapterDefinition[] all = Resources.LoadAll<ChapterDefinition>("Chapters");
		chapters = all.Where(c => !c.isFinal).OrderBy(c => c.sealIndex).ToList();
		finalChapter = all.FirstOrDefault(c => c.isFinal);
	}

	public static List<ChapterDefinition> Chapters { get { LoadChapters(); return chapters; } }
	public static ChapterDefinition FinalChapter { get { LoadChapters(); return finalChapter; } }

	const string SealsKey = "seals";

	public static int SealMask
	{
		get { return PlayerPrefs.GetInt(SealsKey, 0); }
		set { PlayerPrefs.SetInt(SealsKey, value); PlayerPrefs.Save(); }
	}

	public static bool HasSeal(int index) { return (SealMask & (1 << index)) != 0; }
	public static int SealCount { get { int m = SealMask, n = 0; while (m != 0) { n += m & 1; m >>= 1; } return n; } }
	public static bool FinalUnlocked { get { return SealCount >= Chapters.Count && Chapters.Count > 0; } }

	public static void AwardSeal(int index)
	{
		if (index >= 0)
			SealMask = SealMask | (1 << index);
	}

	public static void ResetSeals() { SealMask = 0; }

	public static int BestChapterScore(string id) { return PlayerPrefs.GetInt("best_" + id, 0); }

	public static void RecordChapterScore(string id, int points)
	{
		if (points > BestChapterScore(id))
		{
			PlayerPrefs.SetInt("best_" + id, points);
			PlayerPrefs.Save();
		}
	}

	// ---- High scores ----

	const string ScoresKey = "highscores";

	public static List<HighScore> HighScores
	{
		get
		{
			string json = PlayerPrefs.GetString(ScoresKey, "");
			var table = string.IsNullOrEmpty(json) ? null : JsonUtility.FromJson<HighScoreTable>(json);
			return table != null ? table.entries : new List<HighScore>();
		}
	}

	public static bool Qualifies(int points)
	{
		if (points <= 0)
			return false;
		List<HighScore> list = HighScores;
		return list.Count < HighScoreCount || points > list[list.Count - 1].points;
	}

	public static void AddHighScore(string initials, int points, string reached)
	{
		List<HighScore> list = HighScores;
		list.Add(new HighScore { initials = initials, points = points, reached = reached });
		list = list.OrderByDescending(h => h.points).Take(HighScoreCount).ToList();
		PlayerPrefs.SetString(ScoresKey, JsonUtility.ToJson(new HighScoreTable { entries = list }));
		PlayerPrefs.Save();
	}

	// ---- Difficulty ----

	/// Enemy toughness grows with the number of heroes and with each chapter already cleared,
	/// so chapters can be played in any order and the run still gets harder.
	public static float EnemyHealthScale
	{
		get
		{
			int players = Mathf.Max(1, JoinedCount);
			float chapter = Current != null ? Current.enemyHealthScale : 1.0f;
			return (1.0f + 0.35f * (players - 1)) * (1.0f + 0.2f * SealCount) * chapter;
		}
	}

	public static float EnemyDamageScale
	{
		get
		{
			float chapter = Current != null ? Current.enemyDamageScale : 1.0f;
			return (1.0f + 0.12f * SealCount) * chapter;
		}
	}

	public static float BossHealthScale
	{
		get { return 1.0f + 0.5f * (Mathf.Max(1, JoinedCount) - 1); }
	}
}
