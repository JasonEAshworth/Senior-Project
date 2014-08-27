using UnityEngine;
using System.Collections;

public class LeverController : MonoBehaviour
{
	public GameObject[] obj;
	private bool on = false;
	
	void MoveLever()
	{
		Debug.Log("lever controller!");
		for(int i = 0; i < obj.Length; i++)
		{
			obj[i].SendMessage("move", on);
		}
		on = !on;
	}
}
