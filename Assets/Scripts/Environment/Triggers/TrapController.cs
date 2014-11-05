using UnityEngine;
using System.Collections;

public class TrapController : MonoBehaviour
{
	// not a self reference, but a prefab that the trap instantiates upon activation
	// for example, an arrow from an arrow trap or spikes for a spike trap
	public GameObject trapObject;

	public void ActivateTrigger(bool state)
	{
		Debug.Log("trap start");
		if(trapObject.GetComponent<ProjectileTrapObj>())
		{
			//fix this stuff
			Debug.Log("p trap");
			GameObject p = Instantiate(trapObject, this.transform.position, Quaternion.identity) as GameObject;
			p.GetComponent<ProjectileTrapObj>().travelDir  = this.transform.forward;
		}
		else if (trapObject.GetComponent<DurationTrapObj>())
		{
			Debug.Log("d trap");
			DurationTrapObj d = Instantiate(trapObject, this.transform.position, Quaternion.identity) as DurationTrapObj;
		}
	}
}
