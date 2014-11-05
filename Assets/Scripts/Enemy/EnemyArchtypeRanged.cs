using UnityEngine;
using System.Collections;

public class EnemyArchtypeRanged : EnemyBase {
	/* The Ranged AI Class Follows the following rule Set
	 * 1. Get in Range to call Fire() Based on Distance to the player
	 * 2. If Enemy is in Your "Radius" Then you Move Away.
	 * 3. Otherwise Fire() until Dead
	 */	
	public Transform target;
	private Transform mTransform;

	public float eRange = 10f;
	public float pDistance;
	public GameObject player;
	public float attackRate = 2f;

	// Behavior / Rates
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
		player = findClosestPlayerInRange (eRange);
		target = player.transform;
		pDistance = (target.position - mTransform.position).magnitude;				
	
		if (chasing) 
		{
			Debug.Log("Should be Chasing");
			if(pDistance > giveUpThreshold)
			{
				chasing = false;

			}

			else if(pDistance <= eRange && pDistance >= attackDistance)
			{
				Debug.Log ("Should be Attacking");
				cc.Move(mTransform.forward * moveSpeed * Time.deltaTime);
				mTransform.rotation = Quaternion.Slerp (mTransform.rotation, Quaternion.LookRotation(target.position - mTransform.position), rotationSpeed*Time.deltaTime);
				attackTime = Time.time + attackRate;
				if(attackTime >= attackRate)
				{
					Debug.Log ("Firing Arrows!");
					Attack(attackRate);
				}
				moveSpeed = 0.2f;
			}

			else if(pDistance <= attackDistance)
			{
				// rotate 180 degrees and go to 1/2 the distance of the attack 'sphere'
				//mTransform.position += mTransform.forward*-1 * moveSpeed * Time.deltaTime;
				mTransform.rotation = Quaternion.Slerp(mTransform.rotation, target.rotation, Time.deltaTime * rotationSpeed);
				cc.Move(mTransform.forward * moveSpeed * Time.deltaTime);
				moveSpeed = 3;
				Debug.Log("Should be running");
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