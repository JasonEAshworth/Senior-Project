using System.Collections;
using UnityEngine;

/// A chapter mini-boss: a large, tinted zombie that chases and swings like the melee zombie and
/// mixes in special attacks from its chapter's list. Every special shows a floor warning first,
/// and below half health it uses them more often.
public class MiniBoss : EnemyBase
{
	public MiniBossConfig config;
	public ChapterDefinition chapter;

	Animator anim;
	int attackIndex;
	float nextSpecial;
	float attackStartedAt;
	bool busy;
	float size = 1.0f;

	static readonly Color Warn = new Color(1.0f, 0.15f, 0.1f, 0.45f);

	public static MiniBoss Create(MiniBossConfig config, Vector3 at, Transform parent, ChapterDefinition chapter)
	{
		GameObject prefab = Resources.Load<GameObject>("Prefabs/Enemies/Zombie/Restless-Melee");
		if (prefab == null)
			return null;
		GameObject go = Instantiate(prefab, at, Quaternion.identity, parent);
		EnemyArchtypeMelee melee = go.GetComponent<EnemyArchtypeMelee>();
		GameObject mesh = melee != null ? melee.characterMesh : null;
		if (melee != null)
			DestroyImmediate(melee);
		MiniBoss mb = go.AddComponent<MiniBoss>();
		mb.config = config;
		mb.chapter = chapter;
		mb.bossName = config.name;
		mb.enemyType = enemyArchtype.BOSS;
		mb.characterMesh = mesh;
		mb.points = 2500;
		mb.size = config.scale;
		go.name = config.name;
		SkinSwap.Apply(mb, config.skin);
		go.transform.localScale *= config.scale;
		foreach (Renderer r in go.GetComponentsInChildren<Renderer>())
			foreach (Material m in r.materials)
				if (m.HasProperty("_Color"))
					m.color = m.color * config.tint;
		return mb;
	}

	protected override void Start()
	{
		base.Start();
		anim = GetComponent<Animator>();
		maxHealth = config.health * GameSession.BossHealthScale * (1.0f + 0.25f * GameSession.SealCount);
		health = maxHealth;
		attackDamage = config.damage * GameSession.EnemyDamageScale;
		moveSpeed = config.speed;
		nextSpecial = Time.time + 3.0f;
	}

	bool Enraged { get { return health < maxHealth * 0.5f; } }

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (dead || busy)
			return;
		if (attacking && Time.time - attackStartedAt > 1.6f)
			attacking = false;   // in case the swing animation never sends its end event

		GameObject target = findClosestPlayer();
		// Wait in the boss room until the party arrives; the room wakes one room early.
		bool engaged = ChapterDirector.Instance == null || ChapterDirector.Instance.BossEngaged;
		if (target == null || !engaged)
		{
			anim.SetBool("walking", false);
			nextSpecial = Time.time + 3.0f;
			return;
		}

		if (Time.time >= nextSpecial && config.attacks.Length > 0)
		{
			MiniBossAttack a = config.attacks[attackIndex++ % config.attacks.Length];
			nextSpecial = Time.time + (Enraged ? 4.0f : 6.5f);
			StartCoroutine(Special(a, target));
			return;
		}

