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
			// Game time standing still points at the pause menu or an early results screen instead.
			Check(false, "test stalled (an exception probably ended it; see the errors below). Scene " + SceneManager.GetActiveScene().name +
				", time scale " + Time.timeScale + ", paused " + PauseMenu.Paused + ", last step: " + report.LastOrDefault());
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
		yield return ControlsTest();
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
		if (index == 0)
			foreach (PlayerSlot s in GameSession.Joined)
				Check(RingColor(pm.PlayerFor(s.index)) == s.Color, s.Label + "'s facing ring shows the slot color");
		if (index == 0)
			yield return HeroSkinTest(pm);
		if (index == 1 && pm.players.Count >= 2)
			yield return DownedTest(pm);
		if (ch.variants.Any(v => !string.IsNullOrEmpty(v.skin)))
			yield return SkinTest(ch, pm, rooms);

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
		if (!ch.isFinal && SkinSwap.Exists(ch.miniBoss.skin))
			Check(boss != null && boss.GetComponent<SkinSwap>() != null, ch.id + ": boss wears " + ch.miniBoss.skin);
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

	/// Every skinned variant wears its skin, swings or shoots at a hero, and leaves its skin
	/// behind to collapse when killed.
	IEnumerator SkinTest(ChapterDefinition ch, PlayerManager pm, List<RoomNode> rooms)
	{
		GameObject hero = pm.players.FirstOrDefault(p => p != null && p.activeInHierarchy);
		if (hero == null || rooms.Count == 0 || rooms[0].obj == null)
			yield break;
		foreach (GameObject p in pm.players)
			if (p != null) p.GetComponent<PlayerBase>().shieldUntil = Time.time + 60;
		// Aim ranged spawns into the room, not through a wall into the void.
		Vector3 toCenter = RoomCenter(rooms[0].obj) - hero.transform.position;
		toCenter.y = 0.0f;
		Vector3 dir = toCenter.sqrMagnitude > 1.0f ? toCenter.normalized : Vector3.forward;
		foreach (EnemyVariant v in ch.variants.Where(v => !string.IsNullOrEmpty(v.skin)))
		{
			// Skins come from store packs that a fresh clone does not have.
			if (!SkinSwap.Exists(v.skin))
			{
				Note(v.skin + " is not installed; " + v.name + " stays a zombie");
				continue;
			}
			bool ranged = v.replaces == enemyArchtype.RANGED;
			// A shot left over from the last archer would pass this check by itself.
			foreach (BossProjectile old in UnityEngine.Object.FindObjectsByType<BossProjectile>(FindObjectsSortMode.None))
				Destroy(old.gameObject);
			Vector3 at = hero.transform.position + dir * (ranged ? 8.0f : 1.3f) + Vector3.up * 0.3f;
			EnemyBase e = ChapterDirector.SpawnVariant(v, at, rooms[0].obj.transform);
			SkinSwap skin = e != null ? e.GetComponent<SkinSwap>() : null;
			Animator anim = skin != null && skin.skin != null ? skin.skin.GetComponent<Animator>() : null;
			Check(anim != null && anim.runtimeAnimatorController != null, ch.id + ": " + v.name + " wears " + v.skin);
			if (e == null)
				continue;
			float start = Time.time;
			float until = Time.realtimeSinceStartup + 6.0f;
			bool acted = false;
			while (!acted && Time.realtimeSinceStartup < until && e != null)
			{
				acted = ranged ? UnityEngine.Object.FindAnyObjectByType<BossProjectile>() != null : e.attackStartedTime > start;
				yield return null;
			}
			Check(acted, ch.id + ": " + v.name + (ranged ? " shoots" : " swings") + " at a hero in range");
			yield return CloseUp(ch.id + "_skin_" + v.name.Replace(" ", "_"), e != null ? e.transform : null, hero.transform);
			if (e == null)
				continue;
			GameObject body = skin != null ? skin.skin : null;
			e.takeDamage(e.health + 100000);
			yield return null;
			Check(body != null && body.transform.parent == null, ch.id + ": " + v.name + " collapses when killed");
		}
	}

	/// Player 2 comes back as the Woodsman, then as the Rogue, and stays the Rogue for the rest of
	/// the run. Each checks the store model that replaces the 2015 mesh: bound to the hero's
	/// Animator, standing on the floor, animating, attacking, and shown in the HUD portrait.
	IEnumerator HeroSkinTest(PlayerManager pm)
	{
		PlayerSlot slot = GameSession.Joined.FirstOrDefault(s => s.index == 1);
		if (slot == null)
			yield break;
		foreach (playerClass c in new[] { playerClass.WOODSMAN, playerClass.ROGUE })
		{
			string name = PlayerManager.HeroName(c);
			if (HeroSkin.Find(name) == null)
			{
				Note(name + " model is not installed; the hero keeps its 2015 mesh");
				continue;
			}
			pm.RemovePlayer(slot);
			slot.cls = c;
			GameObject hero = pm.SpawnPlayer(slot, pm.playersCenter + Vector3.right);
			hero.GetComponent<PlayerBase>().shieldUntil = Time.time + 30;
			// Facing the close-up camera, which looks from the south.
			hero.transform.rotation = Quaternion.Euler(0, 180, 0);
			yield return null;
			Animator anim = hero.GetComponent<Animator>();
			Transform hips = anim.GetBoneTransform(HumanBodyBones.Hips);
			Transform hand = anim.GetBoneTransform(HumanBodyBones.RightHand);
			Check(hips != null && hand != null && hips.IsChildOf(hero.transform), name + " wears the new model");
			GameObject old = hero.GetComponent<PlayerBase>().characterMesh;
			Check(old == null || !old.GetComponent<Renderer>().enabled, name + "'s 2015 mesh is hidden");
			if (hips == null || hand == null)
				continue;
			float hipHeight = hips.position.y - hero.transform.position.y;
			Check(hipHeight > 0.6f && hipHeight < 1.3f, name + " stands on the floor (hips " + hipHeight.ToString("0.00") + " m up)");
			Vector3 before = hero.transform.InverseTransformPoint(hand.position);
			yield return new WaitForSeconds(0.6f);
			float moved = Vector3.Distance(before, hero.transform.InverseTransformPoint(hand.position));
			Check(moved > 0.003f, name + "'s model animates (hand moved " + moved.ToString("0.000") + " m)");
			anim.SetTrigger("Attack");
			yield return new WaitForSeconds(0.3f);
			string clip = anim.GetCurrentAnimatorClipInfo(0).Concat(anim.GetNextAnimatorClipInfo(0)).Select(i => i.clip.name).FirstOrDefault(n => n.Contains("Attack") || n.Contains("Shoot"));
			Check(clip != null, name + " plays an attack clip on the new model (" + (clip ?? "none") + ")");
			GameObject stats = GameObject.Find("Player" + (slot.index + 1));
			Texture shown = stats != null ? stats.transform.Find("Emblem/Character").GetComponent<UnityEngine.UI.RawImage>().texture : null;
			Check(shown != null && shown == HeroSkin.Find(name).portrait, name + "'s HUD shows the new portrait");
			yield return CloseUp(name.ToLower() + "_model", hero.transform, hero.transform);
		}
	}

	/// Four frames of a skin in action with the camera pulled in, to check that it animates, faces
	/// its target, and holds its gear. The follow camera is paused for the burst.
	IEnumerator CloseUp(string name, Transform subject, Transform hero)
	{
		Camera cam = Camera.main;
		if (cam == null || subject == null || hero == null)
		{
			yield return Shot(name);
			yield break;
		}
		cameraControl follow = cam.GetComponent<cameraControl>();
		bool following = follow != null && follow.enabled;
		if (follow != null)
			follow.enabled = false;
		Vector3 pos = cam.transform.position;
		Quaternion rot = cam.transform.rotation;
		for (int i = 0; i < 4 && subject != null; i++)
		{
			Vector3 mid = (subject.position + hero.position) * 0.5f;
			cam.transform.position = mid + new Vector3(0.0f, 3.5f, -4.5f);
			cam.transform.LookAt(mid + Vector3.up);
			yield return Shot(name + "_" + i);
			yield return new WaitForSeconds(0.2f);
		}
		cam.transform.SetPositionAndRotation(pos, rot);
		if (follow != null)
			follow.enabled = following;
	}

	static Vector3 RoomCenter(GameObject room)
	{
		Renderer[] rs = room.GetComponentsInChildren<Renderer>();
		if (rs.Length == 0)
			return room.transform.position;
		Bounds b = rs[0].bounds;
		foreach (Renderer r in rs)
			b.Encapsulate(r.bounds);
		return b.center;
	}

	/// Rebinding swaps keys instead of doubling them up, saves, resets, and the screen opens.
	IEnumerator ControlsTest()
	{
		PlayerControls.ResetToDefaults();
		string space = PlayerControls.KeysName(0, BindSlot.Jump);
		PlayerControls.BindKey(0, BindSlot.Jump, UnityEngine.InputSystem.Key.J);
		Check(PlayerControls.KeysName(0, BindSlot.Jump) == PlayerControls.KeyName(UnityEngine.InputSystem.Key.J)
			&& PlayerControls.KeysName(0, BindSlot.Attack) == space, "rebinding jump to J moves attack to the old jump key");
		PlayerControls.BindKey(0, BindSlot.Pause, UnityEngine.InputSystem.Key.Numpad0);
		Check(PlayerControls.KeysName(1, BindSlot.Jump) == PlayerControls.KeyName(UnityEngine.InputSystem.Key.RightShift),
			"a key taken by the other layout leaves that layout's other keys alone");
		PlayerControls.BindPad(PlayerAction.Attack, UnityEngine.InputSystem.LowLevel.GamepadButton.South);
		Check(PlayerControls.PadName(PlayerAction.Attack) == "A" && PlayerControls.PadName(PlayerAction.Jump) == "X",
			"rebinding a gamepad button swaps it with the action that had it");
		PlayerControls.ResetToDefaults();
		Check(PlayerControls.KeysName(0, BindSlot.Jump) == space && PlayerControls.PadName(PlayerAction.Jump) == "A",
			"reset restores the default controls");
		ControlsMenu.Show();
		yield return null;
		Check(ControlsMenu.IsOpen, "the controls screen opens");
		yield return Shot("01_controls");
		Destroy(FindAnyObjectByType<ControlsMenu>().gameObject);
		yield return null;
	}

	static Color RingColor(GameObject hero)
	{
		if (hero == null)
			return Color.clear;
		Transform ring = hero.transform.Find("FacingArrow") ?? hero.transform.Find("ArrowFacing");
		return ring != null ? ring.GetComponent<Renderer>().sharedMaterial.color : Color.clear;
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
