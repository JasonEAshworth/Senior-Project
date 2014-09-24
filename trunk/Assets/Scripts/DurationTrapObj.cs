using UnityEngine;
using System.Collections;

public class DurationTrapObj : MonoBehaviour 
{
	public float damage = 10.0f;	// amount of damage dealt to the player on contact
	public float lifetime = 3.0f;	// how long this trap object has to live

	void FixedUpdate()
	{
		lifetime -= Time.deltaTime;
		if (lifetime <= 0.0f)
		{
			Destroy(gameObject);
		}
	}

	void OnTriggerStay(Collider c)
	{
		if (c.tag == "Player")
		{
			c.GetComponent<PlayerBase>().takeDamage(damage);
		}
	}

	// Can be overridden in a script that inherits from durationtrapobj in case we want a duration trap to have an additional effect on the player
	public virtual void trapEffect() {}
}
