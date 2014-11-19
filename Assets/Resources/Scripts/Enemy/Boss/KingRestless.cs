using UnityEngine;
using System.Collections;

public class KingRestless : EnemyBase
{
	private Animator myAnimator;
	private GameObject closestPlayer;

	// basic attack
	private float basicAttackDamage = 20.0f;
	private float basicAttackRange = 1.5f;

	// home run 
	private float homerunAttackDamage = 80.0f;
	private float homeRunAttackRange = 1.0f;

	// whirlwind
	private float whirlwindAttackRange = 2.5f;
	private float whirlwindDamageRange = 1.0f;
	private float whirlwindDamage = 15.0f;
	private float whirlwindForceRange = 5.0f;
	private float whirlwindForceMagnitude = 1.0f;

	// shockwave
	public GameObject shockwavePrefab;
	private float shockwaveAttackRange = 3.0f;
	private float shockwaveSpawnDistance = 0.25f;

	// room collapse
	public GameObject ceilingBoulder;
	private bool roomCollapsing = false;
	private float boulderFallInterval = 1.0f;

	//firestorm
	private bool firestorm = false;

	// general
	private bool attackInProgress = false;
	private GameObject roomCenter;


	protected override void Start()
	{
		base.Start();
		myAnimator = GetComponent<Animator>();
		roomCenter = GameObject.Find("Boss Room Center");
	}

	protected override void FixedUpdate()
	{
		if (!attackInProgress)
		{
			// Check for phase attacks first
			/*if (health <= maxHealth * 0.6f && !firestorm)
			{
				//move to center of room

				//do attack
				firestormAttack();
			}
			else if (health <= maxHealth * 0.3f && !roomCollapsing)*/
			if (!roomCollapsing)
			{
				// Move to the center of the room
				moveToPosition(roomCenter.transform.position, Time.deltaTime);
				rotateTowardsPoint(roomCenter.transform.position, Time.deltaTime);

				// HULK SMASH
				float dist = Vector3.Distance(transform.position, roomCenter.transform.position);
				if (dist < 0.4f)
				{
					attackInProgress = true;
					roomCollapsing = true;
					myAnimator.SetTrigger("roomCollapse");
				}
			}

			// Then check for other attacks
			/*if (find(basicAttackRange))
			{
				// Begins the attack animation, which has an animation event that calls basicAttack()
				attackInProgress = true;
				myAnimator.SetTrigger("basicAttack");
			}

			if (find(homeRunAttackRange))
			{
				attackInProgress = true;
				myAnimator.SetTrigger("homerun");
			}

			else if (find(shockwaveAttackRange))
			{
				StartCoroutine (shockwaveAttack());
			}

			else if (find(whirlwindAttackRange))
			{
				StartCoroutine (whirlwindAttack());
			}*/
		}
	}

	private bool find(float attackRange)
	{
		closestPlayer = findClosestPlayer();
		moveTowardsPlayer(closestPlayer, Time.deltaTime);
		rotateTowardsPlayer(closestPlayer, Time.deltaTime);
		if (Vector3.Magnitude(closestPlayer.transform.position - transform.position) < attackRange)
		{
			return true;
		}
		myAnimator.SetTrigger("walk");
		return false;
	}

	private IEnumerator shockwaveAttack()
	{
		attackInProgress = true;

		yield return StartCoroutine (Wait (2.0f));
		attackInProgress = false;
	}

	private IEnumerator whirlwindAttack()
	{
		attackInProgress = true;

		LayerMask playerMask = LayerMask.GetMask(new string[]{"Player"});
		// Draw all players in within a large range
		Collider[] hit = Physics.OverlapSphere(transform.position, whirlwindForceRange, playerMask);
		foreach (Collider c in hit)
		{
			Vector3 fromPlayer = (transform.position - c.transform.position).normalized;
			fromPlayer *= whirlwindForceMagnitude * Time.deltaTime;
			c.SendMessage("addForce", fromPlayer);
		}
		// Damage all players in a small sphere
		hit = Physics.OverlapSphere(transform.position, whirlwindDamageRange, playerMask);
		foreach (Collider c in hit)
		{
			c.SendMessage("takeDamage", whirlwindDamage);
		}
		
		yield return StartCoroutine (Wait (2.0f));
		attackInProgress = false;
	}

	public void basicAttack()
	{
		Collider[] hit = Physics.OverlapSphere(transform.position + transform.forward, 1.0f, LayerMask.GetMask("Player"));
		foreach (Collider c in hit)
		{
			c.SendMessage("takeDamage", basicAttackDamage);
		}
	}

	private void firestormAttack()
	{
		//this is a phase attack
		firestorm = true;
	}

	public void startRoomCollapse()
	{
		StartCoroutine(roomCollapseAttack());
	}

	private IEnumerator roomCollapseAttack()
	{
		while (roomCollapsing)
		{
			Vector3 boulderPos = new Vector3(roomCenter.transform.position.x + Random.Range(-15.0f, 15.0f), roomCenter.transform.position.y + 5.0f, roomCenter.transform.position.z + Random.Range(-15.0f, 15.0f));
			Instantiate(ceilingBoulder, boulderPos, Quaternion.identity);
			yield return new WaitForSeconds(boulderFallInterval);
		}
		yield return null;
	}

	public void homerunAttack()
	{
		Collider[] hit = Physics.OverlapSphere(transform.position + transform.forward, 1.0f, LayerMask.GetMask("Player"));
		foreach (Collider c in hit)
		{
			c.SendMessage("takeDamage", homerunAttackDamage);
			c.SendMessage("addForce", (c.transform.position - transform.position).normalized * 35.0f);
		}
	}

	public void notifyAttackEnd()
	{
		attackInProgress = false;
	}
}
