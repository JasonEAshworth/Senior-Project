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

	protected CharacterController cc;
	private Vector3 forces = Vector3.zero;			// used to apply outside forces on the character (since the character controller won't allow us to use a rigidbody)
	private float forceFriction = 5.0f;
	private float maxForce = 10.0f;

	protected float damageInvulnTime = 0.5f; 		// after taking damage, the character is invulnerable for this many seconds
	protected float currentDamageCooldown = 0.0f;	// the character has this many seconds before they can take damage again

	public RawImage healthBar;

	protected void Start()
	{
		cc = GetComponent<CharacterController>();
	}

	public void FixedUpdate()
	{
		cc.Move(forces * Time.deltaTime);
		forces = Vector3.Lerp(forces, Vector3.zero, forceFriction * Time.deltaTime);
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
		dead = true;
		// temp code for testing
		renderer.material.color = Color.red;
	}

	public void takeDamage(float amount)
	{
		if (currentDamageCooldown > 0.0f)
		{
			return;
		}
		health -= amount;

		float amt4Health = amount / maxHealth;
		//healthBar.rectTransform.sizeDelta = healthBar.rectTransform.sizeDelta - (new Vector2 (322*amt4Health, 0.0f));
	
		if (health <= 0)
		{
			kill();
		}
		else
		{
			currentDamageCooldown = damageInvulnTime;
		}
	}
}
