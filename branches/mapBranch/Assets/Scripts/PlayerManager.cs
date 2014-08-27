using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour 
{
	// init variables
	public int numPlayers;
	private GameObject[] spawns;
	public List<GameObject> players;
	public List<playerClass> selectedClasses;
	public List<GameObject> teamInventory;

	void Awake() 
	{
		// instantiate players list
		players = new List<GameObject>();

		// instantiate inventory list
		teamInventory = new List<GameObject>();

		// instantiate selectedClasses and populate it
		selectedClasses = new List<playerClass>(4);
		selectedClasses.Add(playerClass.ADVENTURER);
		selectedClasses.Add(playerClass.ROGUE);
		selectedClasses.Add(playerClass.SORCERER);
		selectedClasses.Add(playerClass.WARRIOR);

		// set players to 4 at start
		numPlayers = 4;

		// acquire all spawn points for players 
		spawns = GameObject.FindGameObjectsWithTag("Respawn");

		for (int i=0; i<numPlayers; i++) 
		{
			switch (selectedClasses[i])
			{
			case playerClass.ADVENTURER:
				GameObject player1 = Instantiate (Resources.Load("Prefabs/Adventurer"),spawns[i].transform.position,Quaternion.identity) as GameObject;
				player1.tag = "Player";
				Adventurer adv = player1.GetComponent<Adventurer>();
				adv.moveAxisX = "Horizontal1";
				adv.moveAxisZ = "Vertical1";
				adv.jumpKey = KeyCode.C;
				adv.playerNum = i;
				players.Add (player1);
				break;
			case playerClass.SORCERER:
				GameObject player2 = Instantiate (Resources.Load("Prefabs/Sorceress"),spawns[i].transform.position,Quaternion.identity) as GameObject;
				player2.tag = "Player";
				Sorcerer sorc = player2.GetComponent<Sorcerer>();
				sorc.moveAxisX = "Horizontal2";
				sorc.moveAxisZ = "Vertical2";
				sorc.jumpKey = KeyCode.N;
				sorc.basicAttackKey = KeyCode.U;
				sorc.playerNum = i;
				players.Add (player2);
				break;

			case playerClass.ROGUE:
				GameObject player3 = Instantiate (Resources.Load("Prefabs/Rogue"),spawns[i].transform.position,Quaternion.identity) as GameObject;
				player3.tag = "Player";
				Rogue rogue = player3.GetComponent<Rogue>();
				rogue.moveAxisX = "Horizontal1";
				rogue.moveAxisZ = "Vertical1";
				rogue.jumpKey = KeyCode.C;
				rogue.playerNum = i;
				players.Add (player3);
				break;
			
			case playerClass.WARRIOR:
				GameObject player4 = Instantiate (Resources.Load("Prefabs/Warrior"),spawns[i].transform.position,Quaternion.identity) as GameObject;
				player4.tag = "Player";
				Warrior war = player4.GetComponent<Warrior>();
				war.moveAxisX = "Horizontal2";
				war.moveAxisZ = "Vertical2";
				war.jumpKey = KeyCode.N;
				war.basicAttackKey = KeyCode.E;
				war.playerNum = i;
				players.Add (player4);
				break;
			}
		}
	}

	public Vector3 getRespawnPoint()
	{
		int randSpawn = Random.Range (0,4);
		return spawns[randSpawn].transform.position;
	}

	public void addItem(GameObject item)
	{
		if (teamInventory.Count > 4) 
		{
			return;
		}
		teamInventory.Add(item);
	}

	public void delItem(GameObject item)
	{
		if (teamInventory.Count == 0) 
		{
			return;
		}
		teamInventory.Remove(item);
	}
}
