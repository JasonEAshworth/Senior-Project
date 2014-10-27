using UnityEngine;
using System.Collections;
//using UnityEngine.UI;

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

	public override void kill()
	{
		dead = true;
		respawnTimer = timeToRespawn;
		// temp code for testing
		renderer.material.color = Color.red;
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

	public virtual void basicAttack(string dir){}
	public virtual void specialAttack(){}
	public virtual void classAbility(string dir){}



}
