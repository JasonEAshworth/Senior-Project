using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour 
{
	// init variables
	public int numEnemies;
	private GameObject[] spawns;
	public List<GameObject> enemies;
	public List<enemyArchtype> enemyType;

	void Start() 
	{
		// instantiate enemy list
		enemies = new List<GameObject>();

		// instantiate selectedClasses and populate it
		enemyType = new List<enemyArchtype>();
	
		// set players to 4 at start
		//numPlayers = 4;
		numEnemies = 0;
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
