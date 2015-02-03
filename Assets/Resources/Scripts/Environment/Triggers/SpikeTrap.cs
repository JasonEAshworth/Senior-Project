using UnityEngine;
using System.Collections;

public class SpikeTrap : TrapBase
{
	//while the SpikeTrap uses the spawner attribute, it doesn't actually spawn a new SpikeTrap
	//spawner == true means it's retracting
	//spawner == false means it's advancing 
	private Vector3 startPos = Vector3.zero;
	private Vector3 endPos = Vector3.zero;
	//public Vector3 travelDir = Vector3.zero;

	public void Start()
	{
		//this.travelDir = this.transform.up;
		this.transform.up = Vector3.up;
		this.startPos = this.transform.position;
		//this.endPos = this.startPos + this.travelDir * this.GetComponent<BoxCollider>().bounds.size.y * this.transform.localScale.y * 1.3f;
		this.endPos = this.startPos + this.transform.up * this.GetComponent<BoxCollider>().bounds.size.y * this.transform.localScale.y * 1.3f;
		//this.transform.up = this.travelDir;
	}

	protected void FixedUpdate()
	{
		Vector3 start = this.transform.position;
		if(!spawner)
		{
			//this.transform.position += this.travelDir * 3.0f * Time.deltaTime;
			this.transform.position += this.transform.up * 3.0f * Time.deltaTime;
			if((this.transform.position - this.startPos).magnitude > (this.endPos - this.startPos).magnitude)
			{
				this.transform.position = this.endPos;
			}
		}
		else
		{
			//this.transform.position += this.travelDir * -3.0f * Time.deltaTime;
			this.transform.position += this.transform.up * -3.0f * Time.deltaTime;
			if((this.transform.position - this.endPos).magnitude > (this.startPos - this.endPos).magnitude)
			{
				this.transform.position = this.startPos;
			}
		}
	}

	protected override void ActivateTrigger(bool state)
	{
		if(state)
		{
			if(spawner)
			{
				this.spawner = false;
			}
		}
		else
		{
			if(!spawner)
			{
				this.spawner = true;
			}
		}
	}

	protected override void OnTriggerEnter(Collider c)
	{
		this.HitObject(c.transform);
	}

	protected void OnTriggerStay(Collider c)
	{
		this.HitObject(c.transform);
	}

	protected override void HitObject(Transform t)
	{
		if(t.gameObject.tag == "Player")
		{
			t.GetComponent<PlayerBase>().takeDamage(this.damage);
			this.trapEffect(t.gameObject);
		}
		if(t.name.Contains("Wall"))
		{
		}
	}
}
