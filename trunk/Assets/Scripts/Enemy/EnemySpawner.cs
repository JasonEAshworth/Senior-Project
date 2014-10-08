using UnityEngine;
using System.Collections;

public enum spawnerType
{
	TIMER,
	TRIGGER
}

public class EnemySpawner : MonoBehaviour 
{
	public spawnerType spawnType;			// determines if the spawner is timer based or trigger based
	public bool infiniteSpawn = false;		// if set to true, enemies will continue to spawn until spawner is shut down
	private bool ableToSpawn = true;		// set to false if the enemies should not spawn immediately
	private bool enemiesRemaining = true;	// becomes false when spawner is spent
	public float spawnTimer = 3.0f;			// enemy will spawn every spawnTimer seconds
	private float timeTilSpawn;				// remaining time until next spawn
	public int[] numberToSpawn;   			// array of ints that correspond with array of enemy prefabs
	public GameObject[] enemiesToSpawn;		// array of enemy prefabs that will be spawned
											// spawner will spawn numberToSpawn[i] of enemiesToSpawn[i] enemy prefabs
											// ex: numberToSpawn[0] = 4
											//	   enemiesToSpawn[0] = zombieprefab
											// 	   spawner will spawn 4 zombies
	void Start()
	{
		timeTilSpawn = spawnTimer;
		if (numberToSpawn.Length != enemiesToSpawn.Length)
		{
			Debug.Log ("numberToSpawn and enemiesToSpawn on " + gameObject.name + "'s EnemySpawner must be the same length");
		}
	}

	void Update()
	{
		if (enemiesToSpawn.Length > 0 && enemiesRemaining && ableToSpawn)
		{
			if (spawnType == spawnerType.TIMER)
			{
				timeTilSpawn -= Time.deltaTime;
				if (timeTilSpawn <= 0.0f)
				{
					spawnEnemy();
					timeTilSpawn += spawnTimer;
				}
			}
			else if (spawnType == spawnerType.TRIGGER)
			{
				spawnEnemy();
				ableToSpawn = false;
			}
		}
	}

	private void spawnEnemy()
	{
		// Pick a random enemy type to spawn
		int index = Random.Range(0, enemiesToSpawn.Length-1);
		if (!infiniteSpawn)
		{
			while (numberToSpawn[index] == 0)
			{
				index = (index + 1) % numberToSpawn.Length;
			}
		}
		Instantiate(enemiesToSpawn[index], transform.position, transform.rotation);

		// Check to see if we have any enemies left to spawn
		if (!infiniteSpawn)
		{
			numberToSpawn[index]--;
			bool canContinue = false;
			for (int i = 0; i < numberToSpawn.Length; i++)
			{
				if (numberToSpawn[i] != 0)
				{
					canContinue = true;
				}
			}
			if (!canContinue)
			{
				enemiesRemaining = false;
			}
		}
	}

	// Call this to enable spawning. Can be used to allow the timer-based spawning to start or trigger a single enemy spawn
	public void triggerSpawn()
	{
		ableToSpawn = true;
	}

	// Call this to pause a timer-based spawn
	public void disableSpawn()
	{
		ableToSpawn = false;
	}
}
