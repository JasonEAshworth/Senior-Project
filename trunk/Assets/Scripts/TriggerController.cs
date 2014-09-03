using UnityEngine;
using System.Collections;

public class TriggerController : MonoBehaviour
{
	public GameObject[] obj;
	private bool on = false;
	private int playersIn = 0;
	public int playersNeeded;
	
	void Trigger()
	{
		Debug.Log("trigger controller!");
		for(int i = 0; i < obj.Length; i++)
		{
			obj[i].SendMessage("move", on);
		}
		on = !on;
	}

	void TriggerOnQuota(bool enter)
	{
		if(enter)
		{
			playersIn++;
		}
		else
		{
			playersIn--;
		}

		if(playersIn >= playersNeeded)
		{
			Trigger();
		}
	}
}
