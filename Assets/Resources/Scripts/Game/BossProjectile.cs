using UnityEngine;

/// A glowing shot from a boss. It flies straight, hurts the first hero it touches, and fades
/// after a few seconds; it passes through walls, as arcade shots do.
public class BossProjectile : MonoBehaviour
{
	Vector3 velocity;
	float damage;
	float diesAt;

	public static void Fire(Vector3 at, Vector3 velocity, float damage, Color color)
	{
		GameObject go = Fx.Orb(at, 0.45f, color, false);
		go.name = "BossShot";
		BossProjectile p = go.AddComponent<BossProjectile>();
		p.velocity = velocity;
		p.damage = damage;
		p.diesAt = Time.time + 4.0f;
	}

	void Update()
	{
		transform.position += velocity * Time.deltaTime;
		foreach (Collider c in Physics.OverlapSphere(transform.position, 0.5f, LayerMask.GetMask("Player")))
		{
			PlayerBase pb = c.GetComponent<PlayerBase>();
			if (pb != null && !pb.dead)
			{
				pb.takeDamage(damage);
				Destroy(gameObject);
				return;
			}
		}
		if (Time.time > diesAt)
			Destroy(gameObject);
	}
}
