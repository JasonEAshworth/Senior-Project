using UnityEngine;
using System.Collections;

public enum enemyArchtype
{
	MELEE,
	RANGED,
	HORDE,
	INTELLIGENT,
	MINDLESS,
	BOSS
}

public class EnemyBase : CharacterBase 
{
	// Init Enemy Variables
	public float respawnTimer = 0.0f;
	public int enemyNumber = -1;
	public enemyArchtype enemyType;

	// Attack Variables
	public bool attacking = false;
	public float attackDistance = 0f;
	public float giveUpThreshold = 0f;
	public float attackRate = 0f;
	public float attackDamage = 0f;

	// Move Variables
	//public float moveSpeed = 1;
	//public float rotationSpeed = 3f;

	// Enemy Control Variables
	public bool partOfHorde = false;
	
	// Manager Code
	public EnemyManager manager;
	private MapManager mapManager;
	protected EnemyArchtypeHorde hordeManager;

	protected void Start()
	{
		base.Start();
		health = maxHealth;
		manager = GameObject.Find("EnemyManager").GetComponent<EnemyManager>();
		mapManager = GameObject.Find("MapManager").GetComponent<MapManager>();
		//renderer.material.color = Color.blue;
		if (transform.parent != null)
		{
			hordeManager = transform.parent.GetComponent<EnemyArchtypeHorde>();
			//partOfHorde = true;
		}
	}

	new protected void FixedUpdate()
	{
		if (!dead)
		{
			// move around the screen based on AI and enemyController script.
			// attack player if within range (melee,ranged)
		}
	}

	public virtual void Attack(float attackRate)
	{
		Debug.Log ("We have called attack!");
		// do calculations based on atk power and player def

	}

	protected void moveTowardsPlayer(GameObject player, float dt)
	{
		Vector3 toPlayer = Vector3.Normalize(player.transform.position - transform.position);
		Vector3 toCenter = Vector3.zero;
		if (partOfHorde)
		{
			toCenter = Vector3.Normalize(hordeManager.centerPoint - transform.position);
		}
		else
		{
			toCenter = toPlayer;
		}
		// check for obstacles
		Vector3 moveVector = toPlayer * 0.7f + toCenter * 0.3f;
		Vector3 dodgeVector = Vector3.zero;
		if (Physics.Raycast(transform.position, moveVector, Time.deltaTime * moveSpeed))
		{
			dodgeVector = transform.right * moveSpeed;
		}
		else
		{
			dodgeVector = toPlayer;
		}
		moveVector = moveVector * 0.4f + dodgeVector * 0.6f;
		// move towards destination
		cc.Move(moveVector * dt);
	}

	protected void rotateTowardsPlayer(GameObject player, float dt)
	{
		transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation(new Vector3(player.transform.position.x, 0.0f, player.transform.position.z) - new Vector3(transform.position.x, 0.0f, transform.position.z)), rotationSpeed*Time.deltaTime);
	}

	protected GameObject findClosestPlayer()
	{
		// Find the closest player and see if they are in range
		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
		float shortestRange = float.PositiveInfinity;
		int closestPlayerIdx = 0;
		for (int i = 0; i < players.Length; i++)
		{
			float sqrRange = Vector3.SqrMagnitude(transform.position - players[i].transform.position);	// squared magnitude is faster
			shortestRange = Mathf.Min(sqrRange, shortestRange);
			closestPlayerIdx = i;
		}
		return players[closestPlayerIdx];
	}

	protected GameObject findClosestPlayerInRange(float range)
	{
		// Find the closest player and see if they are in range
		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
		float shortestRange = float.PositiveInfinity;
		int closestPlayerIdx = 0;
		for (int i = 0; i < players.Length; i++)
		{
			float sqrRange = Vector3.SqrMagnitude(transform.position - players[i].transform.position);	// squared magnitude is faster
			shortestRange = Mathf.Min(sqrRange, shortestRange);
			closestPlayerIdx = i;
		}
		if (shortestRange <= range * range) // squaring range is faster than square rooting every distance
		{
			return players[closestPlayerIdx];
		}
		return null;
	}
}
