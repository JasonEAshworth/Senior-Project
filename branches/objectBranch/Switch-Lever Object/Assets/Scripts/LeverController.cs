using UnityEngine;
using System.Collections;

public class LeverController : MonoBehaviour
{
	public GameObject[] obj;
	private bool on = true;
	
	void MoveLever()
	{
		Debug.Log("lever controller!");
		if(on)
		{
			GetComponentsInChildren<Transform>()[2].Rotate(0.0f, 0.0f, -60.0f);
			for(int i = 0; i < obj.Length; i++)
			{
				obj[i].SendMessage("move", on);
			}
			on = !on;
		}
		else
		{
			GetComponentsInChildren<Transform>()[2].Rotate(0.0f, 0.0f, 60.0f);
			for(int i = 0; i < obj.Length; i++)
			{
				obj[i].SendMessage("move", on);
			}
			on = !on;
		}
	}
}
