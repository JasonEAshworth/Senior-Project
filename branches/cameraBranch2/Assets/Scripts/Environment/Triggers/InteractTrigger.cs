using UnityEngine;
using System.Collections;

public class InteractTrigger : TriggerController
{
	//fires when the player quota is met
	//NOT the same as the AutoTrigger
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
}
