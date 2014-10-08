using UnityEngine;
using System.Collections;

public class ProjectileTrapObj : MonoBehaviour 
{
	public float damage = 10.0f;		// amount of damage dealt to the player on contact
	public float lifetime = 3.0f;		// how long this trap object has to live
	public float travelSpeed = 3.0f;	// speed in units per second that this projectile travels
	//may be needed to ensure the object faces and moves in the correct directions
	//this value should be assigned when it is created by TimedTrap or TriggerTrap
	//the traps should face the direction the projectile is going to travel
	//thus it should be set to the trap's transform.forward
	public Vector3 travelDir = Vector3.zero;
	
	void FixedUpdate()
	{
		lifetime -= Time.deltaTime;
		if (lifetime <= 0.0f)
		{
			Destroy(gameObject);
		}

		Vector3 start = this.transform.position;
		this.transform.position += this.travelDir * this.travelSpeed * Time.deltaTime;
		Vector3 end = this.transform.position;

		Collider cc = this.collider;

		Vector3 sc = this.transform.localScale;
		float radius = cc.bounds.max.magnitude * Mathf.Max(sc.x, sc.y, sc.z);
		Vector3 p1 = cc.bounds.min + this.travelDir * radius;
		Vector3 p2 = cc.bounds.max - this.travelDir * radius;
		RaycastHit[] rh = Physics.CapsuleCastAll(p1, p2, radius, this.travelDir, Vector3.Distance(start, end));
		foreach(RaycastHit hit in rh)
		{
			if(hit.transform.gameObject.tag == "Player")
			{
				Debug.Log("hit player!!!");
				hit.transform.GetComponent<PlayerBase>().takeDamage(this.damage);
				this.trapEffect(hit.transform.gameObject);
				Destroy(this.gameObject);
			}
			if(hit.transform.name.Contains("wall_"))
			{
				Debug.Log("hit something else!!!");
				Destroy(this.gameObject);
			}
		}
	}

	// Can be overridden in a script that inherits from projectiletrapobj in case we want a projectile trap to have an additional effect on the player
	public virtual void trapEffect(GameObject go) {}
}
