using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum playerClass
{
	WARRIOR,
	SORCERER,
	ROGUE,
	WOODSMAN
};

public enum PowerUpType { Fury, Haste, Shield, Heal }

public class PlayerBase : CharacterBase
{
	public float respawnTimer = 0.0f;
	public float timeToRespawn = 2.0f;

	public int playerNum = -1;
	public int playerId = -1;
	public PlayerSlot slot;

	public bool controllable = true;

	public bool canJump = false;
	public float jumpForce = 15.0f;
	public float verticalVelocity = 0.0f;
	public playerClass classType;

	public PotionType item;

	public float mana = 100.0f;
	public float maxMana = 100.0f;

	public int score = 0;			// gold carried; the arcade score lives in slot.points

	protected bool special = false;
	protected bool normal = false;

	public float attackSpeed = 1.0f;

	public RoomNode roomIn;

	// Downed: with a teammate still standing, a hero at zero health goes down instead of dying.
	// A teammate who stands close for ReviveSeconds brings them back; otherwise they lose a life.
	public const float DownedSeconds = 8.0f;
	public const float ReviveSeconds = 2.0f;
	public const float ReviveRange = 2.5f;
	public bool downed = false;
	public float downedTimer = 0.0f;
	public float reviveProgress = 0.0f;

	public float shieldUntil = -1.0f, furyUntil = -1.0f, hasteUntil = -1.0f;
	bool furyActive, hasteActive;

	float damageThisRoom = 0.0f;
	int killsAtRoomStart = 0;

	public PlayerManager manager;

	private MapManager mapMan;

	protected override void Start()
	{
		base.Start();
		manager = PlayerManager.Instance;
		mapMan = MapManager.Instance;
		jumpForce = 9.0f;
		timeToRespawn = 2.0f;
		shieldUntil = Time.time + 1.5f;
		if (slot != null)
			killsAtRoomStart = slot.kills;
	}

	public bool Shielded { get { return Time.time < shieldUntil; } }

	protected virtual void Update()
	{
		UpdatePowerUps();

		if (downed)
		{
			UpdateDowned();
			return;
		}
		// Handle respawn timer
		if (dead)
		{
			respawnTimer -= Time.deltaTime;
			if (respawnTimer <= 0.0f)
			{
				respawn();
			}
		}
	}

	void UpdateDowned()
	{
		downedTimer -= Time.deltaTime;
		bool helped = false;
		foreach (GameObject p in manager.players)
		{
			if (p == null || p == gameObject || !p.activeInHierarchy)
				continue;
			PlayerBase other = p.GetComponent<PlayerBase>();
			if (!other.dead && (p.transform.position - transform.position).sqrMagnitude < ReviveRange * ReviveRange)
				helped = true;
		}
		reviveProgress = helped ? reviveProgress + Time.deltaTime : Mathf.Max(0.0f, reviveProgress - Time.deltaTime * 0.5f);
		if (reviveProgress >= ReviveSeconds)
		{
			downed = false;
			dead = false;
			health = maxHealth * 0.5f;
			shieldUntil = Time.time + 1.5f;
			ArcadeHud.Announce(slot != null ? slot.Label + " IS BACK!" : "REVIVED!", slot != null ? slot.Color : Color.white);
		}
		else if (downedTimer <= 0.0f || !AnyTeammateUp())
		{
			LoseLife();
		}
	}

	bool AnyTeammateUp()
	{
		foreach (GameObject p in manager.players)
		{
			if (p != null && p != gameObject && p.activeInHierarchy && !p.GetComponent<PlayerBase>().dead)
				return true;
		}
		return false;
	}

	public override void takeDamage(float amount)
	{
		// Gives the players only an invuln period after being hit
		if (currentDamageCooldown > 0.0f || dead || Shielded)
		{
			return;
		}

		base.takeDamage(amount);
		damageThisRoom += amount;
		if (slot != null)
			slot.combo = 0;
		CameraShake.Add(0.25f);
		ArcadeHud.Damage(transform.position, amount, true);
	}

	public override void kill()
	{
		if (dead)
			return;
		health = 0.0f;
		if (slot != null && AnyTeammateUp())
		{
			dead = true;
			downed = true;
			downedTimer = DownedSeconds;
			reviveProgress = 0.0f;
			ArcadeHud.Announce(slot.Label + " IS DOWN!", slot.Color);
			return;
		}
		LoseLife();
	}

	void LoseLife()
	{
		downed = false;
		score -= 25;
		if (score < 0)
		{
			score = 0;
		}
		base.kill();
		respawnTimer = timeToRespawn;
		if (slot == null)
			return;
		slot.deaths++;
		slot.lives--;
		slot.combo = 0;
		if (slot.lives <= 0)
		{
			slot.outOfLives = true;
			slot.gold = score;
			ArcadeHud.Announce(slot.Label + " IS OUT!", slot.Color);
			gameObject.SetActive(false);
		}
	}

