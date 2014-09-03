using UnityEngine;
using System.Collections;

public class CharacterBase : MonoBehaviour 
{
	public int health = 100;
	public int maxHealth;
	public bool dead = false;

	public float moveSpeed = 6.0f;
	public float rotationSpeed = 250.0f;
	public float visibility = 1.0f;

	public void takeDamage(int amount)
	{
		health -= amount;
		if (health <= 0)
		{
			kill();
		}
	}

	public virtual void kill()
	{
		dead = true;
		// temp code for testing
		renderer.material.color = Color.red;
	}
}
