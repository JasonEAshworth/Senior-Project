using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

/// Spawns one hero per joined slot of the GameSession, lets players drop in and out during a
/// chapter, and tracks the party's center, which the camera and the stay-together rule use.
public class PlayerManager : MonoBehaviour
{
	public static PlayerManager Instance { get; private set; }

	public int numPlayers;          // kept for the scene file; the GameSession decides the party now
	private GameObject[] spawns;
	public List<GameObject> players = new List<GameObject>();
	public List<playerClass> selectedClasses;

	public bool haveKey = false;

	public Woodsman woods;
	public Sorcerer sorc;
	public Warrior war;
	public Rogue rogue;

	public Vector3 playersCenter;

	readonly GameObject[] hud = new GameObject[GameSession.MaxPlayers];
	readonly GameObject[] bySlot = new GameObject[GameSession.MaxPlayers];
	Transform centerMarker;

	void Awake()
	{
		Instance = this;
		// The scene file still holds the 2015 list, which can contain empty entries.
		players = new List<GameObject>();
		for (int i = 0; i < hud.Length; i++)
			hud[i] = GameObject.Find("Player" + (i + 1));
		GameObject c = GameObject.Find("Center");
		centerMarker = c != null ? c.transform : null;
		// The marker's cube was a 2015 debug aid: it showed as a gray box at the party's feet.
		if (c != null)
			foreach (Renderer r in c.GetComponentsInChildren<Renderer>())
				r.enabled = false;
	}

	void Start()
	{
		if (GameSession.JoinedCount == 0)
		{
			// MainScene opened directly in the editor: one player on the keyboard and first gamepad.
			var pads = UnityEngine.InputSystem.Gamepad.all;
			GameSession.Join(new InputBinding { keyboardLayout = 0, gamepadId = pads.Count > 0 ? pads[0].deviceId : -1 });
			GameSession.NewRun();
		}

		for (int i = 0; i < hud.Length; i++)
			SetHud(i, null);
		int spawnIndex = 0;
		foreach (PlayerSlot slot in GameSession.Joined)
		{
			if (!slot.outOfLives)
				SpawnPlayer(slot, SpawnPoint(spawnIndex++));
		}
		updateCenterLocation();
	}

	Vector3 SpawnPoint(int i)
	{
		if (spawns != null && spawns.Length > 0)
			return spawns[i % spawns.Length].transform.position;
		return playersCenter;
	}

	static string PrefabFor(playerClass c)
	{
		switch (c)
		{
			case playerClass.WOODSMAN: return "Prefabs/Character/WoodsMan/Woodsman";
			case playerClass.SORCERER: return "Prefabs/Character/Sorceress/Sorceress";
			case playerClass.ROGUE: return "Prefabs/Character/Rogue/Rogue";
			default: return "Prefabs/Character/Warrior/Warrior";
		}
	}

	/// The name a hero's store model is built under (see HeroSkin).
	public static string HeroName(playerClass c)
	{
		switch (c)
		{
			case playerClass.WOODSMAN: return "Woodsman";
			case playerClass.SORCERER: return "Sorceress";
			case playerClass.ROGUE: return "Rogue";
			default: return "Warrior";
		}
	}

	static void HudTextures(playerClass c, out Texture mana, out Texture profile)
	{
		string m, p;
		switch (c)
		{
			case playerClass.WOODSMAN: m = "gui_energy_woods"; p = "archer_profile"; break;
			case playerClass.SORCERER: m = "gui_energy_bar"; p = "sorc_profile"; break;
			case playerClass.ROGUE: m = "gui_energy_rogue"; p = "rogue_profile"; break;
			default: m = "gui_energy_war"; p = "warrior_profile"; break;
		}
		mana = Resources.Load("Textures/GUI/" + m) as Texture;
		HeroSkinInfo skin = HeroSkin.Find(HeroName(c));
		profile = skin != null && skin.portrait != null ? skin.portrait : Resources.Load("Textures/GUI/" + p) as Texture;
	}

	public GameObject PlayerFor(int slot)
	{
		return bySlot[slot];
	}

