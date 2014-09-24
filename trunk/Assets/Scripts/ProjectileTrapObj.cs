using UnityEngine;
using System.Collections;

public class ProjectileTrapObj : MonoBehaviour 
{
	public float damage = 10.0f;		// amount of damage dealt to the player on contact
	public float lifetime = 3.0f;		// how long this trap object has to live
	public float travelSpeed = 3.0f;	// speed in units per second that this projectile travels
	
	void FixedUpdate()
	{
		lifetime -= Time.deltaTime;
		if (lifetime <= 0.0f)
		{
			Destroy(gameObject);
		}
		transform.position += transform.forward * travelSpeed * Time.deltaTime;
	}

	void OnTriggerEnter(Collider c)
	{
		if (c.tag == "Player")
		{
			c.GetComponent<PlayerBase>().takeDamage(damage);
		}
		Destroy(gameObject);
	}

	// Can be overridden in a script that inherits from projectiletrapobj in case we want a projectile trap to have an additional effect on the player
	public virtual void trapEffect() {}
}
