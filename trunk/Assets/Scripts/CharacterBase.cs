using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CharacterBase : MonoBehaviour 
{
	public float health = 100.0f;
	public float maxHealth = 100.0f;
	public bool dead = false;

	public float moveSpeed = 6.0f;
	public float rotationSpeed = 250.0f;
	public float visibility = 1.0f;

	public RawImage healthBar;

	public virtual void kill()
	{
		dead = true;
		// temp code for testing
		renderer.material.color = Color.red;
	}

	public void takeDamage(float amount)
	{
		health -= amount;
		float amt4Health = amount / maxHealth;
		healthBar.rectTransform.sizeDelta = healthBar.rectTransform.sizeDelta - (new Vector2 (322*amt4Health, 0.0f));
		if (health <= 0)
		{
			kill();
		}
	}
}
