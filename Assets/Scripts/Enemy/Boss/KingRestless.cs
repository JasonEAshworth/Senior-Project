using UnityEngine;
using System.Collections;

public class KingRestless : EnemyBase
{
	private Animator myAnimator;

	// basic attack
	private float basicAttackRange = 0.8f;
	// home run 
	private float homeRunAttackRange = 1.0f;
	// whirlwind
	private float whirlwindAttackRange = 2.5f;
	private float whirlwindDamageRange = 1.0f;
	private float whirlwindDamage = 15.0f;
	private float whirlwindForceRange = 5.0f;
	private float whirlwindForceMagnitude = 1.0f;
	// room collapse
	private bool roomCollapsing = false;
	// general
	private bool attackBegun = false;
	private int currentAttack = -1; 		// the index of the current attack in progress
											// -1 no attack
											// 0 basic attack
											// 1 home run
											// 2 whirlwind
											// 3 shockwave
											// 4 firestorm
											// 5 room collapse
											// 6 for future use

	void Start()
	{
		base.Start();
		myAnimator = GetComponent<Animator>();
	}

	void FixedUpdate()
	{
		GameObject closestPlayer;
		switch (currentAttack)
		{
		// Choose attack
		case -1:
			if (health < maxHealth * 0.3f && !roomCollapsing)
			{
				currentAttack = 5;
			}
			else
			{
				int r;
				if (health > maxHealth * 0.5f)
				{
					r = Random.Range(1, 16);
				}
				else
				{
					r = Random.Range(1, 18);
				}
				if (r <= 10)
				{
					currentAttack = 0;
				}
				else if (r <= 12)
				{
					currentAttack = 1;
				}
				else if (r <= 14)
				{
					currentAttack = 2;
				}
				else if (r <= 16)
				{
					currentAttack = 3;
				}
				else 
				{
					currentAttack = 4;
				}
			}
			break;
		// Basic attack
		case 0:
			// Get within range of closest player
			if (!attackBegun)
			{
				closestPlayer = findClosestPlayer();
				moveTowardsPlayer(closestPlayer, Time.deltaTime);
				rotateTowardsPlayer(closestPlayer, Time.deltaTime);
				if (Vector3.Magnitude(closestPlayer.transform.position - transform.position) < basicAttackRange)
				{
					attackBegun = true;
				}
			}
			else if (attackBegun)
			{
				// wait for animation to finish playing and set currentAttack to -1
			}
			break;
		// Home run
		case 1:
			// Get within range of closest player
			if (!attackBegun)
			{
				closestPlayer = findClosestPlayer();
				moveTowardsPlayer(closestPlayer, Time.deltaTime);
				rotateTowardsPlayer(closestPlayer, Time.deltaTime);
				if (Vector3.Magnitude(closestPlayer.transform.position - transform.position) < homeRunAttackRange)
				{
					attackBegun = true;
				}
			}
			else if (attackBegun)
			{
				// wait for animation to finish playing and set currentAttack to -1
			}
			break;
		// Whirlwind
		case 2:
			// Get within range of closest player
			if (!attackBegun)
			{
				closestPlayer = findClosestPlayer();
				moveTowardsPlayer(closestPlayer, Time.deltaTime);
				rotateTowardsPlayer(closestPlayer, Time.deltaTime);
				if (Vector3.Magnitude(closestPlayer.transform.position - transform.position) < homeRunAttackRange)
				{
					attackBegun = true;
				}
			}
			else if (attackBegun)
			{
				// If the animation is still playing...
				LayerMask playerMask = LayerMask.GetMask(new string[]{"Player"});
				// Draw all players in within a large range
				Collider[] hit = Physics.OverlapSphere(transform.position, whirlwindForceRange, playerMask);
				foreach (Collider c in hit)
				{
					Vector3 fromPlayer = (transform.position - c.transform.position).normalized;
					fromPlayer *= whirlwindForceMagnitude * Time.deltaTime;
					c.GetComponent<CharacterBase>().addForce(fromPlayer);
				}
				// Damage all players in a small sphere
				hit = Physics.OverlapSphere(transform.position, whirlwindDamageRange, playerMask);
				foreach (Collider c in hit)
				{
					c.GetComponent<CharacterBase>().takeDamage(whirlwindDamage);
				}
			}
			break;
		}
	}
}
