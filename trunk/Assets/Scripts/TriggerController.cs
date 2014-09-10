using UnityEngine;
using System.Collections;

public class TriggerController : MonoBehaviour
{
	public GameObject[] obj;
	public bool on;
	public bool multi;
	public bool state;
	protected int playersIn = 0;
	public int playersNeeded;
	protected float coolDownMax = 1.5f;
	protected float coolDown = 0.0f;
	protected bool inCD = false;

	public void UpdateCoolDown()
	{
		if(inCD)
		{
			coolDown += Time.deltaTime;
			if(coolDown >= coolDownMax)
			{
				inCD = false;
				coolDown = 0;
			}
		}
	}

	public void Trigger()
	{
		if(on && !inCD)
		{
			if(playersIn >= playersNeeded && coolDown < 0)
			{
				for(int i = 0; i < obj.Length; i++)
				{
					obj[i].SendMessage("move", state);
				}
				state = !state;
				if(!multi)
				{
					on = false;
				}
			}
		}
	}
}
