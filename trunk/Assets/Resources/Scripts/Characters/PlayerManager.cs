using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Rewired;

public class PlayerManager : MonoBehaviour 
{
	// init variables
	public int numPlayers;
	private GameObject[] spawns;
	public List<GameObject> players;
	public List<playerClass> selectedClasses;

	public bool haveKey = false;

	public Woodsman woods;
	public Sorcerer sorc;
	public Warrior war;
	public Rogue rogue;

	public Vector3 playersCenter;
	
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
		//numPlayers = 4;
		Debug.Log (numPlayers);
		GameObject newStatBar = GameObject.Find("newStatBar");
		for (int i=0; i<numPlayers; i++) 
		{
			GameObject player = null;
			PlayerBase pBase;
			GameObject stats;
			Texture manaTex = null;
			Texture profTex = null;

			switch(selectedClasses[i])
			{
			case playerClass.WOODSMAN:
				player = Instantiate(Resources.Load("Prefabs/Character/WoodsMan/Woodsman"), spawns[i].transform.position, Quaternion.identity) as GameObject;
				manaTex = Resources.Load("Textures/GUI/gui_energy_woods") as Texture;
				profTex = Resources.Load("Textures/GUI/archer_profile") as Texture;
				break;
			case playerClass.SORCERER:
				player = Instantiate(Resources.Load("Prefabs/Character/Sorceress/Sorceress"), spawns[i].transform.position, Quaternion.identity) as GameObject;
				manaTex = Resources.Load("Textures/GUI/gui_energy_bar") as Texture;
				profTex = Resources.Load("Textures/GUI/sorc_profile") as Texture;
				break;
			case playerClass.ROGUE:
				player = Instantiate(Resources.Load("Prefabs/Character/Rogue/Rogue"), spawns[i].transform.position, Quaternion.identity) as GameObject;
				manaTex = Resources.Load("Textures/GUI/gui_energy_rogue") as Texture;
				profTex = Resources.Load("Textures/GUI/rogue_profile") as Texture;
				break;
			case playerClass.WARRIOR:
				player = Instantiate(Resources.Load("Prefabs/Character/Warrior/Warrior"), spawns[i].transform.position, Quaternion.identity) as GameObject;
				manaTex = Resources.Load("Textures/GUI/gui_energy_war") as Texture;
				profTex = Resources.Load("Textures/GUI/warrior_profile") as Texture;
				break;
			}

			players.Add(player);

			player.tag = "Player";
			pBase = player.GetComponent<PlayerBase>();
			pBase.playerNum = i;
			pBase.mana = 100.0f;
			player.AddComponent<rewiredControl>();

			stats = Instantiate(Resources.Load("Prefabs/Gui/Player")) as GameObject;
			stats.transform.SetParent(newStatBar.transform);
			stats.transform.position = Vector3.zero;
			stats.transform.localScale = Vector3.one;

			RectTransform rt = stats.GetComponent<RectTransform>();
			rt.offsetMin = Vector2.zero;
			rt.offsetMax = Vector2.zero;
			stats.name = pBase.classType.ToString();

			rt.anchorMin = new Vector2(i * 0.25f, 0.0f);
			rt.anchorMax = new Vector2((i + 1) * 0.25f, 0.2f);

			pBase.healthBar = stats.transform.Find("HealthBar").GetComponent<RawImage>();
			pBase.manaBar = stats.transform.Find("ManaBar").GetComponent<RawImage>();
			pBase.manaBar.texture = manaTex;
			pBase.potionImg = stats.transform.Find("Potion").GetComponent<RawImage>();
			stats.transform.Find("Emblem/Character").GetComponent<RawImage>().texture = profTex;
			pBase.scoreText = stats.transform.Find("Score").GetComponent<Text>();



//			switch (selectedClasses[i])
//			{
//			case playerClass.WOODSMAN:
//				GameObject player1 = Instantiate (Resources.Load("Prefabs/Character/WoodsMan/Woodsman"),spawns[i].transform.position,Quaternion.identity) as GameObject;
//				player1.tag = "Player";
//				woods = player1.GetComponent<Woodsman>();
//				woods.classType = playerClass.WOODSMAN;
//				woods.init();
//				player1.AddComponent("rewiredControl");
//				rewiredControl ctrl = player1.GetComponent<rewiredControl>();
//				ctrl.playerId = i;
//				woods.playerNum = i;
//				woods.healthBar = GameObject.Find("woodsmanHealth").GetComponent<RawImage>();
//				woods.manaBar = GameObject.Find("woodsmanMana").GetComponent<RawImage>();
//				woods.potionImg = GameObject.Find("woodPotionImg").GetComponent<RawImage>();
//				woods.scoreText = GameObject.Find("woodScore").GetComponent<Text>();
//				woods.formMana(1);
//				players.Add (player1);
//				break;
//			case playerClass.SORCERER:
//				GameObject player2 = Instantiate (Resources.Load("Prefabs/Character/Sorceress/Sorceress"),spawns[i].transform.position,Quaternion.identity) as GameObject;
//				player2.tag = "Player";
//				sorc = player2.GetComponent<Sorcerer>();
//				sorc.classType = playerClass.SORCERER;
//				player2.AddComponent<rewiredControl>();
//				rewiredControl ctrl2 = player2.GetComponent<rewiredControl>();
//				ctrl2.playerId = i;
//				sorc.playerNum = i;
//				sorc.health = 100.0f;
//				sorc.maxHealth = 100.0f;
//				sorc.mana = 100.0f;
//				sorc.healthBar = GameObject.Find("SorcererHealth").GetComponent<RawImage>();
//				sorc.manaBar = GameObject.Find("SorcererMana").GetComponent<RawImage>();
//				sorc.potionImg = GameObject.Find("sorcPotionImg").GetComponent<RawImage>();
//				sorc.scoreText = GameObject.Find("sorcScore").GetComponent<Text>();
//				sorc.formMana(1);
//				sorc.GetComponentInChildren<Light>().color = new Color(1.0f, 0.6f, 0.6f);
//				players.Add (player2);
//				break;
//
//			case playerClass.ROGUE:
//				GameObject player3 = Instantiate (Resources.Load("Prefabs/Character/Rogue/Rogue"),spawns[i].transform.position,Quaternion.identity) as GameObject;
//				player3.tag = "Player";
//				rogue = player3.GetComponent<Rogue>();
//				rogue.classType = playerClass.ROGUE;
//				player3.AddComponent<rewiredControl>();
//				rewiredControl ctrl3 = player3.GetComponent<rewiredControl>();
//				ctrl3.playerId = i;
//				rogue.playerNum = i;
//				rogue.health = 100.0f;
//				rogue.maxHealth = 100.0f;
//				rogue.mana = 100.0f;
//				rogue.healthBar = GameObject.Find("RogueHealth").GetComponent<RawImage>();
//				rogue.manaBar = GameObject.Find("RogueMana").GetComponent<RawImage>();
//				rogue.potionImg = GameObject.Find("roguePotionImg").GetComponent<RawImage>();
//				rogue.scoreText = GameObject.Find("rogueScore").GetComponent<Text>();
//				rogue.formMana(1);
//				players.Add (player3);
//				break;
//			
//			case playerClass.WARRIOR:
//				GameObject player4 = Instantiate (Resources.Load("Prefabs/Character/Warrior/Warrior"),spawns[i].transform.position,Quaternion.identity) as GameObject;
//				player4.tag = "Player";
//				war = player4.GetComponent<Warrior>();
//				war.classType = playerClass.WARRIOR;
//				player4.AddComponent<rewiredControl>();
//				rewiredControl ctrl4 = player4.GetComponent<rewiredControl>();
//				ctrl4.playerId = i;
//				war.playerNum = i;
//				war.health = 100.0f;
//				war.maxHealth = 100.0f;
//				war.mana = 0.0f;
//				war.healthBar = GameObject.Find("WarriorHealth").GetComponent<RawImage>();
//				war.manaBar = GameObject.Find("WarriorMana").GetComponent<RawImage>();
//				war.potionImg = GameObject.Find("warPotionImg").GetComponent<RawImage>();
//				war.scoreText = GameObject.Find("warScore").GetComponent<Text>();
//				war.formMana(0);
//				players.Add (player4);
//				break;
//			}
//			Debug.LogWarning(players [i].GetComponent<PlayerBase>().GetType().ToString());
		}
	}

	public void Update() 
	{
		updateCenterLocation();
	}

	public Vector3 getRespawnPoint()
	{
		int randSpawn = Random.Range (0,4);
		return spawns[randSpawn].transform.position;
	}

	public void assignNewSpawnPoints(GameObject[] newSpawns)
	{
		spawns = newSpawns;
	}

	public void respawnAllPlayers()
	{
		for (int i = 0; i < players.Count; i++)
		{
			players[i].transform.position = spawns[i].transform.position;
			players[i].GetComponent<PlayerBase>().controllable = true;
		}
	}

	public void updateCenterLocation()
	{
		Vector3 center = Vector3.zero;
		for (int i = 0; i <  players.Count; i++) 
		{
			center = center + players[i].transform.position;
		}
		center = center / players.Count;

		playersCenter = center;
		GameObject.Find ("Center").transform.position = center;
	}
}
