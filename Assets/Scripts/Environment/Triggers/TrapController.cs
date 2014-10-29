using UnityEngine;
using System.Collections;

public class TrapController : MonoBehaviour
{
	// not a self reference, but a prefab that the trap instantiates upon activation
	// for example, an arrow from an arrow trap or spikes for a spike trap
	public GameObject trapObject;

	public void ActivateTrigger(bool state)
	{
		if(trapObject.GetComponent<ProjectileTrapObj>())
		{
			GameObject p = Instantiate(trapObject, this.transform.position, Quaternion.identity) as GameObject;
			ProjectileTrapObj pto = p.GetComponent<ProjectileTrapObj>();
			pto.travelDir  = this.transform.forward;
			pto.spawner  = false;
			//only works if the prefabs scale is (1,1,1)
			p.transform.localScale = Vector3.one;
			p.tag = "Untagged";
			p.transform.SetParent(this.transform);
		}
		else if(trapObject.GetComponent<SpikeTrap>())
		{
			if(state)
			{
				GameObject d = Instantiate(trapObject, this.transform.position, Quaternion.identity) as GameObject;
				SpikeTrap st = d.GetComponent<SpikeTrap>();
				st.travelDir = this.transform.up;
				st.spawner = false;
				d.transform.localScale = Vector3.one;
				d.tag = "Untagged";
				d.transform.SetParent(this.transform);
			}
			else
			{
				foreach(Transform tr in this.transform)
				{
					if(tr.GetComponent<SpikeTrap>() && tr.tag != "TrapSpawn")
					{
						Destroy(tr.gameObject);
						break;
					}
				}
			}
		}
	}
}
