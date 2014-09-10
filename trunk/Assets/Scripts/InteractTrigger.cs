using UnityEngine;
using System.Collections;

public class InteractTrigger : TriggerController
{
	//these can be used for things like levers that require more than one player to activate
	//NOT to be confused with the coolDown variables in the TriggerController
	private float timeOut = 0.0f;
	private float timeOutMax = 1.0f;
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
				Trigger();
				if(playersIn >= playersNeeded)
				{
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
