using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyArchtypeHorde : MonoBehaviour
{
	private Transform enemyTransform;
	private Vector3[] playerPositions;
	private float attackRange;
	private bool canFly;
	private Vector3 center;

	// AI constructor. Takes the enemy's transform for movement reasons.
	// An array of Vectors for player positions. A float for the range of attack.
	public void EnemyArchtypeHord(Transform eT, Vector3[] pP, float aR)
	{
		enemyTransform = eT;
		playerPositions = pP;
		attackRange = aR;
		canFly = false;
	
	}

	// Sets the fly status of an enemy. Flying enemies can fly over pits.
	public void setFly(bool flyStatus) 
	{
		canFly = flyStatus;
	}

	public void setCenter(Vector3 newCenter)
	{
		center = newCenter;
	}

	/* The update for the AI. Horde AI follows these rules
	 *   1. Attack a player within attack range
	 *   2. If Player not in range, find closest player
	 *   3. Create a vector to the player and a vector to center point
	 *   4. If there is something blocking the enemy, create vector pointing to the left or right of it. 
	 *   5. Factor the player vector, center vector, and the dodge vector to get the enemy's direction to move
	 *   6. Use the dT and enemy speed combined with the direction to get the location to move to
	 *   7. Confirm that moving to that space is legal (No moving though objects, or over pits if canFly is false)
	 *   8. If the space isn't legal, slowly decreese the distance until a legal spot is found. 
	 */
	public void update()
	{

	}
}