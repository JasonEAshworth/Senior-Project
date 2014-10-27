using UnityEngine;
using System.Collections;

public class ProjectileTrapObj : TrapBase 
{
	public float travelSpeed = 5.0f;	// speed in units per second that this projectile travels
	//may be needed to ensure the object faces and moves in the correct directions
	//this value should be assigned when it is created by TimedTrap or TriggerTrap
	//the traps should face the direction the projectile is going to travel
	//thus it should be set to the trap's transform.forward
	public Vector3 travelDir = Vector3.zero;
	
	protected override void FixedUpdate()
	{
		base.FixedUpdate();

		Vector3 start = this.transform.position;
		this.transform.position += this.travelDir * this.travelSpeed * Time.deltaTime;
		Vector3 end = this.transform.position;

		Collider cc = this.collider;

		Vector3 sc = this.transform.localScale;
		Vector3 size = cc.bounds.size;
		float radius = Mathf.Pow(Mathf.Max(size.x, size.y, size.z), 2) * 0.5f * Mathf.Max(sc.x, sc.y, sc.z);
		//min and max are quite correct
		Vector3 p1 = cc.bounds.min + this.travelDir * radius;
		Vector3 p2 = cc.bounds.max - this.travelDir * radius;
		RaycastHit[] rh = Physics.CapsuleCastAll(p1, p2, radius, this.travelDir, Vector3.Distance(start, end));


//		SphereCollider s = this.collider as SphereCollider;
//		Vector3 sc = this.transform.localScale;
//		float radius = s.radius * Mathf.Max(sc.x, sc.y, sc.z);
//		Debug.Log ("or: " + s.radius + "\nnr: " + radius);
//		RaycastHit[] rh = Physics.SphereCastAll(start, radius, this.travelDir, Vector3.Distance(start, end));

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

	protected override void ActivateTrigger (bool state)
	{
	}
}
