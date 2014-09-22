using UnityEngine;
using System.Collections;

public class CharacterBase : MonoBehaviour 
{
	public float health = 100.0f;
	public float maxHealth = 100.0f;
	public bool dead = false;

	public float moveSpeed = 6.0f;
	public float rotationSpeed = 250.0f;
	public float visibility = 1.0f;

	public virtual void kill()
	{
		dead = true;
		// temp code for testing
		renderer.material.color = Color.red;
	}
}
