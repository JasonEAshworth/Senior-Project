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
	private float homerunTimer = 0.0f;
	private float homerunCooldown = 20.0f;

	// whirlwind
	private float whirlwindSpeedMod = 0.8f;
	private float whirlwindAttackRange = 2.5f;
	private float whirlwindDamageRange = 1.5f;
	private float whirlwindDamage = 15.0f;
	private float whirlwindForceRange = 8.0f;
	private float whirlwindForceMagnitude = 3.0f;
	private float whirlwindInterval = 0.2f;
	private float whirlwindTimer = 0.0f;
	private float whirlwindCooldown = 10.0f;
	private bool spinning = false;

	// shockwave
	public GameObject shockwavePrefab;
	private float shockwaveAttackRange = 3.0f;
	private float shockwaveSpawnDistance = 0.25f;
	private float shockwaveTimer = 0.0f;
	private float shockwaveCooldown = 15.0f;

	// room collapse
	public GameObject ceilingBoulder;
	private bool roomCollapsing = false;
	private float boulderFallInterval = 1.0f;
	private float boulderFallHeight = 10.0f;

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
		base.FixedUpdate();
		homerunTimer -= Time.deltaTime;
		shockwaveTimer -= Time.deltaTime;
		whirlwindTimer -= Time.deltaTime;

		if (!attackInProgress)
		{
			// Check for phase attacks first
			if (health <= maxHealth * 0.6f && !firestorm)
			{
				//move to center of room

				//do attack
				firestormAttack();
			}
			else if (health <= maxHealth * 0.3f && !roomCollapsing)
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
			if (homerunTimer <= 0.0f)
			{
				if (find(homeRunAttackRange))
				{
					attackInProgress = true;
					myAnimator.SetTrigger("homerun");
					homerunTimer = homerunCooldown;
				}
			}
			else if (shockwaveTimer <= 0.0f)
			{
				if (find(shockwaveAttackRange))
				{
					attackInProgress = true;
					myAnimator.SetTrigger("shockwave");
					shockwaveTimer = shockwaveCooldown;
				}
			}
			else if (whirlwindTimer <= 0.0f)
			{
				if (find(whirlwindAttackRange))
				{
					attackInProgress = true;
					myAnimator.SetTrigger("whirlwind");
					whirlwindTimer = whirlwindCooldown;
				}
			}
			else
			{
				if (find(basicAttackRange))
				{
					attackInProgress = true;
					myAnimator.SetTrigger("basicAttack");
				}
			}
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

	public void startShockwave()
	{
		GameObject shockwave = Instantiate(shockwavePrefab, transform.position + shockwaveSpawnDistance * transform.forward, Quaternion.LookRotation(transform.forward)) as GameObject;
		shockwave.transform.parent = roomCenter.transform.root;
	}

	public void startWhirlwind()
	{
		spinning = true;
		StartCoroutine(whirlwindAttack());
		StartCoroutine(whirlwindPlayerSeek());
	}

	public void endWhirlwind()
	{
		spinning = false;
	}

	private IEnumerator whirlwindAttack()
	{
		LayerMask playerMask = LayerMask.GetMask(new string[]{"Player"});

		while (spinning)
		{
			// Draw all players in within a large range
			Collider[] hit = Physics.OverlapSphere(transform.position, whirlwindForceRange, playerMask);
			foreach (Collider c in hit)
			{
				Vector3 fromPlayer = (transform.position - c.transform.position).normalized;
				fromPlayer *= whirlwindForceMagnitude;
				c.GetComponent<CharacterBase>().addForce(fromPlayer);
			}
			// Damage all players in a small sphere
			hit = Physics.OverlapSphere(transform.position, whirlwindDamageRange, playerMask);
			foreach (Collider c in hit)
			{
				c.SendMessage("takeDamage", whirlwindDamage);
			}
			yield return new WaitForSeconds(whirlwindInterval);
		}
	}

	private IEnumerator whirlwindPlayerSeek()
	{
		moveMulti *= whirlwindSpeedMod;
		while (spinning)
		{
			GameObject target = findClosestPlayer();
			moveTowardsPlayer(target, Time.deltaTime);
			yield return new WaitForEndOfFrame();
		}
		moveMulti /= whirlwindSpeedMod;
		yield return null;
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
			Vector3 boulderPos = new Vector3(roomCenter.transform.position.x + Random.Range(-20.0f, 20.0f), roomCenter.transform.position.y + boulderFallHeight, roomCenter.transform.position.z + Random.Range(-20.0f, 20.0f));
			GameObject boulder = Instantiate(ceilingBoulder, boulderPos, Quaternion.identity) as GameObject;
			boulder.transform.parent = roomCenter.transform.root; // make sure the boulder is spawned as a child of the current room
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