	/// Creates the hero for a slot at a position, with the slot's device, color, and upgrades.
	public GameObject SpawnPlayer(PlayerSlot slot, Vector3 position)
	{
		GameObject player = Instantiate(Resources.Load(PrefabFor(slot.cls)), position, Quaternion.identity) as GameObject;
		player.tag = "Player";
		PlayerBase pBase = player.GetComponent<PlayerBase>();
		pBase.playerNum = slot.index;
		pBase.slot = slot;

		PlayerInputControl control = player.AddComponent<PlayerInputControl>();
		control.binding = slot.binding;
		control.speedFactor = 1.0f + 0.08f * slot.speedLevel;
		pBase.maxHealth *= 1.0f + 0.2f * slot.healthLevel;
		pBase.health = pBase.maxHealth;
		pBase.attackMultiplier *= 1.0f + 0.15f * slot.attackLevel;
		pBase.score = slot.gold;
		HeroSkin.Apply(player, HeroName(slot.cls));

		// The Rogue names this child FacingArrow and the other heroes ArrowFacing. The 2015
		// materials draw it as a solid gray square in Unity 6, so it gets an unlit one in the
		// slot color that keeps the texture's transparency.
		Transform arrow = player.transform.Find("FacingArrow") ?? player.transform.Find("ArrowFacing");
		if (arrow != null)
		{
			Material ring = Fx.Unlit(slot.Color);
			ring.mainTexture = Resources.Load<Texture2D>("Textures/Characters/FacingArrowTemp");
			arrow.GetComponent<Renderer>().sharedMaterial = ring;
		}

		// A late joiner starts in the room the rest of the party is in.
		foreach (GameObject other in players)
		{
			RoomNode room = other != null ? other.GetComponent<PlayerBase>().roomIn : null;
			if (room != null)
			{
				pBase.roomIn = room;
				break;
			}
		}
		if (pBase.roomIn == null && MapManager.Instance != null)
			pBase.roomIn = MapManager.Instance.StartRoom;

		players.Add(player);
		bySlot[slot.index] = player;
		SetHud(slot.index, pBase);
		return player;
	}

	public void RemovePlayer(PlayerSlot slot)
	{
		GameObject p = bySlot[slot.index];
		if (p != null)
		{
			players.Remove(p);
			Destroy(p);
		}
		bySlot[slot.index] = null;
		SetHud(slot.index, null);
	}

	void SetHud(int index, PlayerBase pBase)
	{
		GameObject stats = hud[index];
		if (stats == null)
			return;
		CharacterStats cStat = stats.GetComponent<CharacterStats>();
		bool on = pBase != null;
		cStat.player = pBase;
		cStat.healthBar.enabled = on;
		cStat.manaBar.enabled = on;
		cStat.potion.enabled = on;
		cStat.scoreText.enabled = on;
		RawImage prof = stats.transform.Find("Emblem/Character").GetComponent<RawImage>();
		prof.enabled = on;
		if (on)
		{
			HudTextures(pBase.classType, out Texture mana, out Texture profile);
			cStat.manaBar.texture = mana;
			prof.texture = profile;
		}
	}

	/// Players who count for the camera and the stay-together rule: everyone in the scene who
	/// is not down or dead, or, when nobody is up, everyone in the scene.
	public List<GameObject> CameraTargets()
	{
		var up = new List<GameObject>();
		var all = new List<GameObject>();
		foreach (GameObject p in players)
		{
			if (p == null || !p.activeInHierarchy)
				continue;
			all.Add(p);
			if (!p.GetComponent<PlayerBase>().dead)
				up.Add(p);
		}
		return up.Count > 0 ? up : all;
	}

	public void Update()
	{
		updateCenterLocation();

		InputBinding joining = PlayerControls.DetectJoin();
		if (joining != null && Time.timeScale > 0.0f)
		{
			PlayerSlot slot = GameSession.Join(joining);
			if (slot != null)
			{
				Vector3 at = playersCenter + new Vector3(1.0f, 0.5f, 1.0f);
				float best = float.MaxValue;
				if (spawns != null)
					foreach (GameObject s in spawns)
					{
						if (s == null)
							continue;
						float d = (s.transform.position - playersCenter).sqrMagnitude;
						if (d < best) { best = d; at = s.transform.position; }
					}
				SpawnPlayer(slot, at);
				ArcadeHud.Announce(slot.Label + " JOINS!", slot.Color);
			}
		}
	}

	public Vector3 getRespawnPoint()
	{
		if (spawns == null || spawns.Length == 0)
			return playersCenter;
		return spawns[Random.Range(0, spawns.Length)].transform.position;
	}

	public void assignNewSpawnPoints(GameObject[] newSpawns)
	{
		spawns = newSpawns;
	}

	public void respawnAllPlayers()
	{
		for (int i = 0; i < players.Count; i++)
		{
			players[i].transform.position = SpawnPoint(i);
			players[i].GetComponent<PlayerBase>().controllable = true;
		}
	}

	public void updateCenterLocation()
	{
		List<GameObject> targets = CameraTargets();
		if (targets.Count == 0)
			return;
		Vector3 center = Vector3.zero;
		foreach (GameObject p in targets)
			center += p.transform.position;
		playersCenter = center / targets.Count;
		if (centerMarker != null)
			centerMarker.position = playersCenter;
	}
}
