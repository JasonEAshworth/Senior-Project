using UnityEngine;
using System.Collections;

public class TriggerController : MonoBehaviour
{
	//list of objects to affect
	public GameObject[] obj;
	//whether the trigger can be used
	public bool on;
	//whether the trigger can be used multiple times
	public bool multi;
	//two positions for the trigger to allow two different actions
	public bool state;
	//# of players that need to be within the trigger's collider
	protected int playersIn = 0;
	//# of players (or objects) to activate the trigger
	public int playersNeeded;
	//# of seconds before the trigger can be used again
	protected float coolDownMax = 1.5f;
	//# of seconds since the trigger was used
	protected float coolDown = 0.0f;
	//whether the trigger is in cool down
	protected bool inCD = false;

	public void Trigger()
	{
		if(on && !inCD)
		{
			for(int i = 0; i < obj.Length; i++)
			{
				//"activateTrigger" can be changed and is simply the
				//name of the function that will be called on obj[i]
				obj[i].SendMessage("ActivateTrigger", state);
			}
			state = !state;
			if(!multi)
			{
				on = false;
			}
			inCD = true;
		}
	}

	public bool CanTrigger(Collider other)
	{
		bool ok = false;

		//follow this format
		if(other.gameObject.CompareTag("Player"))
		{
			if(gameObject.CompareTag("DoorTrigger"))
			{
				ok = true;
			}
		}

		return ok;
	}

	public void UpdateCoolDown()
	{
		if(inCD)
		{
			coolDown += Time.deltaTime;
			if(coolDown >= coolDownMax)
			{
				inCD = false;
				coolDown = 0;

				if(tag == "timedTrigger")
				{
					if(!state)
					{
						Trigger();
					}
				}
			}
		}
	}
}
