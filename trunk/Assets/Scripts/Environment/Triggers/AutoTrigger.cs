using UnityEngine;
using System.Collections;

public class AutoTrigger : TriggerController
{
	public void OnTriggerEnter(Collider other)
	{
		//add if statements to restrict what objects can affect this trigger
		playersIn++;
		if(state && playersIn >= playersNeeded)
		{
			Trigger();
		}
	}

	public void OnTriggerExit(Collider other)
	{
		//add if statements to restrict what objects can affect this trigger
		playersIn--;
		//
		if(!state && playersIn < playersNeeded)
		{
			Trigger();
		}
	}

	public void Update()
	{
		UpdateCoolDown();
	}
}
