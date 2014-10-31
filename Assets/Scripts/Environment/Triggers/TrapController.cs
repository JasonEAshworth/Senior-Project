using UnityEngine;
using System.Collections;

public class TrapController : MonoBehaviour
{
	// not a self reference, but a prefab that the trap instantiates upon activation
	// for example, an arrow from an arrow trap or spikes for a spike trap
	public GameObject trapObject;

	//a list of currently active traps
	public ArrayList traps = new ArrayList();

	public void ActivateTrigger(bool state)
	{
		if(trapObject.GetComponent<ProjectileTrapObj>())
		{
			GameObject p = Instantiate(trapObject, this.transform.position, Quaternion.identity) as GameObject;
			ProjectileTrapObj pto = p.GetComponent<ProjectileTrapObj>();
			pto.travelDir  = this.transform.forward;
			pto.spawner  = false;
			p.GetComponent<MeshRenderer>().enabled = true;
			p.transform.localScale = Vector3.one;
			p.transform.SetParent(this.transform);
			this.traps.Add(p);
		}
		else if(trapObject.GetComponent<SpikeTrap>())
		{
			if(state)
			{
				GameObject d = Instantiate(trapObject, this.transform.position, Quaternion.identity) as GameObject;
				SpikeTrap st = d.GetComponent<SpikeTrap>();
				st.travelDir = this.transform.up;
				st.spawner = false;
				d.GetComponent<MeshRenderer>().enabled = true;
				d.transform.localScale = Vector3.one;
				d.transform.SetParent(this.transform);
				this.traps.Add(d);
			}
			else
			{
				GameObject go = this.traps[0] as GameObject;
				Destroy(go);
				this.traps.Remove(go);
			}
		}
	}
}
