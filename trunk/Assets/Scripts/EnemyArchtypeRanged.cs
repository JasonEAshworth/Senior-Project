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

	// Behavior / Rates
	private bool chasing = false;
	private float attackTime = Time.time;

	// Use this for initialization
	void Awake() 
	{
		mTransform = transform;
	}
	// Update is called once per frame
	void Update() 
	{
		player = findClosestPlayerInRange (eRange);
		target = player.transform;
		pDistance = (target.position - mTransform.position).magnitude;				
	
		if (chasing) 
		{
			mTransform.rotation = Quaternion.Slerp (mTransform.rotation, Quaternion.LookRotation(target.position - mTransform.position), rotationSpeed*Time.deltaTime);
			mTransform.position += mTransform.forward * moveSpeed * Time.deltaTime;

			if(pDistance > giveUpThreshold)
			{
				chasing = false;
			}

			if(pDistance < eRange && Time.time > attackTime)
			{
				attackTime = Time.time + attackRate;
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