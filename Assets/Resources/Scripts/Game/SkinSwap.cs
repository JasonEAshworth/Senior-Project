using UnityEngine;

/// Dresses an enemy in another model from a Resources/Skins folder. The zombie underneath stays
/// in charge: its rig, AI, and hit timing are unchanged, and only its meshes are hidden. The skin
/// copies what the enemy does (walk, run, attack, flinch) and collapses on its own when the enemy
/// dies. When a skin is not installed, the enemy simply stays a zombie.
public class SkinSwap : MonoBehaviour
{
	const string Folder = "Skins/";
	// Each skin animator has three attacks, three flinches, and three deaths to pick from.
	const int Variations = 3;

	public GameObject skin;
	Animator anim;
	EnemyBase enemy;
	SkinInfo info;
	Vector3 lastPos;
	float lastAttack = -100.0f, lastHit = -100.0f;
	float speed;
	float nativeScale = 1.0f;

	public static bool Exists(string skinName)
	{
		return !string.IsNullOrEmpty(skinName) && Resources.Load<GameObject>(Folder + skinName) != null;
	}

	public static SkinSwap Apply(EnemyBase e, string skinName)
	{
		if (e == null || string.IsNullOrEmpty(skinName))
			return null;
		GameObject prefab = Resources.Load<GameObject>(Folder + skinName);
		if (prefab == null)
			return null;
		// Measure before hiding: a disabled renderer reports empty bounds.
		Bounds body;
		bool measured = Measure(e.gameObject, out body);
		foreach (Renderer r in e.GetComponentsInChildren<Renderer>())
			if (r is SkinnedMeshRenderer || r is MeshRenderer)
				r.enabled = false;

		GameObject go = Instantiate(prefab, e.transform);
		go.name = "Skin";
		go.transform.localPosition = Vector3.zero;
		go.transform.localRotation = prefab.transform.localRotation;
		SkinInfo info = go.GetComponent<SkinInfo>();
		if (measured && info != null && info.height > 0.1f)
		{
			float own = info.height * go.transform.lossyScale.y / Mathf.Max(0.01f, prefab.transform.localScale.y);
			go.transform.localScale *= Mathf.Clamp(body.size.y * info.heightScale / own, 0.2f, 5.0f);
			go.transform.position += Vector3.up * (body.min.y - go.transform.position.y);
		}

		SkinSwap s = e.gameObject.AddComponent<SkinSwap>();
		s.skin = go;
		s.anim = go.GetComponent<Animator>();
		s.enemy = e;
		s.info = info;
		s.lastPos = e.transform.position;
		s.nativeScale = Mathf.Max(0.01f, prefab.transform.localScale.y);
		e.flashRenderers = go.GetComponentsInChildren<Renderer>();
		return s;
	}

	static bool Measure(GameObject go, out Bounds b)
	{
		b = new Bounds();
		bool any = false;
		foreach (Renderer r in go.GetComponentsInChildren<Renderer>())
		{
			if (!r.enabled || !(r is SkinnedMeshRenderer || r is MeshRenderer))
				continue;
			if (!any) { b = r.bounds; any = true; }
			else b.Encapsulate(r.bounds);
		}
		return any;
	}

	void Update()
	{
		if (anim == null || enemy == null || skin == null || Time.deltaTime <= 0.0f)
			return;
		Vector3 step = transform.position - lastPos;
		step.y = 0.0f;
		lastPos = transform.position;
		speed = Mathf.Lerp(speed, step.magnitude / Time.deltaTime, 10.0f * Time.deltaTime);

		// The clips' own speeds grow with the skin's size: a giant takes longer strides.
		float size = skin.transform.lossyScale.y / nativeScale;
		float walk = (info != null ? info.walkSpeed : 1.4f) * size;
		float run = (info != null ? info.runSpeed : 4.0f) * size;
		bool running = speed > (walk + run) * 0.5f;
		anim.SetBool("walking", speed > walk * 0.2f);
		anim.SetBool("running", running);
		anim.SetFloat("moveSpeed", Mathf.Clamp(speed / (running ? run : walk), 0.5f, 2.2f));

		if (enemy.attackStartedTime > lastAttack)
		{
			lastAttack = enemy.attackStartedTime;
			anim.SetInteger("attackIndex", Random.Range(0, Variations));
			anim.SetTrigger("attack");
		}
		if (enemy.lastHitTime > lastHit)
		{
			lastHit = enemy.lastHitTime;
			// A flinch would cancel the swing the zombie underneath is still timing.
			if (Time.time - enemy.attackStartedTime > 0.8f)
			{
				anim.SetInteger("hitIndex", Random.Range(0, Variations));
				anim.SetTrigger("hit");
			}
		}
	}

	/// Leaves the skin behind to play its death, since the enemy object is destroyed at once.
	public void OnDeath()
	{
		if (skin == null)
			return;
		skin.transform.SetParent(null, true);
		if (anim != null)
		{
			anim.SetBool("walking", false);
			anim.SetInteger("dieIndex", Random.Range(0, Variations));
			anim.SetTrigger("die");
		}
		Destroy(skin, 3.0f);
		skin = null;
	}
}
