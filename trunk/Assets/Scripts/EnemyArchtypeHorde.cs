using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyArchtypeHorde : MonoBehaviour
{
	private Transform enemyTransform;
	public float attackRange = 5;
	public bool canFly = true;
	private Vector3 center;

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
	void FixedUpdate()
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
		// If within range, attack
		if (shortestRange < attackRange * attackRange) // squaring attack range is faster than square rooting every distance
		{
			// attack that player
		}
		// Otherwise, move towards closest player
		else
		{
			Vector3 toPlayer = Vector3.Normalize(players[closestPlayerIdx].transform.position - transform.position);
			Vector3 toCenter = Vector3.Normalize(center - transform.position);
			Vector3 dodgeVector = toPlayer; // temporary value if dodge is needed
			// check for obstacles
			if (Physics.Raycast(transform.position, transform.forward, 3.0f))
			{
				dodgeVector = transform.right;
			}
			// calculate movement vector and destination
			Vector3 moveVector = toPlayer * 0.4f + toCenter * 0.2f + dodgeVector * 0.4f; 
			Vector3 destination = transform.position + moveVector * Time.deltaTime;
			// unity navmesh stuff here
		}
	}
}