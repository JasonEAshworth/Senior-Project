using UnityEngine;
using System.Collections;

public class InteractTrigger : TriggerController
{
	public void OnTriggerEnter(Collider other)
	{
		if(CanTrigger(other))
		{
			playersIn++;
			if(playersIn >= playersNeeded)
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
		}
	}

	public void Update()
	{
		UpdateCoolDown();
	}
}