	/// Spends a continue: full lives, back in at a respawn point.
	public void Continue()
	{
		slot.lives = GameSession.StartingLives;
		slot.outOfLives = false;
		gameObject.SetActive(true);
		respawn();
	}

	public override void respawn()
	{
		transform.position = manager.getRespawnPoint();
		transform.rotation = Quaternion.identity;

		health = maxHealth;

		verticalVelocity = 0.0f;

		dead = false;
		downed = false;
		canJump = false;
		shieldUntil = Time.time + 2.0f;

		base.respawn();
	}

	public void enterRoom(RoomNode room)
	{
		if (room != roomIn && roomIn != null && slot != null)
		{
			// Clearing a room's enemies without a scratch earns a bonus.
			if (slot.kills > killsAtRoomStart && damageThisRoom <= 0.0f)
			{
				int bonus = 300;
				slot.points += bonus;
				ArcadeHud.Popup(transform.position, "FLAWLESS +" + bonus, slot.Color);
			}
			killsAtRoomStart = slot.kills;
			damageThisRoom = 0.0f;
		}

		roomIn = room;
		mapMan.notifySpawners(room);
		mapMan.loadNeighbors(room);
		mapMan.unloadEmptyRooms();
		mapMan.updateRespawnPoints(room);

		HordeRoom h = room.obj.GetComponent<HordeRoom>();
		if (h != null)
		{
			h.startSpawning();
		}
		if (ChapterDirector.Instance != null)
			ChapterDirector.Instance.OnPlayerEnteredRoom(room);
	}

	public void addItem(PotionType p)
	{
		item = p;

		if (p == PotionType.HEALTH)
		{
			gameObject.AddComponent<HealthPotion> ();
		}
		else if (p == PotionType.HASTE)
		{
			gameObject.AddComponent<HastePotion> ();
		}
		else if (p == PotionType.ATTACK)
		{
			gameObject.AddComponent<AttackUpPotion> ();
		}
	}

	public void addScore(GameObject p)
	{
		int gold = 0;
		if (p.tag == "Coin")
		{
			gold = 1;
		}
		if (p.tag == "Gold")
		{
			gold = 10;
		}
		score += gold;
		if (slot != null && gold > 0)
			slot.points += gold * 10;
	}

	private void addKey()
	{
		manager.haveKey = true;
		ArcadeHud.Announce("KEY FOUND!", ArcadeUI.Gold);
	}

	public void itemAbility()
	{
		if (item != PotionType.NONE)
		{
			SendMessage("usePotion", this);
			item = PotionType.NONE;
		}
	}

	public void ApplyPowerUp(PowerUpType type)
	{
		switch (type)
		{
			case PowerUpType.Fury: furyUntil = Time.time + 10.0f; break;
			case PowerUpType.Haste: hasteUntil = Time.time + 10.0f; break;
			case PowerUpType.Shield: shieldUntil = Time.time + 6.0f; break;
			case PowerUpType.Heal: health = Mathf.Min(maxHealth, health + maxHealth * 0.4f); break;
		}
		UpdatePowerUps();
	}

	void UpdatePowerUps()
	{
		// Multiply on start and divide on end, so these stack with potions and shop upgrades.
		bool fury = Time.time < furyUntil;
		if (fury != furyActive)
		{
			attackMultiplier = fury ? attackMultiplier * 2.0f : attackMultiplier / 2.0f;
			furyActive = fury;
		}
		bool haste = Time.time < hasteUntil;
		if (haste != hasteActive)
		{
			PlayerInputControl control = GetComponent<PlayerInputControl>();
			if (control != null)
				control.speedFactor = haste ? control.speedFactor * 1.5f : control.speedFactor / 1.5f;
			hasteActive = haste;
		}
	}

	public void useMana(float amt)
	{
		mana -= amt;
		mana = Mathf.Clamp(mana, 0, maxMana);
	}

	public void addMana(float amt)
	{
		mana += amt;
		mana = Mathf.Clamp(mana, 0, maxMana);
	}

	public bool checkForMana(float amt)
	{
		//takes in an amount of mana to check if attack can occur
		if(mana - amt >= 0)
			return true;
		else
			return false;
	}

	public void manaRegen(float perSec)
	{
		//mana regeneration function for any players with mana regenerate.
		mana += perSec * Time.deltaTime;
		mana = Mathf.Clamp(mana, 0, maxMana);
	}

	public virtual void basicAttack(string dir){}
	public virtual void specialAttack(){}
	public virtual void classAbility(string dir){}
}
