using UnityEngine;
using System.Collections;

public class AutoTrigger : TriggerController
{
	//fires when the player quota is met
	//fires again when it no longer meets the quota
	public void OnTriggerEnter(Collider other)
	{
		if(CanTrigger(other) && other.tag == "Player")
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
		if(CanTrigger(other) && other.tag == "Player")
		{
			playersIn--;
			if(!state && playersIn < playersNeeded)
			{
				Trigger();
			}
		}
	}
}
