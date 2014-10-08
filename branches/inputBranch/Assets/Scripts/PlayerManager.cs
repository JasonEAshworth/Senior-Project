using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Microsoft.Win32;
using Rewired;


public class PlayerManager : MonoBehaviour 
{
	// init variables
	public int numPlayers;
	private GameObject[] spawns;
	public List<GameObject> players;
	public List<playerClass> selectedClasses;

	public bool haveKey = false;
	private List<string> xboxInputs;
	private List<string> keyboardInputs;

	public Woodsman woods;
	public Sorcerer sorc;
	public Warrior war;
	public Rogue rogue;

	void Start() 
	{
		// instantiate players list
		players = new List<GameObject>();

		// instantiate selectedClasses and populate it
		selectedClasses = new List<playerClass>();
		selectedClasses.Add(playerClass.WOODSMAN);
		selectedClasses.Add(playerClass.ROGUE);
		selectedClasses.Add(playerClass.SORCERER);
		selectedClasses.Add(playerClass.WARRIOR);

		// acquire all spawn points for players 
		getNewSpawnPoints();
		for (int i=0; i<selectedClasses.Count; i++) 
		{
			switch (selectedClasses[i])
			{
			case playerClass.WOODSMAN:
				GameObject player1 = Instantiate (Resources.Load("Prefabs/Character/Woodsman"),spawns[i].transform.position,Quaternion.identity) as GameObject;
				player1.tag = "Player";
				woods = player1.GetComponent<Woodsman>();
				woods.classType = playerClass.WOODSMAN;
				woods.init();
				player1.AddComponent("rewiredControl");
				rewiredControl ctrl = player1.GetComponent<rewiredControl>();
				ctrl.playerId = i;
				players.Add (player1);
				//classScript.Add(woods);
				break;

			case playerClass.SORCERER:
				GameObject player2 = Instantiate (Resources.Load("Prefabs/Character/Sorceress"),spawns[i].transform.position,Quaternion.identity) as GameObject;
				player2.tag = "Player";
				sorc = player2.GetComponent<Sorcerer>();	
				sorc.classType = playerClass.SORCERER;
				player2.AddComponent<rewiredControl>();
				rewiredControl ctrl2 = player2.GetComponent<rewiredControl>();
				ctrl2.playerId = i;	
				players.Add (player2);
				//lassScript.Add(sorc);
				break;

			case playerClass.ROGUE:
				GameObject player3 = Instantiate (Resources.Load("Prefabs/Character/Rogue"),spawns[i].transform.position,Quaternion.identity) as GameObject;
				player3.tag = "Player";
				rogue = player3.GetComponent<Rogue>();		
				rogue.classType = playerClass.ROGUE;
				player3.AddComponent<rewiredControl>();
				rewiredControl ctrl3 = player3.GetComponent<rewiredControl>();
				ctrl3.playerId = i;
				players.Add (player3);
				//lassScript.Add(rogue);
				break;
			
			case playerClass.WARRIOR:
				GameObject player4 = Instantiate (Resources.Load("Prefabs/Character/Warrior"),spawns[i].transform.position,Quaternion.identity) as GameObject;
				player4.tag = "Player";
				war = player4.GetComponent<Warrior>();
				war.classType = playerClass.WARRIOR;
				player4.AddComponent<rewiredControl>();
				rewiredControl ctrl4 = player4.GetComponent<rewiredControl>();
				ctrl4.playerId = i;
				players.Add (player4);
				//classScript.Add(war);
				break;
			}
		}
	}

	public Vector3 getRespawnPoint()
	{
		int randSpawn = UnityEngine.Random.Range (0,4);
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
