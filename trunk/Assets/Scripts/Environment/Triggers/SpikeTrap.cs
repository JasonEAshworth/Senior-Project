using UnityEngine;
using System.Collections;

public class SpikeTrap : TrapBase
{
	public float travelDist = 0.0f;
	private Vector3 startPos = Vector3.zero;
	public Vector3 travelDir = Vector3.zero;

	public void Start()
	{
		this.startPos = this.transform.position;
	}

	protected override void FixedUpdate()
	{
		if(!spawner)
		{
			Vector3 start = this.transform.position;
			//need to do this better
			if((this.transform.position - this.startPos).magnitude < travelDist)
			{
				this.transform.position += this.travelDir * 3.0f * Time.deltaTime;
			}

			Vector3 end = this.transform.position;
			Collider cc = this.GetComponent<Collider>();
			Vector3 sc = this.transform.localScale;
			Vector3 size = cc.bounds.size;
			float radius = Mathf.Pow(Mathf.Max(size.x, size.y, size.z), 2) * 0.5f * Mathf.Max(sc.x, sc.y, sc.z);
			//min and max aren't quite correct?
			Vector3 p1 = cc.bounds.min + this.travelDir * radius;
			Vector3 p2 = cc.bounds.max - this.travelDir * radius;
			RaycastHit[] rh = Physics.CapsuleCastAll(p1, p2, radius, this.travelDir, Mathf.Min(Vector3.Distance(start, end), 0.5f));

			foreach(RaycastHit hit in rh)
			{
				this.HitObject(hit.transform);
			}
		}
	}

	protected override void OnTriggerEnter(Collider c)
	{
		if(!spawner)
		{
			this.HitObject(c.transform);
		}
	}

	protected void OnTriggerStay(Collider c)
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
			Debug.Log("hit player!!!");
			t.GetComponent<PlayerBase>().takeDamage(this.damage);
			this.trapEffect(t.gameObject);
		}
		if(t.name.Contains("Wall"))
		{
			Debug.Log("hit something else!!!");
		}
	}
}
