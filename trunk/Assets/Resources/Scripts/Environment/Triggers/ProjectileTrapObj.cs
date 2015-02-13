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
	//public Vector3 travelDir = Vector3.zero;

	protected void OnEnable()
	{
		StartCoroutine(WaitToDisable());
	}

	/*protected void Start()
	{
		if(!spawner)
		{
			StartCoroutine(WaitToDisable());
		}
	}*/

	protected void FixedUpdate()
	{
		//Vector3 start = this.transform.position;
		//this.transform.position += this.travelDir * this.travelSpeed * Time.deltaTime;
		this.transform.position += this.transform.forward * this.travelSpeed * Time.deltaTime;
		//Vector3 end = this.transform.position;
	}

	protected IEnumerator WaitToDisable()
	{
		yield return new WaitForSeconds(this.lifetime);
		this.gameObject.SetActive(false);
	}

	//protected override void ActivateTrigger(bool state)
	//{
		/*GameObject p = Instantiate(this.gameObject, this.transform.position, this.transform.rotation) as GameObject;
		p.GetComponent<MeshRenderer>().enabled = true;
		p.transform.SetParent(this.transform.parent);
		ProjectileTrapObj pto = p.GetComponent<ProjectileTrapObj>();
		//pto.travelDir = this.transform.forward;
		//pto.spawner = false;
		pto.enabled = true;
		//p.transform.forward = this.transform.forward;*/
	//}

	protected override void OnTriggerEnter(Collider c)
	{
		this.HitObject(c.transform);
	}

	protected override void HitObject(Transform t)
	{
		if(t.gameObject.tag == "Player")
		{
			t.GetComponent<PlayerBase>().takeDamage(this.damage);
			this.trapEffect(t.gameObject);
			this.gameObject.SetActive(false);
		}
		else if(t.name.Contains("Wall"))
		{
			this.gameObject.SetActive(false);
		}
	}
}
