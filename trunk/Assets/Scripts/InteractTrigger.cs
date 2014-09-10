using UnityEngine;
using System.Collections;

public class InteractTrigger : TriggerController
{
	//these can be used for things like levers that require more than one player to activate
	//NOT to be confused with the coolDown variables in the TriggerController

	//these only matter if playersNeeded > 1
	//# of seconds since the first player pressed the interact button
	private float timeOut = 0.0f;
	//timeframe within playersIn needs to reach playersNeeded
	private float timeOutMax = 1.0f;
	//whether the trigger is waiting for additional players to press the interact button
	private bool inTO = false;

	public void onTriggerStay(Collider other)
	{
		//add if statements to restrict what objects can affect this trigger
		if(other.tag == "Player")
		{
			PlayerBase pb = other.GetComponent<PlayerBase>();
			//special attack needs to be replaced with interact
			if(Input.GetKeyDown(pb.specialAttackKey))
			{
				if(playersIn == 0)
				{
					inTO = true;
				}
				playersIn++;
				if(playersIn >= playersNeeded)
				{
					Trigger();
					timeOut = 0.0f;
					inTO = false;
				}
			}
		}
	}

	public void Update()
	{
		if(inTO)
		{
			timeOut += Time.deltaTime;
			if(timeOut >= timeOutMax)
			{
				inTO = false;
				timeOut = 0;
				playersIn = 0;
			}
		}
		UpdateCoolDown();
	}
}
