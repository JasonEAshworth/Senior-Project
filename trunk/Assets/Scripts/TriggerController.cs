using UnityEngine;
using System.Collections;

public class TriggerController : MonoBehaviour
{
	public GameObject[] obj;
	public bool active;
	public bool multi;
	public bool state;
	private int playersIn = 0;
	public int playersNeeded;
	private float cooldown = 1.5f;
	private float elapsed = 0;
	private bool inCD = false;

	void Update()
	{
		if(inCD)
		{
			elapsed += Time.deltaTime;
		}
		if(elapsed >= cooldown)
		{
			inCD = false;
			elapsed = 0;
		}
	}

	void Trigger(bool enter)
	{
		if(active && !inCD)
		{
			if(enter)
			{
				playersIn++;
			}
			else
			{
				playersIn--;
			}

			if(playersIn >= playersNeeded && elapsed < 0)
			{
				for(int i = 0; i < obj.Length; i++)
				{
					obj[i].SendMessage("move", state);
				}
				state = !state;
				if(!multi)
				{
					active = false;
				}
			}
		}
	}
}
