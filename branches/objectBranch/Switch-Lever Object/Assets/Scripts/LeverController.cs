using UnityEngine;
using System.Collections;

public class LeverController : MonoBehaviour
{
	public GameObject[] obj;
	private bool on = false;
	
	void MoveLever()
	{
		Debug.Log("lever controller!");
		if(on)
		{
			//need a better way to do this. maybe tags?
			GetComponentsInChildren<Transform>()[2].Rotate(0.0f, 0.0f, 60.0f);
		}
		else
		{
			//need a better way to do this. maybe tags?
			GetComponentsInChildren<Transform>()[2].Rotate(0.0f, 0.0f, -60.0f);
		}
		
		for(int i = 0; i < obj.Length; i++)
		{
			obj[i].SendMessage("move", on);
		}
		on = !on;
	}
}
