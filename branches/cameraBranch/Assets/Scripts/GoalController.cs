using UnityEngine;
using System.Collections;

public class GoalController : MonoBehaviour
{
	protected int playersIn = 0;

	void OnTriggerEnter(Collider other)
	{
		Debug.Log("Goal enter");
		playersIn++;
		if(playersIn == 4)
		{
			Debug.Log("Move to the next room...");
		}
	}
	
	void OnTriggerExit(Collider other)
	{
		Debug.Log("Goal exit");
		playersIn--;
	}
}
