using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviour 
{
	public int numPlayers;
	private GameObject[] spawns;
	public List<GameObject> players;
	public List<playerClass> selectedClasses;

	// Use this for initialization
	void Start () 
	{
		players = new List<GameObject>();
		selectedClasses = new List<playerClass>(4);
		selectedClasses.Add (playerClass.ADVENTURER);
		selectedClasses.Add(playerClass.ROGUE);
		selectedClasses.Add(playerClass.SORCERER);
		selectedClasses.Add(playerClass.WARRIOR);

		numPlayers = 4;
		spawns = GameObject.FindGameObjectsWithTag ("Respawn");

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
				players.Add (player1);
				break;
			case playerClass.SORCERER:
				GameObject player2 = Instantiate (Resources.Load("Prefabs/Sorceress"),spawns[i].transform.position,Quaternion.identity) as GameObject;
				player2.tag = "Player";
				Sorcerer sorc = player2.GetComponent<Sorcerer>();
				sorc.moveAxisX = "Horizontal1";
				sorc.moveAxisZ = "Vertical1";
				players.Add (player2);
				break;

			case playerClass.ROGUE:
				GameObject player3 = Instantiate (Resources.Load("Prefabs/Rogue"),spawns[i].transform.position,Quaternion.identity) as GameObject;
				player3.tag = "Player";
				Rogue rogue = player3.GetComponent<Rogue>();
				rogue.moveAxisX = "Horizontal1";
				rogue.moveAxisZ = "Vertical1";
				players.Add (player3);
				break;
			
			case playerClass.WARRIOR:
				GameObject player4 = Instantiate (Resources.Load("Prefabs/Warrior"),spawns[i].transform.position,Quaternion.identity) as GameObject;
				player4.tag = "Player";
				Warrior war = player4.GetComponent<Warrior>();
				war.moveAxisX = "Horizontal1";
				war.moveAxisZ = "Vertical1";
				players.Add (player4);
				break;
			}
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
}
