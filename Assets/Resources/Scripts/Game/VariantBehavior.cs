using UnityEngine;

/// Turns a stock zombie into a chapter variant (tint, size, stats, glow) and runs its trait:
/// exploding or splitting on death, or charging at players.
public class VariantBehavior : MonoBehaviour
{
	public EnemyVariant variant;
	EnemyBase enemy;
	float nextCharge;
	bool isSplit;

	public static void Apply(EnemyBase e, EnemyVariant v, bool split = false)
	{
		if (e == null)
			return;
		VariantBehavior vb = e.gameObject.AddComponent<VariantBehavior>();
		vb.variant = v;
		vb.enemy = e;
		vb.isSplit = split;
		float size = split ? v.scale * 0.6f : v.scale;
		e.transform.localScale *= size;
		e.displayName = v.name;
		e.points = split ? Mathf.Max(20, v.points / 3) : v.points;
		e.maxHealth *= split ? v.healthMultiplier * 0.3f : v.healthMultiplier;
		e.health = e.maxHealth;
		e.attackDamage *= v.damageMultiplier;
		e.moveSpeed *= v.speedMultiplier * (split ? 1.3f : 1.0f);
		foreach (Renderer r in e.GetComponentsInChildren<Renderer>())
		{
			foreach (Material m in r.materials)
				if (m.HasProperty("_Color"))
					m.color = m.color * v.tint;
		}
		if (v.glowColor.maxColorComponent > 0.01f)
		{
			var glow = new GameObject("Glow");
			glow.transform.SetParent(e.transform, false);
			glow.transform.localPosition = new Vector3(0, 1.2f, 0);
			Light l = glow.AddComponent<Light>();
			l.color = v.glowColor;
			l.range = 3.5f * size;
			l.intensity = 1.6f;
		}
	}

	void FixedUpdate()
	{
		if (variant == null || enemy == null || enemy.dead || variant.trait != VariantTrait.Charges)
			return;
		if (Time.time < nextCharge)
			return;
		GameObject target = NearestPlayer(9.0f);
		if (target == null)
			return;
		nextCharge = Time.time + Random.Range(3.5f, 5.0f);
		Vector3 dir = target.transform.position - transform.position;
		dir.y = 0;
		Fx.WarningLine(transform.position, dir, 6.0f, 1.2f, 0.5f, new Color(1, 0.2f, 0.1f, 0.4f));
		StartCoroutine(Charge(dir.normalized));
	}

	System.Collections.IEnumerator Charge(Vector3 dir)
	{
		yield return new WaitForSeconds(0.5f);
		if (enemy != null && !enemy.dead)
			enemy.addForce(dir * 18.0f);
	}

	GameObject NearestPlayer(float range)
	{
		PlayerManager pm = PlayerManager.Instance;
		if (pm == null)
			return null;
		GameObject best = null;
		float bestSqr = range * range;
		foreach (GameObject p in pm.players)
		{
			if (p == null || !p.activeInHierarchy || p.GetComponent<PlayerBase>().dead)
				continue;
			float d = (p.transform.position - transform.position).sqrMagnitude;
			if (d < bestSqr) { bestSqr = d; best = p; }
		}
		return best;
	}

	public void OnDeath()
	{
		if (variant == null)
			return;
		if (variant.trait == VariantTrait.ExplodesOnDeath)
		{
			Fx.Flash(transform.position + Vector3.up, 3.0f, new Color(1.0f, 0.5f, 0.1f, 0.8f));
			CameraShake.Add(0.3f);
			foreach (Collider c in Physics.OverlapSphere(transform.position, 3.0f, LayerMask.GetMask("Player")))
			{
				PlayerBase pb = c.GetComponent<PlayerBase>();
				if (pb != null)
					pb.takeDamage(18.0f * GameSession.EnemyDamageScale);
			}
		}
		else if (variant.trait == VariantTrait.SplitsOnDeath && !isSplit)
		{
			for (int i = 0; i < 2; i++)
			{
				Vector3 at = transform.position + new Vector3(i == 0 ? -0.8f : 0.8f, 0.2f, 0);
				GameObject prefab = Resources.Load<GameObject>(ChapterDirector.PrefabFor(variant.replaces));
				if (prefab == null)
					return;
				GameObject go = Instantiate(prefab, at, transform.rotation, transform.parent);
				Apply(go.GetComponent<EnemyBase>(), variant, true);
			}
		}
	}
}
