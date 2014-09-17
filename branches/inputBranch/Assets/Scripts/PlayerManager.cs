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
	private List<string> xboxInputs;
	private List<string> keyboardInputs;
	void Start() 
	{
		// instantiate players list
		players = new List<GameObject>();
		xboxInputs = new List<string> ();
		keyboardInputs = new List<string> ();


		// setup the strings for each controller
		string xbox1 = "XAxis1 Yaxis1 A1 B1 X1 Y1 Start1 Select1";
		string xbox2 = "XAxis2 Yaxis2 A2 B2 X2 Y2 Start2 Select2";
		string xbox3 = "XAxis3 Yaxis3 A3 B3 X3 Y3 Start3 Select3";
		string xbox4 = "XAxis4 Yaxis4 A4 B4 X4 Y4 Start4 Select4";
		xboxInputs.Add (xbox1);
		xboxInputs.Add (xbox2);
		xboxInputs.Add (xbox3);
		xboxInputs.Add (xbox4);

		// instantiate inventory list
		//teamInventory = new List<GameObject>();

		// instantiate selectedClasses and populate it
		selectedClasses = new List<playerClass>();
		selectedClasses.Add(playerClass.WOODSMAN);
		//selectedClasses.Add(playerClass.ROGUE);
		selectedClasses.Add(playerClass.SORCERER);
		//selectedClasses.Add(playerClass.WARRIOR);

		// set players to 4 at start
		numPlayers = 2;

		// acquire all spawn points for players 
		getNewSpawnPoints();
		Debug.Log (selectedClasses.Count);
		for (int i=0; i<selectedClasses.Count; i++) 
		{
			switch (selectedClasses[i])
			{
			case playerClass.WOODSMAN:
				GameObject player1 = Instantiate (Resources.Load("Prefabs/Character/Woodsman"),spawns[i].transform.position,Quaternion.identity) as GameObject;
				player1.tag = "Player";
				Woodsman woods = player1.GetComponent<Woodsman>();
				//woods.moveAxisX = "XAxis1";
				//woods.moveAxisZ = "YAxis1";
				woods.classType = playerClass.WOODSMAN;
				//woods.basicAttackKey = "X1";
				//woods.classAbilityKey = "B1";
				//woods.jumpKey = "A1";
				//woods.useItemKey = "Y1";
				woods.init();
				woods.playerNum = i;
				players.Add (player1);
				setupControls(player1,1,1);
				break;
			case playerClass.SORCERER:
				GameObject player2 = Instantiate (Resources.Load("Prefabs/Character/Sorceress"),spawns[i].transform.position,Quaternion.identity) as GameObject;
				player2.tag = "Player";
				Sorcerer sorc = player2.GetComponent<Sorcerer>();
				//sorc.moveAxisX = "XAxis2";
				//sorc.moveAxisZ = "YAxis2";
				sorc.classType = playerClass.SORCERER;
				//sorc.jumpKey = KeyCode.N;
				//sorc.basicAttackKey = "X2";
				//sorc.classAbilityKey = "B2";
				//sorc.jumpKey = "A2";
				//sorc.useItemKey = "Y2";
				sorc.playerNum = i;
				players.Add (player2);
				setupControls(player2,1,2);
				break;

			case playerClass.ROGUE:
				GameObject player3 = Instantiate (Resources.Load("Prefabs/Character/Rogue"),spawns[i].transform.position,Quaternion.identity) as GameObject;
				player3.tag = "Player";
				Rogue rogue = player3.GetComponent<Rogue>();
				//rogue.moveAxisX = "Horizontal1";
				//rogue.moveAxisZ = "Vertical1";
				rogue.classType = playerClass.ROGUE;
				//rogue.jumpKey = "Jump1";
				//rogue.basicAttackKey = "fire1";
				rogue.playerNum = i;
				players.Add (player3);
				setupControls(player3,1,3);
				break;
			
			case playerClass.WARRIOR:
				GameObject player4 = Instantiate (Resources.Load("Prefabs/Character/Warrior"),spawns[i].transform.position,Quaternion.identity) as GameObject;
				player4.tag = "Player";
				Warrior war = player4.GetComponent<Warrior>();
				//war.moveAxisX = "Horizontal2";
				//war.moveAxisZ = "Vertical2";
				war.classType = playerClass.WARRIOR;
				//war.jumpKey = "Jump2";
				//war.basicAttackKey = "fire2";
				war.playerNum = i;
				players.Add (player4);
				setupControls(player4,1,4);
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

	private void setupControls(GameObject go,int type,int playerType)
	{
		if (type == 1) 
		{
			switch(playerType)
			{
				case 1:
					Woodsman scr = go.GetComponent<Woodsman>();
					string[] buttons = xboxInputs[scr.playerNum].Split(" ");
					scr.moveAxisX = buttons[0];
					scr.moveAxisZ = buttons[1];
					scr.jumpKey = buttons[2];
					scr.classAbilityButton = buttons[3];
					scr.basicAttackButton = buttons[4];
					Debug.Log (scr.basicAttackButton);
					scr.useItemKey = buttons[5];
					break;
			case 2:
					Sorcerer scr2 = go.GetComponent<Sorcerer>();
					string[] buttons2 = xboxInputs[scr.playerNum].Split(" ");
					scr2.moveAxisX = buttons2[0];
					scr2.moveAxisZ = buttons2[1];
					scr2.jumpKey = buttons2[2];
					scr2.classAbilityButton = buttons2[3];
					scr2.basicAttackButton = buttons2[4];
					scr2.useItemKey = buttons2[5];
					break;
			case 3:
					Rogue scr3 = go.GetComponent<Rogue>();
					string[] buttons3 = xboxInputs[scr.playerNum].Split(" ");
					scr3.moveAxisX = buttons3[0];
					scr3.moveAxisZ = buttons3[1];
					scr3.jumpKey = buttons3[2];
					scr3.classAbilityButton = buttons3[3];
					scr3.basicAttackButton = buttons3[4];
					scr3.useItemKey = buttons3[5];
					break;
			case 4:
					Warrior scr4 = go.GetComponent<Warrior>();
					string[] buttons4 = xboxInputs[scr.playerNum].Split(" ");
					scr4.moveAxisX = buttons4[0];
					scr4.moveAxisZ = buttons4[1];
					scr4.jumpKey = buttons4[2];
					scr4.classAbilityButton = buttons4[3];
					scr4.basicAttackButton = buttons4[4];
					scr4.useItemKey = buttons4[5];
					break;
			}
			
			
		}
	}
}
