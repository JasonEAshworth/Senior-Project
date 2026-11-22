using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Plays the whole run without a person, for checking a build: run a desktop build with
/// -smoketest <folder>. It joins players, clears every chapter (walking room to room and
/// fighting each boss for a while), beats the king, reaches the ending, forces a game over,
/// and writes a report with every check, every error, and screenshots.
public class SmokeTest : MonoBehaviour
{
	string folder;
	readonly List<string> report = new List<string>();
	readonly List<string> errors = new List<string>();
	int failures;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	static void Install()
	{
		if (Application.isEditor)
			return;
		string[] args = Environment.GetCommandLineArgs();
		int i = Array.IndexOf(args, "-smoketest");
		if (i < 0)
			return;
		var go = new GameObject("SmokeTest");
		DontDestroyOnLoad(go);
		SmokeTest t = go.AddComponent<SmokeTest>();
		t.folder = i + 1 < args.Length ? args[i + 1] : Application.persistentDataPath;
		Directory.CreateDirectory(t.folder);
		Application.logMessageReceived += t.OnLog;
	}

	void OnLog(string message, string stack, LogType type)
	{
		if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert)
		{
			string line = type + ": " + message + " | " + (stack ?? "").Split('\n').FirstOrDefault();
			if (!errors.Contains(line))
				errors.Add(line);
		}
	}

	float lastProgress;
	bool finished;

	void Check(bool ok, string what)
	{
		report.Add((ok ? "PASS " : "FAIL ") + what);
		if (!ok) failures++;
		lastProgress = Time.realtimeSinceStartup;
	}

	void Note(string what) { report.Add("     " + what); lastProgress = Time.realtimeSinceStartup; }

	void Update()
	{
		// An exception ends the test coroutine; report what was done instead of hanging.
		if (!finished && lastProgress > 0 && Time.realtimeSinceStartup - lastProgress > 120)
		{
			Check(false, "test stalled (an exception probably ended it; see the errors below)");
			Finish();
		}
	}

	IEnumerator Shot(string name)
	{
		yield return new WaitForEndOfFrame();
		ScreenCapture.CaptureScreenshot(Path.Combine(folder, name + ".png"));
		yield return null;
	}

	IEnumerator WaitScene(string scene, float timeout = 30)
	{
		float until = Time.realtimeSinceStartup + timeout;
		while ((SceneManager.GetActiveScene().name != scene || SceneFlow.Busy) && Time.realtimeSinceStartup < until)
			yield return null;
		yield return new WaitForSecondsRealtime(0.5f);
		Check(SceneManager.GetActiveScene().name == scene, "reached scene " + scene);
	}

	IEnumerator Start()
	{
		yield return WaitScene("Title");
		yield return new WaitForSecondsRealtime(1.0f);
		yield return Shot("01_title");
		GameSession.ResetSeals();
		Check(GameSession.Join(new InputBinding { keyboardLayout = 0 }) != null, "player 1 joins on the keyboard");
		GameSession.NewRun();
		SceneFlow.Load("Hub");
		yield return WaitScene("Hub");
		yield return Shot("02_hub");
		Check(GameSession.Chapters.Count == 4, "four chapters load (found " + GameSession.Chapters.Count + ")");
		Check(GameSession.FinalChapter != null, "the final chapter loads");
		Check(!GameSession.FinalUnlocked, "the throne room starts locked");

		for (int i = 0; i < GameSession.Chapters.Count; i++)
			yield return PlayChapter(GameSession.Chapters[i], i);

		Check(GameSession.FinalUnlocked, "four seals unlock the throne room");
		yield return PlayChapter(GameSession.FinalChapter, 9);
		yield return WaitScene("Ending");
		yield return new WaitForSecondsRealtime(1.0f);
		yield return Shot("90_ending");

		yield return GameOverTest();
		Finish();
	}

	IEnumerator PlayChapter(ChapterDefinition ch, int index)
	{
		Note("---- " + ch.title + " ----");
		GameSession.Current = ch;
		GameSession.ReviveAllForChapter();
		SceneFlow.Load("MainScene", ch.title, ch.tagline);
		yield return WaitScene("MainScene");
		yield return new WaitForSecondsRealtime(1.5f);

		MapManager map = MapManager.Instance;
		PlayerManager pm = PlayerManager.Instance;
		Check(map != null && pm != null && ChapterDirector.Instance != null, ch.id + ": managers exist");
		if (map == null || pm == null)
			yield break;
		List<RoomNode> rooms = map.Rooms;
		Note("rooms: " + string.Join(", ", rooms.Select(r => r.name)));
		Check(rooms.Count == (ch.isFinal ? 1 : ch.numRooms), ch.id + ": dungeon has " + rooms.Count + " rooms (want " + (ch.isFinal ? 1 : ch.numRooms) + ")");
		Check(rooms.Count > 0 && rooms[rooms.Count - 1].type == RoomType.BOSS, ch.id + ": last room is a boss room");
		Check(pm.players.Count == GameSession.JoinedCount, ch.id + ": one hero per joined player (" + pm.players.Count + ")");
		yield return Shot(index + "_a_start");

		if (index == 0)
		{
			// Drop-in: a second player joins mid-chapter on the arrow-key layout.
			PlayerSlot p2 = GameSession.Join(new InputBinding { keyboardLayout = 1 });
			Check(p2 != null, "player 2 can join mid-chapter");
			if (p2 != null)
			{
				pm.SpawnPlayer(p2, pm.playersCenter + Vector3.right);
				yield return new WaitForSecondsRealtime(0.5f);
				Check(pm.players.Count == 2, "player 2's hero spawns (" + pm.players.Count + " heroes)");
				Check(pm.PlayerFor(1) != null && pm.PlayerFor(1).GetComponent<PlayerBase>().slot == p2, "player 2's hero has its slot");
			}
		}
		if (index == 1 && pm.players.Count >= 2)
			yield return DownedTest(pm);

		// Walk the party room to room, entering each the way a doorway would.
		for (int r = 1; r < rooms.Count; r++)
		{
			RoomNode room = rooms[r];
			if (room.obj == null)
			{
				Check(false, ch.id + ": room " + r + " (" + room.name + ") was never built");
				continue;
			}
			room.obj.SetActive(true);
			Vector3 at = room.playerRespawns.Count > 0 ? room.playerRespawns[0].transform.position : room.obj.transform.position + Vector3.up;
			int k = 0;
			foreach (GameObject p in pm.players.ToList())
			{
				if (p == null || !p.activeInHierarchy)
					continue;
				Teleport(p, at + new Vector3(k++ * 1.2f, 0.5f, 0));
				p.GetComponent<PlayerBase>().enterRoom(room);
			}
			yield return new WaitForSecondsRealtime(r == rooms.Count - 1 ? 1.0f : 0.6f);
		}

		EnemyBase boss = EnemyBase.All.FirstOrDefault(e => e != null && e.IsBoss && !e.dead);
		string want = ch.isFinal ? "King Restless" : ch.miniBoss.name;
		Check(boss != null && boss.bossName == want, ch.id + ": boss is " + (boss != null ? boss.bossName : "missing") + " (want " + want + ")");
		// Let the fight run so the boss uses its special attacks against the party.
		foreach (GameObject p in pm.players)
			if (p != null) p.GetComponent<PlayerBase>().shieldUntil = Time.time + 30;
		yield return new WaitForSeconds(4.0f);
		int bosses = EnemyBase.All.Count(e => e != null && e.IsBoss && !e.dead);
		Check(bosses == 1, ch.id + ": exactly one boss in the fight (" + bosses + ")");
		yield return Shot(index + "_b_boss");
		yield return new WaitForSeconds(6.0f);
		yield return Shot(index + "_c_boss");
		if (boss == null)
			yield break;

		int before = GameSession.Joined.Sum(s => s.points);
		boss.takeDamage(boss.health + 100000);
		float until = Time.realtimeSinceStartup + 10;
		while (Time.timeScale != 0.0f && Time.realtimeSinceStartup < until)
			yield return null;
		Check(Time.timeScale == 0.0f, ch.id + ": results screen opens after the boss dies");
		Check(GameSession.Joined.Sum(s => s.points) > before, ch.id + ": the boss kill scores points");
		yield return Shot(index + "_d_results");
		typeof(ChapterDirector).GetMethod("FinishChapter", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(ChapterDirector.Instance, null);
		if (!ch.isFinal)
		{
			yield return WaitScene("Hub");
			Check(GameSession.HasSeal(ch.sealIndex), ch.id + ": seal awarded");
		}
	}

	IEnumerator DownedTest(PlayerManager pm)
	{
		PlayerBase a = pm.PlayerFor(0).GetComponent<PlayerBase>();
		PlayerBase b = pm.PlayerFor(1).GetComponent<PlayerBase>();
		int lives = b.slot.lives;
		b.shieldUntil = -1;
		b.takeDamage(b.health + 1000);
		yield return null;
		Check(b.downed, "a hero at zero health goes down while a teammate stands");
		Teleport(a.gameObject, b.transform.position + Vector3.right);
		yield return new WaitForSeconds(PlayerBase.ReviveSeconds + 0.6f);
		Check(!b.downed && !b.dead, "standing close revives a downed teammate");
		Check(b.slot.lives == lives, "a revive costs no life");
		b.shieldUntil = -1;
		b.takeDamage(b.health + 1000);
		Teleport(a.gameObject, b.transform.position + Vector3.right * 20);
		b.downedTimer = 0.05f;
		yield return new WaitForSeconds(0.3f);
		Check(b.slot.lives == lives - 1, "an unrevived hero loses a life");
		yield return new WaitForSeconds(b.timeToRespawn + 0.5f);
		Check(!b.dead, "the hero respawns after losing a life");
	}

	IEnumerator GameOverTest()
	{
		Note("---- game over ----");
		GameSession.NewRun();
		GameSession.Continues = 0;
		ChapterDefinition ch = GameSession.Chapters[0];
		GameSession.Current = ch;
		SceneFlow.Load("MainScene", ch.title, ch.tagline);
		yield return WaitScene("MainScene");
		yield return new WaitForSecondsRealtime(1.5f);
		PlayerManager pm = PlayerManager.Instance;
		foreach (PlayerSlot s in GameSession.Joined)
			s.points = 4321 + s.index * 100;   // a score worth entering
		for (int round = 0; round < 12 && GameSession.Joined.Any(s => !s.outOfLives); round++)
		{
			foreach (GameObject p in pm.players.ToList())
			{
				if (p == null || !p.activeInHierarchy)
					continue;
				PlayerBase pb = p.GetComponent<PlayerBase>();
				pb.shieldUntil = -1;
				pb.downedTimer = 0.05f;
				if (!pb.dead)
					pb.takeDamage(pb.health + 1000);
			}
			yield return new WaitForSeconds(2.6f);
		}
		Check(GameSession.Joined.All(s => s.outOfLives), "every hero runs out of lives");
		float until = Time.realtimeSinceStartup + 10;
		while (FindAnyObjectByType<InitialsEntry>() == null && SceneManager.GetActiveScene().name == "MainScene" && Time.realtimeSinceStartup < until)
			yield return null;
		yield return Shot("95_gameover");
		InitialsEntry entry = FindAnyObjectByType<InitialsEntry>();
		Note(entry != null ? "initials entry opened" : "no initials entry (no qualifying score)");
		while (entry != null)
		{
			typeof(InitialsEntry).GetMethod("Confirm", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(entry, null);
			yield return null;
			entry = FindAnyObjectByType<InitialsEntry>();
		}
		yield return WaitScene("Hub");
		Check(GameSession.SealCount == GameSession.Chapters.Count, "seals survive a game over (" + GameSession.SealCount + ")");
		Check(GameSession.HighScores.Count > 0, "high scores were saved (" + GameSession.HighScores.Count + ")");
	}

	static void Teleport(GameObject p, Vector3 at)
	{
		CharacterController cc = p.GetComponent<CharacterController>();
		if (cc != null) cc.enabled = false;
		p.transform.position = at;
		if (cc != null) cc.enabled = true;
	}

	void Finish()
	{
		if (finished)
			return;
		finished = true;
		report.Add("");
		report.Add(failures == 0 ? "RESULT: ALL CHECKS PASSED" : "RESULT: " + failures + " CHECK(S) FAILED");
		report.Add("errors and exceptions: " + errors.Count);
		report.AddRange(errors.Take(60).Select(e => "  " + e));
		File.WriteAllLines(Path.Combine(folder, "report.txt"), report);
		Application.Quit(failures == 0 ? 0 : 1);
	}
}
