using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// Runs one chapter in MainScene: its look, music, and enemy variants, the mini-boss swap,
/// continues, game over, and the results screen when the boss falls.
public class ChapterDirector : MonoBehaviour
{
	public static ChapterDirector Instance { get; private set; }

	enum State { Playing, Victory, Results, GameOver }
	State state = State.Playing;

	struct Snapshot { public int points, gold, lives, kills, deaths; }
	readonly Dictionary<int, Snapshot> atStart = new Dictionary<int, Snapshot>();
	readonly Dictionary<int, float> continueDeadline = new Dictionary<int, float>();
	readonly HashSet<int> continueExpired = new HashSet<int>();
	readonly HashSet<RoomNode> roomsEntered = new HashSet<RoomNode>();
	float startTime, clearTime;
	string rank = "";
	bool bossMusicOn;

	public bool BossEngaged { get { return bossMusicOn; } }

	public int RoomsTotal { get { return MapManager.Instance != null ? MapManager.Instance.Rooms.Count : 0; } }
	public int RoomsReached { get { return Mathf.Max(1, roomsEntered.Count); } }

	ChapterDefinition Chapter { get { return GameSession.Current; } }

	public static void Ensure()
	{
		if (Instance != null)
			return;
		var go = new GameObject("ChapterDirector");
		go.AddComponent<ChapterDirector>();
		go.AddComponent<ArcadeHud>();
		go.AddComponent<PauseMenu>();
	}

	void Awake()
	{
		Instance = this;
		startTime = Time.time;
		foreach (PlayerSlot s in GameSession.Joined)
			atStart[s.index] = new Snapshot { points = s.points, gold = s.gold, lives = s.lives, kills = s.kills, deaths = s.deaths };
		ApplyLook();
	}

	void OnDestroy()
	{
		if (Instance == this)
			Instance = null;
	}

	void Start()
	{
		if (Camera.main != null && Camera.main.GetComponent<CameraShake>() == null)
			Camera.main.gameObject.AddComponent<CameraShake>();
		if (Chapter != null)
		{
			MusicPlayer.Play(Chapter.isFinal ? Chapter.bossMusic : Chapter.music);
			bossMusicOn = Chapter.isFinal;
			ArcadeHud.Announce(Chapter.title.ToUpper(), ArcadeUI.Gold);
		}
		if (MapManager.Instance != null && MapManager.Instance.StartRoom != null)
			roomsEntered.Add(MapManager.Instance.StartRoom);
		BuildAtmosphere();
	}

	// ---------------- Look ----------------

	void ApplyLook()
	{
		if (Chapter == null)
			return;
		RenderSettings.ambientLight = Chapter.ambient;
		RenderSettings.fog = Chapter.fogDensity > 0.0f;
		RenderSettings.fogMode = FogMode.ExponentialSquared;
		RenderSettings.fogColor = Chapter.fogColor;
		RenderSettings.fogDensity = Chapter.fogDensity;
	}

	public void OnRoomLoaded(RoomNode room)
	{
		if (room.obj == null)
			return;
		if (Chapter != null)
		{
			foreach (Light l in room.obj.GetComponentsInChildren<Light>(true))
			{
				l.color *= Chapter.lightTint;
				l.intensity *= Chapter.lightIntensity;
			}
			// Torch flames take the chapter's color too: blue in the catacombs, red in the halls.
			foreach (ParticleSystem ps in room.obj.GetComponentsInChildren<ParticleSystem>(true))
			{
				var main = ps.main;
				main.startColor = MultiplyGradient(main.startColor, Chapter.lightTint);
				var col = ps.colorOverLifetime;
				if (col.enabled)
					col.color = MultiplyGradient(col.color, Chapter.lightTint);
			}
		}

		foreach (EnemyBase e in room.obj.GetComponentsInChildren<EnemyBase>(true))
		{
			if (e is KingRestless)
				continue;
			ApplyVariant(e);
		}

		if (Chapter != null && !Chapter.isFinal && room.type == RoomType.BOSS)
		{
			// Boss rooms spawn King Restless from a spawner when the party walks in. In a
			// chapter the spawner is emptied and the chapter's mini-boss stands in its place.
			Vector3 at = room.obj.transform.position;
			foreach (EnemySpawner sp in room.obj.GetComponentsInChildren<EnemySpawner>(true))
			{
				if (!sp.enemiesToSpawn.Any(p => p != null && p.GetComponent<KingRestless>() != null))
					continue;
				at = sp.transform.position;
				sp.enemiesToSpawn = new GameObject[0];
				sp.numberToSpawn = new int[0];
				sp.enemiesRemaining = false;
			}
			KingRestless placed = room.obj.GetComponentInChildren<KingRestless>(true);
			if (placed != null)
			{
				at = placed.transform.position;
				Destroy(placed.gameObject);
			}
			MiniBoss.Create(Chapter.miniBoss, at, room.obj.transform, Chapter);
		}
	}

