using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CharacterBase : MonoBehaviour 
{
	public float health = 100.0f;
	public float maxHealth = 100.0f;
	public bool dead = false;

	public float moveSpeed = 10.0f;
	public float rotationSpeed = 250.0f;
	public float visibility = 1.0f;
	public float moveMulti = 1.0f;

	protected CharacterController cc;
	public Vector3 forces = Vector3.zero;			// used to apply outside forces on the character (since the character controller won't allow us to use a rigidbody)
	private float forceFriction = 5.0f;
	private float maxForce = 10.0f;

	protected float damageInvulnTime = 0.5f; 		// after taking damage, the character is invulnerable for this many seconds
	protected float currentDamageCooldown = 0.0f;	// the character has this many seconds before they can take damage again

	public float attackMultiplier = 1.0f;
	
	public GameObject characterMesh;
	// Set when the visible model is not characterMesh, as on an enemy wearing a skeleton skin.
	[System.NonSerialized] public Renderer[] flashRenderers;
	Color[] baseColors;
	
	protected virtual void Start()
	{
		cc = GetComponent<CharacterController>();
	}

	protected virtual void FixedUpdate()
	{
		cc.Move(forces * Time.deltaTime * moveMulti);
		float y = forces.y;
		forces = Vector3.Lerp(forces, Vector3.zero, forceFriction * Time.deltaTime);
		forces = new Vector3(forces.x, y, forces.z);
		Vector3.ClampMagnitude(forces, maxForce);

		if (currentDamageCooldown > 0.0f)
		{
			currentDamageCooldown -= Time.deltaTime;
		}
	}

	public void addForce(Vector3 force)
	{
		forces += force;
	}

	public virtual void kill()
	{
		health = 0.0f;
		dead = true;
	}

	public virtual void respawn()
	{
		health = maxHealth;
	}

	public virtual void takeDamage(float amount)
	{
		health -= amount;

		// Flash red to indicate damage taken
		if (characterMesh != null)
		{
			StopCoroutine ("flashRed"); // stop the character from flashing if they are flashing already
			StartCoroutine("flashRed");
		}

		if (health <= 0)
		{
			health = 0.0f;
			kill();
		}
		else
		{
			currentDamageCooldown = damageInvulnTime;
		}
	}

	public IEnumerator Wait(float sec)
	{
		yield return new WaitForSeconds(sec);
	}

	Material[] FlashMaterials()
	{
		if (flashRenderers == null)
			return characterMesh.GetComponent<Renderer>().materials;
		var mats = new System.Collections.Generic.List<Material>();
		foreach (Renderer r in flashRenderers)
			if (r != null)
				mats.AddRange(r.materials);
		return mats.ToArray();
	}

	public IEnumerator flashRed()
	{
		float start = Time.time;
		float end = start + damageInvulnTime;
		Material[] modelMats = FlashMaterials();
		// Flash relative to each material's own color, so tinted enemy variants keep their tint.
		if (baseColors == null || baseColors.Length != modelMats.Length)
		{
			baseColors = new Color[modelMats.Length];
			for (int i = 0; i < modelMats.Length; i++)
				baseColors[i] = modelMats[i].HasProperty("_Color") ? modelMats[i].color : Color.white;
		}

		while (Time.time <= end)
		{
			float gbValue = Mathf.SmoothStep(0.0f, 1.0f, (Time.time - start) / (end - Time.time));
			for (int i = 0; i < modelMats.Length; i++)
			{
				if (modelMats[i].HasProperty("_Color"))
					modelMats[i].color = baseColors[i] * new Color(1.0f, gbValue, gbValue);
			}

			yield return null;
		}
		for (int i = 0; i < modelMats.Length; i++)
		{
			if (modelMats[i].HasProperty("_Color"))
				modelMats[i].color = baseColors[i];
		}
	}
}
