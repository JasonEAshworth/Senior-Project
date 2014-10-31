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
	public float timeToRespawn = 3.0f;

	public int playerNum = -1;
	public int playerId = -1;

	public bool controllable = true;

	public bool canJump = true;
	public float jumpForce = 7.0f;
	public float verticalVelocity = 0.0f;
	public bool attacking = false;
	public playerClass classType;

	protected GameObject item;
	//public RawImage healthBar;
	public RawImage manaBar;

	public float mana;
	public float maxMana = 100.0f;

	protected bool special = false;
	protected bool normal = false;

	public RoomNode roomIn;

	// controls
	public string moveAxisX;
	public string moveAxisZ;
	public KeyCode jumpKey;
	public KeyCode basicAttackKey;
	public KeyCode useItemKey = KeyCode.Space;
	public KeyCode classAbilityKey;
	public KeyCode specialAttackKey;

	public PlayerManager manager;
	private MapManager mapMan;

	void Start()
	{
		base.Start();
		manager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
		mapMan = GameObject.Find("MapManager").GetComponent<MapManager>();
	}

	protected void Update()
	{
		// Handle respawn timer
		if (dead)
		{

		}
	}

	public override void kill()
	{
		dead = true;
		respawnTimer = timeToRespawn;
	}

	public void respawn()
	{
		transform.position = manager.getRespawnPoint();
		transform.rotation = Quaternion.identity;

		verticalVelocity = 0.0f;

		dead = false;
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

	private void addItem(GameObject p)
	{
		item = p;
	}

	private void addKey()
	{
		manager.haveKey = true;
	}

	public void itemAbility()
	{
		if (item) 
		{

			switch(item.tag)
			{
			case "Potion":
				health += item.GetComponent<Potion>().potionValue;
				Debug.Log(health);
				item = null;
				break;
			case "AttackPotion":
				item = null;
				break;
			case "DefensePotion":
				item = null;
				break;
			}
				

		}
	}

	public IEnumerator Wait(float sec){
		yield return new WaitForSeconds (sec);
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
		if (manaBar.rectTransform.sizeDelta.x > 0)
				return;

		perSec = perSec * Time.deltaTime;
		mana += perSec;
		Mathf.Clamp (mana, 0, 100);
		float amt = perSec / maxMana;

		if (manaBar != null)
		{
			manaBar.rectTransform.sizeDelta = manaBar.rectTransform.sizeDelta + (new Vector2 (322*amt, 0.0f));
			if(manaBar.rectTransform.sizeDelta.x > 0){
				manaBar.rectTransform.sizeDelta = new Vector2(0, manaBar.rectTransform.sizeDelta.y);
			}
		}
	}

	public virtual void basicAttack(string dir){}
	public virtual void specialAttack(){}
	public virtual void classAbility(string dir){}



}
