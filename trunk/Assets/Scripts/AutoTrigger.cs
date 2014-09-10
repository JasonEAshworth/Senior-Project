using UnityEngine;
using System.Collections;

public class AutoTrigger : TriggerController
{
	public void onTriggerEnter(Collider other)
	{
		//add if statements to restrict what objects can affect this trigger
		playersIn++;
		Trigger();
	}

	public void onTriggerExit(Collider other)
	{
		//add if statements to restrict what objects can affect this trigger
		playersIn--;
	}

	public void Update()
	{
		UpdateCoolDown();
	}
}
