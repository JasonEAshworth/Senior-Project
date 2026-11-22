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
	public Transform shootPos;

	// Behavior / Rates
	private bool chasing = false;
	private float attackTime = 0.0f;
	public float aR;
	
	// Use this for initialization
	void Awake() 
	{
		mTransform = transform;
		shootPos = transform.Find ("shootPos");
		aR = 2.2f;
		// Stagger the first shot so a group does not fire in one volley.
		attackTime = Random.Range(0.0f, aR);
	}
	// Update is called once per frame
	protected override void FixedUpdate() 
	{
		base.FixedUpdate();
		player = findClosestPlayerInRange (eRange);
		// Nobody in range: the 2015 code dereferenced null here every physics tick.
		if (player == null)
			return;
		target = player.transform;
		pDistance = (target.position - mTransform.position).magnitude;

		if (chasing) 
		{
			//Debug.Log("Should be Chasing");
			if(pDistance > giveUpThreshold)
			{
				chasing = false;
			}

			else if(pDistance <= eRange && pDistance >= attackDistance)
			{
				//Debug.Log ("Should be Attacking");
				cc.Move(mTransform.forward * moveSpeed * Time.deltaTime * moveMulti);
				rotateTowardsPlayer(player, Time.deltaTime);
				attackTime = Time.deltaTime + attackTime;
				if(attackTime >= aR)
				{
					attackTime = 0.0f;
					StartCoroutine(Shoot());
				}
				moveMulti = 0.2f;
			}

			else if(pDistance <= attackDistance)
			{
				// rotate 180 degrees and go to 1/2 the distance of the attack 'sphere'
				//mTransform.position += mTransform.forward*-1 * moveSpeed * Time.deltaTime;
				mTransform.rotation = Quaternion.Slerp(mTransform.rotation, target.rotation, Time.deltaTime * rotationSpeed);
				cc.Move(mTransform.forward * moveSpeed * Time.deltaTime * moveMulti);
				moveMulti = 3.0f;
				//Debug.Log("Should be running");
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

	/// The 2015 version never fired: its timer was commented out and its bullet prefab is gone.
	IEnumerator Shoot()
	{
		attackStartedTime = Time.time;
		// Let the draw or cast animation lead the shot.
		yield return new WaitForSeconds(0.35f);
		if (dead || player == null)
			yield break;
		Vector3 from = transform.position + Vector3.up * 1.1f + transform.forward * 0.6f;
		Vector3 dir = player.transform.position - transform.position;
		dir.y = 0.0f;
		BossProjectile.Fire(from, dir.normalized * 11.0f, attackDamage, ShotColor());
	}

	Color ShotColor()
	{
		VariantBehavior vb = GetComponent<VariantBehavior>();
		if (vb != null && vb.variant != null && vb.variant.glowColor.maxColorComponent > 0.01f)
			return vb.variant.glowColor;
		return new Color(0.6f, 1.0f, 0.4f);
	}
}