		Vector3 to = target.transform.position - transform.position;
		to.y = 0;
		if (to.magnitude <= 1.9f * size)
		{
			anim.SetBool("walking", false);
			rotateTowardsPlayer(target, Time.deltaTime);
			if (!attacking)
			{
				PlayAttack(anim);
				attacking = true;
				attackStartedAt = Time.time;
			}
		}
		else if (!attacking)
		{
			anim.SetBool("walking", true);
			rotateTowardsPlayer(target, Time.deltaTime);
			cc.Move(transform.forward * moveSpeed * moveMulti * Time.deltaTime * (Enraged ? 1.25f : 1.0f));
		}
	}

	// Animation events from the zombie's swing.
	private void Attack()
	{
		foreach (Collider c in Physics.OverlapSphere(transform.position + transform.forward * 1.2f * size, 1.4f * size, LayerMask.GetMask("Player")))
		{
			PlayerBase pb = c.GetComponent<PlayerBase>();
			if (pb != null)
				pb.takeDamage(attackDamage);
		}
	}

	public void notifyAttackEnd()
	{
		attacking = false;
	}

	IEnumerator Special(MiniBossAttack a, GameObject target)
	{
		busy = true;
		anim.SetBool("walking", false);
		switch (a)
		{
			case MiniBossAttack.Slam: yield return Slam(); break;
			case MiniBossAttack.Charge: yield return Charge(target); break;
			case MiniBossAttack.Summon: yield return Summon(); break;
			case MiniBossAttack.FireRing: yield return FireRing(); break;
			case MiniBossAttack.HookPull: yield return HookPull(); break;
			case MiniBossAttack.Volley: yield return Volley(); break;
		}
		busy = false;
	}

	void HurtPlayersAround(Vector3 center, float radius, float damage, float push)
	{
		foreach (Collider c in Physics.OverlapSphere(center, radius, LayerMask.GetMask("Player")))
		{
			PlayerBase pb = c.GetComponent<PlayerBase>();
			if (pb == null)
				continue;
			pb.takeDamage(damage);
			Vector3 away = c.transform.position - center;
			away.y = 0;
			pb.addForce(away.normalized * push + Vector3.up * 3.0f);
		}
	}

	IEnumerator Slam()
	{
		float radius = 4.5f;
		Fx.WarningDisc(transform.position, radius, 0.9f, Warn);
		PlayAttack(anim);
		yield return new WaitForSeconds(0.9f);
		HurtPlayersAround(transform.position, radius, attackDamage * 1.6f, 9.0f);
		Fx.Flash(transform.position + Vector3.up * 0.5f, radius, new Color(1.0f, 0.6f, 0.3f, 0.6f));
		CameraShake.Add(0.5f);
		yield return new WaitForSeconds(0.4f);
	}

	IEnumerator Charge(GameObject target)
	{
		Vector3 dir = target.transform.position - transform.position;
		dir.y = 0;
		dir.Normalize();
		transform.rotation = Quaternion.LookRotation(dir);
		Fx.WarningLine(transform.position, dir, 13.0f, 2.2f, 0.75f, Warn);
		yield return new WaitForSeconds(0.75f);
		anim.SetBool("walking", true);
		var hit = new System.Collections.Generic.HashSet<PlayerBase>();
		for (float t = 0; t < 0.8f && !dead; t += Time.deltaTime)
		{
			cc.Move(dir * 16.0f * Time.deltaTime);
			foreach (Collider c in Physics.OverlapSphere(transform.position, 1.6f * size, LayerMask.GetMask("Player")))
			{
				PlayerBase pb = c.GetComponent<PlayerBase>();
				if (pb != null && hit.Add(pb))
				{
					pb.takeDamage(attackDamage * 1.3f);
					pb.addForce(dir * 10.0f + Vector3.up * 4.0f);
				}
			}
			yield return null;
		}
		anim.SetBool("walking", false);
		yield return new WaitForSeconds(0.5f);
	}

	IEnumerator Summon()
	{
		Fx.Flash(transform.position + Vector3.up, 2.5f, new Color(0.6f, 0.3f, 1.0f, 0.7f));
		yield return new WaitForSeconds(0.6f);
		if (chapter == null || chapter.variants.Length == 0)
			yield break;
		int alive = 0;
		foreach (EnemyBase e in EnemyBase.All)
			if (e != null && !e.dead && !e.IsBoss) alive++;
		int count = Enraged ? 4 : 3;
		for (int i = 0; i < count && alive + i < 8; i++)
		{
			Vector3 p = transform.position + Quaternion.Euler(0, i * 360.0f / count, 0) * Vector3.forward * 4.0f;
			ChapterDirector.SpawnVariant(chapter.variants[Random.Range(0, chapter.variants.Length)], p, transform.parent);
			Fx.Flash(p + Vector3.up, 1.2f, new Color(0.6f, 0.3f, 1.0f, 0.7f));
		}
		yield return new WaitForSeconds(0.4f);
	}

	IEnumerator FireRing()
	{
		Fx.WarningDisc(transform.position, 2.0f, 0.7f, new Color(1.0f, 0.5f, 0.1f, 0.5f));
		yield return new WaitForSeconds(0.7f);
		int rings = Enraged ? 2 : 1;
		for (int r = 0; r < rings; r++)
		{
			int n = 14;
			for (int i = 0; i < n; i++)
			{
				Vector3 dir = Quaternion.Euler(0, (i + r * 0.5f) * 360.0f / n, 0) * Vector3.forward;
				BossProjectile.Fire(transform.position + Vector3.up * 1.0f + dir, dir * 7.0f, attackDamage * 0.7f, new Color(1.0f, 0.45f, 0.1f, 0.95f));
			}
			yield return new WaitForSeconds(0.5f);
		}
	}

	IEnumerator HookPull()
	{
		GameObject far = null;
		float best = 0;
		foreach (GameObject p in PlayerManager.Instance.players)
		{
			if (p == null || !p.activeInHierarchy || p.GetComponent<PlayerBase>().dead)
				continue;
			float d = (p.transform.position - transform.position).sqrMagnitude;
			if (d > best && d < 16 * 16) { best = d; far = p; }
		}
		if (far == null)
			yield break;
		Vector3 dir = far.transform.position - transform.position;
		Fx.WarningLine(transform.position, dir, dir.magnitude, 0.8f, 0.6f, Warn);
		yield return new WaitForSeconds(0.6f);
		if (far == null || !far.activeInHierarchy)
			yield break;
		PlayerBase pb = far.GetComponent<PlayerBase>();
		Vector3 pull = transform.position - far.transform.position;
		pull.y = 0;
		pb.addForce(pull.normalized * Mathf.Min(24.0f, pull.magnitude * 2.2f) + Vector3.up * 3.0f);
		pb.takeDamage(attackDamage * 0.5f);
		yield return new WaitForSeconds(0.5f);
	}

	IEnumerator Volley()
	{
		for (int burst = 0; burst < 3; burst++)
		{
			GameObject t = findClosestPlayer();
			if (t == null)
				yield break;
			Vector3 aim = t.transform.position - transform.position;
			aim.y = 0;
			aim.Normalize();
			transform.rotation = Quaternion.LookRotation(aim);
			for (int i = -2; i <= 2; i++)
			{
				Vector3 dir = Quaternion.Euler(0, i * 12.0f, 0) * aim;
				BossProjectile.Fire(transform.position + Vector3.up * 1.2f + dir, dir * 10.0f, attackDamage * 0.6f, new Color(0.6f, 1.0f, 0.4f, 0.95f));
			}
			yield return new WaitForSeconds(0.35f);
		}
		yield return new WaitForSeconds(0.4f);
	}

	public override void kill()
	{
		StopAllCoroutines();
		base.kill();
	}
}
