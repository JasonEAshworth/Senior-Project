using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyArchtypeMelee : EnemyBase {
	/* The Ranged AI Class Follows the following rule Set
	 * 1. Get in Range to call Fire() Based on Distance to the player
	 * 2. If Enemy is in Your "Radius" Then you Move Away.
	 * 3. Otherwise Fire() until Dead
	 */	
	public Transform target;
	private Transform mTransform;

	public float eRange = 20f;
	public float pDistance;
	public GameObject player;

	public float weaponReach = 1.5f;

	// Behavior / Rates
	private float attackRate = 2.0f;
	private bool chasing = false;
	private float attackTime = Time.time;
	
	// Use this for initialization
	void Awake() 
	{
		mTransform = transform;
	}
	// Update is called once per frame
	void FixedUpdate() 
	{
		base.FixedUpdate();
		player = findClosestPlayerInRange (eRange);
		if (player != null)
		{
			target = player.transform;
			pDistance = (target.position - mTransform.position).magnitude;
		
			if (chasing) 
			{
				if(pDistance > giveUpThreshold)
				{
					chasing = false;

				}

				else if(pDistance <= eRange && pDistance >= attackDistance)
				{
					cc.Move(mTransform.forward * moveSpeed * Time.deltaTime);
					rotateTowardsPlayer(player, Time.deltaTime);
					attackTime = Time.time + attackRate;
					if(attackTime >= attackRate && pDistance <= weaponReach)
					{
						Debug.Log ("SWING SWORD!");
						moveSpeed = 0f;
						Attack(attackRate);

					}
					moveSpeed = 2f;
				}

			}
			else
			{
				if(pDistance < eRange)
				{
					chasing = true;
				}
			}
		}
	}
}