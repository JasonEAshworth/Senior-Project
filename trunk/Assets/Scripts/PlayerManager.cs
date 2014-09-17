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

	public bool haveKey = false;

	void Start() 
	{
		// instantiate players list
		players = new List<GameObject>();

		// instantiate inventory list
		//teamInventory = new List<GameObject>();

		// instantiate selectedClasses and populate it
		selectedClasses = new List<playerClass>();
		selectedClasses.Add(playerClass.WOODSMAN);
		selectedClasses.Add(playerClass.ROGUE);
		selectedClasses.Add(playerClass.SORCERER);
		selectedClasses.Add(playerClass.WARRIOR);

		// set players to 4 at start
		numPlayers = 4;

		// acquire all spawn points for players 
		getNewSpawnPoints();

		for (int i=0; i<numPlayers; i++) 
		{
			switch (selectedClasses[i])
			{
			case playerClass.WOODSMAN:
				GameObject player1 = Instantiate (Resources.Load("Prefabs/Character/Woodsman"),spawns[i].transform.position,Quaternion.identity) as GameObject;
				player1.tag = "Player";
				Woodsman woods = player1.GetComponent<Woodsman>();
				woods.moveAxisX = "Horizontal1";
				woods.moveAxisZ = "Vertical1";
				woods.classType = playerClass.WOODSMAN;
				woods.basicAttackKey = KeyCode.R;
				woods.init();
				woods.playerNum = i;
				players.Add (player1);
				break;
			case playerClass.SORCERER:
				GameObject player2 = Instantiate (Resources.Load("Prefabs/Character/Sorceress"),spawns[i].transform.position,Quaternion.identity) as GameObject;
				player2.tag = "Player";
				Sorcerer sorc = player2.GetComponent<Sorcerer>();
				sorc.moveAxisX = "Horizontal2";
				sorc.moveAxisZ = "Vertical2";
				sorc.classType = playerClass.SORCERER;
				//sorc.jumpKey = KeyCode.N;
				sorc.basicAttackKey = KeyCode.U;
				sorc.specialAttackKey = KeyCode.O;
				sorc.classAbilityKey = KeyCode.P;
				sorc.playerNum = i;
				players.Add (player2);
				break;

			case playerClass.ROGUE:
				GameObject player3 = Instantiate (Resources.Load("Prefabs/Character/Rogue"),spawns[i].transform.position,Quaternion.identity) as GameObject;
				player3.tag = "Player";
				Rogue rogue = player3.GetComponent<Rogue>();
				rogue.moveAxisX = "Horizontal1";
				rogue.moveAxisZ = "Vertical1";
				rogue.classType = playerClass.ROGUE;
				//rogue.jumpKey = KeyCode.C;
				rogue.basicAttackKey = KeyCode.G;
				rogue.playerNum = i;
				players.Add (player3);
				break;
			
			case playerClass.WARRIOR:
				GameObject player4 = Instantiate (Resources.Load("Prefabs/Character/Warrior"),spawns[i].transform.position,Quaternion.identity) as GameObject;
				player4.tag = "Player";
				Warrior war = player4.GetComponent<Warrior>();
				war.moveAxisX = "Horizontal2";
				war.moveAxisZ = "Vertical2";
				war.classType = playerClass.WARRIOR;
				//war.jumpKey = KeyCode.N;
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

	public void getNewSpawnPoints()
	{
		spawns = GameObject.FindGameObjectsWithTag("Respawn");
	}

	public void respawnAllPlayers()
	{
		for (int i = 0; i < players.Count; i++)
		{
			players[i].transform.position = spawns[i].transform.position;
			players[i].GetComponent<PlayerBase>().controllable = true;
		}
	}
}
