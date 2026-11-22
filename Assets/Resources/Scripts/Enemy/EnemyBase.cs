using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum enemyArchtype
{
	MELEE,
	RANGED,
	HORDE,
	INTELLIGENT,
	MINDLESS,
	BOSS
}

public class EnemyBase : CharacterBase
{
	/// Every enemy in the scene, for the HUD's health bars and the bosses' summons.
	public static readonly List<EnemyBase> All = new List<EnemyBase>();

	// Init Enemy Variables
	public float respawnTimer = 0.0f;
	public int enemyNumber = -1;
	public enemyArchtype enemyType;
	public float damageTaken = 0.0f;

	// Attack Variables
	public bool attacking = false;
	public float attackDistance = 0f;
	public float giveUpThreshold = 0f;
	public float attackDamage = 0f;

	// Enemy Control Variables
	public bool partOfHorde = false;

	/// A boss has a name: it gets the big health bar, and its death ends the chapter.
	public string bossName = "";
	public int points = 0;              // 0 uses the archetype's default
	public string displayName = "";
	[System.NonSerialized] public float lastHitTime = -100.0f;
	[System.NonSerialized] public bool scaled;
	[System.NonSerialized] public float attackStartedTime = -100.0f;

	public bool IsBoss { get { return !string.IsNullOrEmpty(bossName); } }

	// Manager Code
	public EnemyManager manager;
	private MapManager mapManager;
	protected EnemyArchtypeHorde hordeManager;

	void OnEnable() { All.Add(this); }
	void OnDisable() { All.Remove(this); }

	protected override void Start()
	{
		base.Start();
		GameObject em = GameObject.Find("EnemyManager");
		if (em != null)
			manager = em.GetComponent<EnemyManager>();
		mapManager = MapManager.Instance;
		if (transform.parent != null)
		{
			hordeManager = transform.parent.GetComponent<EnemyArchtypeHorde>();
		}
		if (!scaled && !IsBoss)
		{
			// More heroes and more cleared chapters make every enemy tougher.
			scaled = true;
			maxHealth *= GameSession.EnemyHealthScale;
			health = maxHealth;
			attackDamage *= GameSession.EnemyDamageScale;
		}
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (cc.isGrounded)
		{
			forces = new Vector3(forces.x, Mathf.Max(0.0f, forces.y), forces.z);
		}
		else
		{
			addForce(new Vector3(0.0f, Physics.gravity.y * 2.0f * Time.deltaTime, 0.0f));
		}
	}

	public override void takeDamage(float amount)
	{
		if (dead)
			return;
		lastHitTime = Time.time;
		ArcadeHud.Damage(transform.position, amount, false);
		base.takeDamage(amount);
	}

	public int PointValue
	{
		get
		{
			if (points > 0) return points;
			switch (enemyType)
			{
				case enemyArchtype.RANGED: return 150;
				case enemyArchtype.HORDE: return 60;
				case enemyArchtype.BOSS: return 3000;
				default: return 100;
			}
		}
	}

	public override void kill()
	{
		if (!dead)
		{
			dead = true;
			DropGold dg = gameObject.GetComponent<DropGold> ();
			if (dg != null)
			{
				dg.Reward ();
			}
			ScoreKeeper.CreditKill(this);
			VariantBehavior vb = GetComponent<VariantBehavior>();
			if (vb != null)
				vb.OnDeath();
			SkinSwap skin = GetComponent<SkinSwap>();
			if (skin != null)
				skin.OnDeath();
			PowerUp.MaybeDrop(transform.position, IsBoss ? 1.0f : 0.08f);
			if (IsBoss && ChapterDirector.Instance != null)
				ChapterDirector.Instance.OnBossDefeated(this);
		}
		Destroy(gameObject);
	}

	/// Starts the swing on the enemy's own rig and tells its skin, if it wears one, to swing too.
	protected void PlayAttack(Animator anim)
	{
		attackStartedTime = Time.time;
		// The unarmed and ranged zombies have no controller; triggering one only logs a warning.
		if (anim != null && anim.runtimeAnimatorController != null)
			anim.SetTrigger("attack");
	}

	protected void moveToPosition(Vector3 pos, float dt)
	{
		Vector3 moveVector = pos - transform.position;
		cc.Move(moveVector.normalized * dt * moveSpeed * moveMulti);
	}

	protected void rotateTowardsPoint(Vector3 pos, float dt)
	{
		transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation(new Vector3(pos.x, 0.0f, pos.z) - new Vector3(transform.position.x, 0.0f, transform.position.z)), rotationSpeed*Time.deltaTime);
	}

	protected void moveTowardsPlayer(GameObject player, float dt)
	{
		if (player == null)
			return;
		Vector3 toPlayer = Vector3.Normalize(player.transform.position - transform.position);
		Vector3 toCenter = Vector3.zero;
		if (partOfHorde && hordeManager != null)
		{
			toCenter = Vector3.Normalize(hordeManager.centerPoint - transform.position);
		}
		else
		{
			toCenter = toPlayer;
		}
		// check for obstacles
		Vector3 moveVector = toPlayer * 0.7f + toCenter * 0.3f;
		Vector3 dodgeVector = Vector3.zero;
		if (Physics.Raycast(transform.position, moveVector, Time.deltaTime * moveSpeed))
		{
			dodgeVector = transform.right * Time.deltaTime;
		}
		else
		{
			dodgeVector = toPlayer;
		}
		moveVector = moveVector * 0.4f + dodgeVector * 0.6f;
		// move towards destination
		cc.Move(moveVector * dt * moveSpeed * moveMulti);
	}

	protected void rotateTowardsPlayer(GameObject player, float dt)
	{
		if (player == null)
			return;
		transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation(new Vector3(player.transform.position.x, 0.0f, player.transform.position.z) - new Vector3(transform.position.x, 0.0f, transform.position.z)), rotationSpeed*Time.deltaTime);
	}

	/// The closest hero who is up and visible, or null when nobody is. The 2015 version
	/// returned the first player even when everyone was dead, and threw with no players.
	protected GameObject findClosestPlayer()
	{
		return findClosestPlayerInRange(float.PositiveInfinity);
	}

	protected GameObject findClosestPlayerInRange(float range)
	{
		PlayerManager pm = PlayerManager.Instance;
		if (pm == null)
			return null;
		GameObject closest = null;
		float shortest = range * range;
		foreach (GameObject p in pm.players)
		{
			if (p == null || !p.activeInHierarchy)
				continue;
			PlayerBase pb = p.GetComponent<PlayerBase>();
			if (pb.dead || pb.visibility == 0)
				continue;
			float sqr = (transform.position - p.transform.position).sqrMagnitude;
			if (sqr <= shortest)
			{
				shortest = sqr;
				closest = p;
			}
		}
		return closest;
	}

	public IEnumerator slow(){
		moveMulti = 0.5f;

		yield return StartCoroutine(Wait(5.0f));

		moveMulti = 1.0f;
	}

	public IEnumerator freeze(){
		moveMulti = 0.0f;

		yield return StartCoroutine (Wait (5.0f));

		moveMulti = 1.0f;
	}
}
