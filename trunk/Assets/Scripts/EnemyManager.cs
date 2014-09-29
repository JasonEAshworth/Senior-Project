using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour 
{
	// init variables
	public int numEnemies;

	private GameObject[] spawns;
	public List<GameObject> enemies;
	public List<enemyArchtype> enemyList;

	void Start() 
	{
		// instantiate enemy list
		enemies = new List<GameObject>();

		// instantiate selectedClasses and populate it
		enemyList = new List<enemyArchtype>();
		
		enemyList.Add(enemyArchtype.MELEE);
		enemyList.Add(enemyArchtype.RANGED);
		enemyList.Add(enemyArchtype.HORDE);
		enemyList.Add(enemyArchtype.INTELLIGENT);
		enemyList.Add(enemyArchtype.MINDLESS);
		// set players to 4 at start
		//numPlayers = 4;
		numEnemies = 0;
	}
	public enemyArchtype create(int type)
	{
		for (int i=0; i<numEnemies; i++)
		{
			switch (enemyList[i])
			{
				case(enemyArchtype.MELEE):
					// load zombie model here
					GameObject m = Instantiate (Resources.Load("Prefabs/Character/Woodsman"),spawns[i].transform.position,Quaternion.identity) as GameObject;
					//Melee m = mel.GetComponent<MELEE>();
					//m.init();
					//m.health = 100.f;
					//m.maxHealth = 100.0f;
					//enemies.Add(mel);
					break;
			}
		}
		return enemyArchtype.MELEE;
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

}
