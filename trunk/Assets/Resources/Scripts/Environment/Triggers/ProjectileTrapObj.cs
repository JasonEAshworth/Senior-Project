using UnityEngine;
using System.Collections;

public class ProjectileTrapObj : TrapBase 
{
	public float lifetime = 5.0f;	// how long this trap object has to live
	public float travelSpeed = 5.0f;	// speed in units per second that this projectile travels

	//may be needed to ensure the object faces and moves in the correct directions
	//this value should be assigned when it is created by TimedTrap or TriggerTrap
	//the traps should face the direction the projectile is going to travel
	//thus it should be set to the trap's transform.forward
	public Vector3 travelDir = Vector3.zero;

	protected void Start()
	{
		if(!spawner)
		{
			StartCoroutine(waitToDestroy());
		}
	}
	
	protected void FixedUpdate()
	{
		if(!spawner)
		{
			Vector3 start = this.transform.position;
			this.transform.position += this.travelDir * this.travelSpeed * Time.deltaTime;
			Vector3 end = this.transform.position;
		}
	}

	protected IEnumerator waitToDestroy()
	{
		yield return new WaitForSeconds(this.lifetime);
		Destroy(this.gameObject);
	}

	protected override void ActivateTrigger(bool state)
	{
		GameObject p = Instantiate(this.gameObject, this.transform.position, Quaternion.identity) as GameObject;
		ProjectileTrapObj pto = p.GetComponent<ProjectileTrapObj>();
		pto.travelDir = this.transform.forward;
		pto.spawner = false;
		p.GetComponent<MeshRenderer>().enabled = true;
		p.transform.SetParent(this.transform.parent);
		p.transform.forward = this.transform.forward;
	}

	protected override void OnTriggerEnter(Collider c)
	{
		if(!spawner)
		{
			this.HitObject(c.transform);
		}
	}

	protected override void HitObject(Transform t)
	{
		if(t.gameObject.tag == "Player")
		{
			//Debug.Log("hit player!!!");
			t.GetComponent<PlayerBase>().takeDamage(this.damage);
			this.trapEffect(t.gameObject);
			Destroy(this.gameObject);
		}
		if(t.name.Contains("Wall"))
		{
			//Debug.Log("hit something else!!!");
			Destroy(this.gameObject);
		}
	}
}
