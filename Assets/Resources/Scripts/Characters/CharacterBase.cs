using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CharacterBase : MonoBehaviour 
{
	public float health = 100.0f;
	public float maxHealth = 100.0f;
	public float score = 0.0f;
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

	public RawImage healthBar;
	protected float healthBarWidth;
	protected float healthBarHeight;

	public RawImage potionImg;

	public GameObject characterMesh;

	protected virtual void Start()
	{
		cc = GetComponent<CharacterController>();

		if (healthBar)
		{
			if (potionImg)
			{
				potionImg.enabled = false;
			}
			healthBarWidth = healthBar.rectTransform.rect.width;
			healthBarHeight = healthBar.rectTransform.rect.height;
		}
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
		float amt4Health = health / maxHealth;
		dead = true;
		if (healthBar)
		{
			healthBar.rectTransform.sizeDelta = healthBar.rectTransform.sizeDelta - (new Vector2 (healthBarWidth*amt4Health, 0.0f));
		}
	}

	public virtual void respawn()
	{
		health = maxHealth;
		if (healthBar)
		{
			healthBar.rectTransform.sizeDelta = healthBar.rectTransform.sizeDelta + (new Vector2 (healthBarWidth*1, 0.0f));
		}
	}

	public void takeDamage(float amount)
	{
		health -= amount;

		// Flash red to indicate damage taken
		if (characterMesh != null)
		{
			StopCoroutine ("flashRed"); // stop the character from flashing if they are flashing already
			StartCoroutine("flashRed");
		}

		float amt4Health = amount / maxHealth;
		if (healthBar != null)
		{
			healthBar.rectTransform.sizeDelta = healthBar.rectTransform.sizeDelta - (new Vector2 (healthBarWidth*amt4Health, 0.0f));
		}
		if (health <= 0)
		{
			kill();
		}
		else
		{
			currentDamageCooldown = damageInvulnTime;
		}
	}

	public virtual void addItem(GameObject p)
	{
		if(p.tag == "Potion")
		{
			potionImg.enabled = true;
		}
	}

	public virtual void addScore(GameObject p)
	{
		if (p.tag == "Coin")
		{
			score += 1;
		}
		if (p.tag == "Gold")
		{
			score += 10;
		}
	}

	public void itemAbility(string i, float amount)
	{
		if(i == "Potion")
		{
			potionImg.enabled = false;
			if(health + amount > maxHealth)
			{
				Debug.Log(amount);
				Debug.Log(health);
				Debug.Log(maxHealth);
				amount -= (health + amount) - maxHealth;
			}
			float amt4Health = amount / maxHealth;
			healthBar.rectTransform.sizeDelta = healthBar.rectTransform.sizeDelta + (new Vector2 (healthBarWidth*amt4Health, 0.0f));

		}
	}

	public IEnumerator Wait(float sec){
		yield return new WaitForSeconds (sec);
	}

	public IEnumerator flashRed()
	{
		float start = Time.time;
		float end = start + damageInvulnTime;
		Material[] modelMats = characterMesh.renderer.materials;

		while (Time.time <= end)
		{
			float gbValue = Mathf.SmoothStep(0.0f, 1.0f, (Time.time - start) / (end - Time.time));
			foreach (Material m in modelMats)
			{
				m.color = new Color(1.0f, gbValue, gbValue);
			}

			yield return null;
		}
	}
}
