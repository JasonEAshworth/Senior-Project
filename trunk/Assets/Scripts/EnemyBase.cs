using UnityEngine;
using System.Collections;

public enum enemyArchtype
{
	MELEE,
	RANGED,
	HORDE,
	INTELLIGENT,
	MINDLESS
}

public class EnemyBase : CharacterBase 
{
	public float respawnTimer = 0.0f;

	public int enemyNumber = -1;

	public enemyArchtype enemyType;

	public bool attacking = false;

	// Manager Code
	public EnemyManager manager;
	private MapManager mapManager;

	void Start()
	{
		health = maxHealth;
		manager = GameObject.Find("EnemyManager").GetComponent<EnemyManager>();
		mapManager = GameObject.Find("MapManager").GetComponent<MapManager>();
		renderer.material.color = Color.blue;
	}

	protected new void FixedUpdate()
	{
		if (!dead)
		{
			// move around the screen based on AI and enemyController script.
			// attack player if within range (melee,ranged)
		}
	}

	public virtual void Attakck()
	{
		// do calculations based on atk power and player def

	}

}
