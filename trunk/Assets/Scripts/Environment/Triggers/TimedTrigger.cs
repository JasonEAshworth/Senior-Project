using UnityEngine;
using System.Collections;

public class TimedTrigger : TriggerController
{
	//continously fires
	void Update()
	{
		if(!inCD)
		{
			Trigger();
		}
	}
}
