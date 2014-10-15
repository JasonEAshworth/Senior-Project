using UnityEngine;
using System.Collections;

public class AutoTrigger : TriggerController
{
	public void OnTriggerEnter(Collider other)
	{
		if(CanTrigger(other))
		{
			playersIn++;
			if(state && playersIn >= playersNeeded)
			{
				Trigger();
			}
		}
	}

	public void OnTriggerExit(Collider other)
	{
		if(CanTrigger(other))
		{
			playersIn--;
			if(!state && playersIn < playersNeeded)
			{
				Trigger();
			}
		}
	}

	public void Update()
	{
		UpdateCoolDown();
	}
}