	static ParticleSystem.MinMaxGradient MultiplyGradient(ParticleSystem.MinMaxGradient g, Color tint)
	{
		switch (g.mode)
		{
			case ParticleSystemGradientMode.Color:
				return new ParticleSystem.MinMaxGradient(g.color * tint);
			case ParticleSystemGradientMode.TwoColors:
				return new ParticleSystem.MinMaxGradient(g.colorMin * tint, g.colorMax * tint);
			case ParticleSystemGradientMode.Gradient:
				return new ParticleSystem.MinMaxGradient(Tint(g.gradient, tint));
			case ParticleSystemGradientMode.TwoGradients:
				return new ParticleSystem.MinMaxGradient(Tint(g.gradientMin, tint), Tint(g.gradientMax, tint));
			default:
				return g;
		}
	}

	static Gradient Tint(Gradient g, Color tint)
	{
		var keys = g.colorKeys;
		for (int i = 0; i < keys.Length; i++)
			keys[i].color *= tint;
		var t = new Gradient();
		t.SetKeys(keys, g.alphaKeys);
		return t;
	}

	void BuildAtmosphere()
	{
		if (Chapter == null || Chapter.atmosphere == "none" || Camera.main == null)
			return;
		var go = new GameObject("Atmosphere");
		go.transform.SetParent(Camera.main.transform, false);
		go.transform.localPosition = new Vector3(0, 0, 14);
		ParticleSystem ps = go.AddComponent<ParticleSystem>();
		ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		var main = ps.main;
		var emission = ps.emission;
		var shape = ps.shape;
		var vel = ps.velocityOverLifetime;
		shape.shapeType = ParticleSystemShapeType.Box;
		shape.scale = new Vector3(30, 18, 14);
		main.simulationSpace = ParticleSystemSimulationSpace.World;
		main.maxParticles = 300;
		vel.enabled = true;
		vel.space = ParticleSystemSimulationSpace.World;
		Color c;
		switch (Chapter.atmosphere)
		{
			case "embers":
				c = new Color(1.0f, 0.55f, 0.15f, 0.9f);
				main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.14f);
				main.startLifetime = 5;
				emission.rateOverTime = 40;
				vel.y = new ParticleSystem.MinMaxCurve(0.6f, 1.6f);
				vel.x = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);
				vel.z = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);
				break;
			case "mist":
				c = new Color(0.7f, 0.8f, 0.9f, 0.08f);
				main.startSize = new ParticleSystem.MinMaxCurve(2.5f, 5.0f);
				main.startLifetime = 9;
				emission.rateOverTime = 12;
				vel.x = new ParticleSystem.MinMaxCurve(-0.25f, 0.25f);
				vel.y = new ParticleSystem.MinMaxCurve(-0.05f, 0.05f);
				vel.z = new ParticleSystem.MinMaxCurve(-0.25f, 0.25f);
				break;
			default:
				c = new Color(0.85f, 0.8f, 0.7f, 0.35f);
				main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.07f);
				main.startLifetime = 8;
				emission.rateOverTime = 30;
				vel.x = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);
				vel.y = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);
				vel.z = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);
				break;
		}
		main.startColor = c;
		main.startSpeed = 0;
		go.GetComponent<ParticleSystemRenderer>().sharedMaterial = Fx.SoftParticle();
		ps.Play();
	}

	// ---------------- Enemies ----------------

	public void OnEnemySpawned(GameObject enemy)
	{
		EnemyBase e = enemy.GetComponent<EnemyBase>();
		if (e != null)
			ApplyVariant(e);
	}

	void ApplyVariant(EnemyBase e)
	{
		if (Chapter == null || e.IsBoss || e is KingRestless || e.GetComponent<VariantBehavior>() != null)
			return;
		EnemyVariant[] options = Chapter.variants.Where(v => Matches(v.replaces, e.enemyType)).ToArray();
		if (options.Length == 0)
			return;
		VariantBehavior.Apply(e, options[Random.Range(0, options.Length)]);
	}

	static bool Matches(enemyArchtype variant, enemyArchtype enemy)
	{
		if (variant == enemy)
			return true;
		// The unarmed zombie is a melee archetype; horde variants dress it too.
		return variant == enemyArchtype.HORDE && enemy == enemyArchtype.MELEE && Random.value < 0.4f;
	}

	public static string PrefabFor(enemyArchtype a)
	{
		switch (a)
		{
			case enemyArchtype.RANGED: return "Prefabs/Enemies/Zombie/Restless-Ranged";
			case enemyArchtype.HORDE: return "Prefabs/Enemies/Zombie/Restless-NoWeapon";
			default: return "Prefabs/Enemies/Zombie/Restless-Melee";
		}
	}

	/// Spawns one enemy of a variant at a point, parented to a room so the room's clear checks see it.
	public static EnemyBase SpawnVariant(EnemyVariant v, Vector3 at, Transform parent)
	{
		GameObject prefab = Resources.Load<GameObject>(PrefabFor(v.replaces));
		if (prefab == null)
			return null;
		GameObject go = Instantiate(prefab, at, Quaternion.Euler(0, Random.Range(0, 360), 0), parent);
		EnemyBase e = go.GetComponent<EnemyBase>();
		VariantBehavior.Apply(e, v);
		return e;
	}

	public void SummonFinalMinions(Vector3 around, int count)
	{
		var pool = GameSession.Chapters.SelectMany(c => c.variants).ToList();
		if (pool.Count == 0)
			return;
		int alive = EnemyBase.All.Count(e => e != null && !e.dead && !e.IsBoss);
		for (int i = 0; i < count && alive + i < 10; i++)
		{
			Vector3 p = around + Quaternion.Euler(0, i * 360.0f / count, 0) * Vector3.forward * 5.0f;
			SpawnVariant(pool[Random.Range(0, pool.Count)], p, transform);
			Fx.Flash(p + Vector3.up, 1.5f, new Color(0.6f, 0.3f, 1.0f, 0.8f));
		}
		ArcadeHud.Announce("THE KING CALLS HIS ARMY!", new Color(0.8f, 0.5f, 1.0f));
	}

	// ---------------- Rooms and the boss ----------------

	public void OnPlayerEnteredRoom(RoomNode room)
	{
		roomsEntered.Add(room);
		if (room.type == RoomType.BOSS && !bossMusicOn && Chapter != null)
		{
			bossMusicOn = true;
			MusicPlayer.Play(Chapter.bossMusic);
			string name = Chapter.isFinal ? "King Restless" : Chapter.miniBoss.name;
			ArcadeHud.Announce(name.ToUpper(), new Color(1.0f, 0.35f, 0.25f));
		}
	}

	public void OnBossDefeated(EnemyBase boss)
	{
		if (state != State.Playing)
			return;
		state = State.Victory;
		clearTime = Time.time - startTime;
		StartCoroutine(VictoryRoutine());
	}

	IEnumerator VictoryRoutine()
	{
		bool final = Chapter != null && Chapter.isFinal;
		ArcadeHud.Announce(final ? "THE KING HAS FALLEN!" : "CHAPTER CLEAR!", ArcadeUI.Gold);
		CameraShake.Add(0.6f);
		MusicPlayer.Play("Music/victory", false);
		Time.timeScale = 0.35f;
		yield return new WaitForSecondsRealtime(1.2f);
		Time.timeScale = 1.0f;
		yield return new WaitForSecondsRealtime(1.8f);

		int deaths = GameSession.Joined.Sum(s => s.deaths - (atStart.ContainsKey(s.index) ? atStart[s.index].deaths : 0));
		rank = deaths == 0 ? (clearTime < 360 ? "S" : "A") : deaths <= 2 ? "B" : deaths <= 5 ? "C" : "D";
		state = State.Results;
		Time.timeScale = 0.0f;
	}

	void FinishChapter()
	{
		Time.timeScale = 1.0f;
		SaveGold();
		int chapterPoints = GameSession.Joined.Sum(s => s.points - (atStart.ContainsKey(s.index) ? atStart[s.index].points : 0));
		if (Chapter != null)
		{
			GameSession.RecordChapterScore(Chapter.id, chapterPoints);
			if (Chapter.isFinal)
			{
				SceneFlow.Load("Ending", "Victory", "The dungeon is quiet at last.");
				return;
			}
			GameSession.AwardSeal(Chapter.sealIndex);
		}
		SceneFlow.Load("Hub", "Seal earned", GameSession.SealCount + " of " + GameSession.Chapters.Count + " seals");
	}

	void SaveGold()
	{
		PlayerManager pm = PlayerManager.Instance;
		if (pm == null)
			return;
		foreach (PlayerSlot s in GameSession.Joined)
		{
			GameObject p = pm.PlayerFor(s.index);
			if (p != null)
				s.gold = p.GetComponent<PlayerBase>().score;
		}
	}

	/// Puts every joined player back to how they started the chapter (for restart and quit).
	public void RestoreSnapshot()
	{
		foreach (PlayerSlot s in GameSession.Joined)
		{
			if (!atStart.TryGetValue(s.index, out Snapshot snap))
				continue;
			s.points = snap.points;
			s.gold = snap.gold;
			s.lives = Mathf.Max(1, snap.lives);
			s.kills = snap.kills;
			s.deaths = snap.deaths;
			s.outOfLives = false;
			s.combo = 0;
		}
	}

	// ---------------- Continues and game over ----------------

	public float ContinueSecondsLeft(PlayerSlot slot)
	{
		return continueDeadline.TryGetValue(slot.index, out float t) ? Mathf.Max(0.0f, t - Time.unscaledTime) : 0.0f;
	}

	void Update()
	{
		if (state == State.Results)
		{
			if (GameSession.Joined.Any(s => PlayerControls.Pressed(s.binding, PlayerAction.Jump) || PlayerControls.Pressed(s.binding, PlayerAction.Attack)) || PlayerControls.AnyConfirm())
				FinishChapter();
			return;
		}
		if (state != State.Playing)
			return;

		foreach (PlayerSlot s in GameSession.Joined)
		{
			if (!s.outOfLives)
			{
				continueExpired.Remove(s.index);
				continue;
			}
			if (continueExpired.Contains(s.index))
				continue;
			if (!continueDeadline.ContainsKey(s.index))
			{
				if (GameSession.Continues <= 0)
				{
					continueExpired.Add(s.index);
					continue;
				}
				continueDeadline[s.index] = Time.unscaledTime + 10.0f;
			}
			if (PlayerControls.Pressed(s.binding, PlayerAction.Jump) || PlayerControls.Pressed(s.binding, PlayerAction.Pause))
			{
				GameSession.Continues--;
				continueDeadline.Remove(s.index);
				GameObject p = PlayerManager.Instance != null ? PlayerManager.Instance.PlayerFor(s.index) : null;
				if (p != null)
					p.GetComponent<PlayerBase>().Continue();
				ArcadeHud.Announce(s.Label + " CONTINUES!", s.Color);
			}
			else if (Time.unscaledTime > continueDeadline[s.index])
			{
				continueDeadline.Remove(s.index);
				continueExpired.Add(s.index);
			}
		}

		if (GameSession.JoinedCount > 0 && GameSession.Joined.All(s => s.outOfLives && continueExpired.Contains(s.index)))
			StartCoroutine(GameOverRoutine());
	}

	IEnumerator GameOverRoutine()
	{
		state = State.GameOver;
		MusicPlayer.Play("Music/gameover", false);
		yield return new WaitForSecondsRealtime(3.0f);
		string reached = Chapter != null ? Chapter.title : "The Dungeon";
		bool done = false;
		InitialsEntry.Begin(GameSession.Joined.ToList(), reached + " (" + GameSession.SealCount + " seals)", () => done = true);
		while (!done)
			yield return null;
		GameSession.NewRun();
		SceneFlow.Load("Hub", "Game Over", "Your seals are safe. Try again.");
	}

	// ---------------- Screens ----------------

	void OnGUI()
	{
		if (state != State.Results && state != State.GameOver)
			return;
		GUI.depth = 0;
		ArcadeUI.Begin();
		float w = ArcadeUI.Width;
		if (state == State.GameOver)
		{
			ArcadeUI.Panel(new Rect(0, 0, w, ArcadeUI.VirtualHeight), new Color(0, 0, 0, 0.6f));
			ArcadeUI.Text(new Rect(0, 250, w, 100), "GAME OVER", 90, new Color(0.9f, 0.2f, 0.15f), TextAnchor.MiddleCenter, true);
			return;
		}

		var r = new Rect(w / 2 - 420, 110, 840, 470);
		ArcadeUI.Panel(r, new Color(0, 0, 0, 0.88f));
		ArcadeUI.Frame(r, ArcadeUI.Gold);
		bool final = Chapter != null && Chapter.isFinal;
		ArcadeUI.Text(new Rect(r.x, r.y + 14, r.width, 50), final ? "THE KING HAS FALLEN" : "CHAPTER CLEAR", 42, ArcadeUI.Gold, TextAnchor.MiddleCenter, true);
		if (Chapter != null)
			ArcadeUI.Text(new Rect(r.x, r.y + 62, r.width, 26), Chapter.title + "   Time " + FormatTime(clearTime), 18, ArcadeUI.Dim, TextAnchor.MiddleCenter);

		string[] heads = { "", "Kills", "Points", "Gold", "Deaths" };
		float[] cols = { 60, 260, 400, 560, 680 };
		for (int c = 0; c < heads.Length; c++)
			ArcadeUI.Text(new Rect(r.x + cols[c], r.y + 110, 140, 24), heads[c], 16, ArcadeUI.Dim);
		int row = 0;
		foreach (PlayerSlot s in GameSession.Joined)
		{
			atStart.TryGetValue(s.index, out Snapshot st);
			float y = r.y + 140 + row * 44;
			GameObject p = PlayerManager.Instance != null ? PlayerManager.Instance.PlayerFor(s.index) : null;
			int gold = p != null ? p.GetComponent<PlayerBase>().score : s.gold;
			ArcadeUI.Text(new Rect(r.x + cols[0], y, 200, 36), s.Label + " " + GameSession.ClassName(s.cls), 20, s.Color, TextAnchor.MiddleLeft, true);
			ArcadeUI.Text(new Rect(r.x + cols[1], y, 140, 36), (s.kills - st.kills).ToString(), 20, Color.white);
			ArcadeUI.Text(new Rect(r.x + cols[2], y, 160, 36), "+" + (s.points - st.points).ToString("N0"), 20, Color.white);
			ArcadeUI.Text(new Rect(r.x + cols[3], y, 140, 36), gold.ToString(), 20, ArcadeUI.Gold);
			ArcadeUI.Text(new Rect(r.x + cols[4], y, 140, 36), (s.deaths - st.deaths).ToString(), 20, Color.white);
			row++;
		}
		ArcadeUI.Text(new Rect(r.x, r.yMax - 130, r.width, 70), "RANK  " + rank, 54, RankColor(rank), TextAnchor.MiddleCenter, true);
		if (ArcadeUI.Blink)
			ArcadeUI.Text(new Rect(r.x, r.yMax - 56, r.width, 30), "Press jump to continue", 20, Color.white, TextAnchor.MiddleCenter);
		if (ArcadeUI.Button(new Rect(r.xMax - 180, r.yMax - 56, 160, 40), "Continue", 18, true))
			FinishChapter();
	}

	static Color RankColor(string r)
	{
		switch (r)
		{
			case "S": return new Color(1.0f, 0.85f, 0.3f);
			case "A": return new Color(0.5f, 1.0f, 0.5f);
			case "B": return new Color(0.5f, 0.8f, 1.0f);
			default: return ArcadeUI.Dim;
		}
	}

	static string FormatTime(float t)
	{
		int m = Mathf.FloorToInt(t / 60), s = Mathf.FloorToInt(t % 60);
		return m + ":" + s.ToString("00");
	}
}
