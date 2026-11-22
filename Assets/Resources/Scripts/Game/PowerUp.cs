using UnityEngine;

/// A glowing orb dropped by enemies. The first hero to touch it gets a short boost.
public class PowerUp : MonoBehaviour
{
	public PowerUpType type;
	float bornAt;
	Vector3 basePos;

	static readonly Color[] colors = {
		new Color(1.0f, 0.3f, 0.2f, 0.9f), new Color(0.3f, 1.0f, 0.4f, 0.9f),
		new Color(0.4f, 0.7f, 1.0f, 0.9f), new Color(1.0f, 0.5f, 0.85f, 0.9f) };
	public static readonly string[] Names = { "FURY", "HASTE", "SHIELD", "HEAL" };

	public static void MaybeDrop(Vector3 at, float chance)
	{
		if (Random.value > chance)
			return;
		var type = (PowerUpType)Random.Range(0, 4);
		GameObject go = Fx.Orb(new Vector3(at.x, Mathf.Max(at.y, 0.0f) + 0.8f, at.z), 0.55f, colors[(int)type]);
		go.name = "PowerUp " + type;
		SphereCollider c = go.AddComponent<SphereCollider>();
		c.isTrigger = true;
		c.radius = 1.2f;
		Rigidbody rb = go.AddComponent<Rigidbody>();
		rb.isKinematic = true;
		PowerUp p = go.AddComponent<PowerUp>();
		p.type = type;
	}

	void Start()
	{
		bornAt = Time.time;
		basePos = transform.position;
	}

	void Update()
	{
		transform.position = basePos + Vector3.up * Mathf.Sin((Time.time - bornAt) * 3.0f) * 0.2f;
		if (Time.time - bornAt > 15.0f)
			Destroy(gameObject);
	}

	void OnTriggerEnter(Collider other)
	{
		if (!other.CompareTag("Player"))
			return;
		PlayerBase pb = other.GetComponent<PlayerBase>();
		if (pb == null || pb.dead)
			return;
		pb.ApplyPowerUp(type);
		ArcadeHud.Popup(transform.position, Names[(int)type] + "!", colors[(int)type]);
		Destroy(gameObject);
	}
}
