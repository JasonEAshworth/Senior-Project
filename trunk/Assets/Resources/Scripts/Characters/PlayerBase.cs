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

public class PlayerBase : CharacterBase 
{
	public float respawnTimer = 0.0f;
	public float timeToRespawn = 0.5f;

	public int playerNum = -1;
	public int playerId = -1;

	public bool controllable = true;

	public bool canJump = false;
	public float jumpForce = 15.0f;
	public float verticalVelocity = 0.0f;
	public playerClass classType;

	protected PotionType item;
	//public RawImage healthBar;
	public RawImage manaBar;

	public float mana;
	public float maxMana = 100.0f;

	protected bool special = false;
	protected bool normal = false;

	public float attackSpeed = 1.0f;

	public RoomNode roomIn;

	// controls
/*	public string moveAxisX;
	public string moveAxisZ;
	public KeyCode jumpKey;
	public KeyCode basicAttackKey;
	public KeyCode useItemKey = KeyCode.Space;
	public KeyCode classAbilityKey;
	public KeyCode specialAttackKey;*/

	public PlayerManager manager;
	
	private MapManager mapMan;

	protected override void Start()
	{
		base.Start();
		manager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
		mapMan = GameObject.Find("MapManager").GetComponent<MapManager>();
		jumpForce = 9.0f;
	}

	protected virtual void Update()
	{
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

	public override void takeDamage(float amount)
	{
		// Gives the players only an invuln period after being hit
		if (currentDamageCooldown > 0.0f || dead)
		{
			return;
		}

		if(visibility == 0.0f)
		{
			useMana(20.0f);
		}
		base.takeDamage(amount);
	}

	public override void kill()
	{
		base.kill();
		health = 0.0f;
		dead = true;
		respawnTimer = timeToRespawn;
	}

	public override void respawn()
	{
		transform.position = manager.getRespawnPoint();
		transform.rotation = Quaternion.identity;

		health = maxHealth;

		verticalVelocity = 0.0f;

		dead = false;
		canJump = false;

		base.respawn();
	}

	public void enterRoom(RoomNode room)
	{
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
	}

	public void addItem(PotionType p)
	{
		item = p;

		if (p == PotionType.HEALTH)
		{
			potionImg.enabled = true;
			gameObject.AddComponent<HealthPotion>();
		}
		else if (p == PotionType.HASTE)
		{
			potionImg.enabled = true;
			gameObject.AddComponent<HastePotion>();
		}
	}

	private void addKey()
	{
		manager.haveKey = true;
	}

	public void itemAbility()
	{
		if (item != PotionType.NONE) 
		{
			potionImg.enabled = false;
			SendMessage("usePotion", this);
			item = PotionType.NONE;
		}
	}

	public void useMana(float amt){
	//checks and then subtracts mana from pool
		if (checkForMana (amt))
			mana -= amt;
		else
			return;

		amt = amt / maxMana;
		if (manaBar != null)
		{
			manaBar.rectTransform.sizeDelta = manaBar.rectTransform.sizeDelta - (new Vector2 (322*amt, 0.0f));
		}

	}

	public void addMana(float amt){
		mana += amt;

		if(mana + amt > maxMana)
			mana = maxMana;

		amt = amt / maxMana;
		if (manaBar) {
			manaBar.rectTransform.sizeDelta = manaBar.rectTransform.sizeDelta + (new Vector2 (322 * amt, 0.0f));
			if(manaBar.rectTransform.sizeDelta.x > 0){
				manaBar.rectTransform.sizeDelta = new Vector2(0, manaBar.rectTransform.sizeDelta.y);
			}
		}
	}

	private bool checkForMana(float amt){
	//takes in an amount of mana to check if attack can occur
		if (mana - amt > 0)
			return true;
		else
			return false;
	}

	public void manaRegen(float perSec){
		//mana regeneration function for any players with
		//mana regenerate.
		if (manaBar == null || manaBar.rectTransform.sizeDelta.x > 0)
			return;

		perSec = perSec * Time.deltaTime;
		mana += perSec;
		Mathf.Clamp (mana, 0, maxMana);
		float amt = perSec / maxMana;

		if (manaBar != null)
		{
			manaBar.rectTransform.sizeDelta = manaBar.rectTransform.sizeDelta + (new Vector2 (322*amt, 0.0f));
			if(manaBar.rectTransform.sizeDelta.x > 0){
				manaBar.rectTransform.sizeDelta = new Vector2(0, manaBar.rectTransform.sizeDelta.y);
			}
		}
	}

	public void formMana(int size){
	//when the player starts, either fill his mana bar or not
		Debug.Log ("making the mana bar");
		if(size == 1 && manaBar != null)
			manaBar.rectTransform.sizeDelta = new Vector2 (0, manaBar.rectTransform.sizeDelta.y);
		else if(size == 0 && manaBar != null)
			manaBar.rectTransform.sizeDelta = new Vector2 (-322, manaBar.rectTransform.sizeDelta.y);
	}

	public virtual void basicAttack(string dir){}
	public virtual void specialAttack(){}
	public virtual void classAbility(string dir){}



}
