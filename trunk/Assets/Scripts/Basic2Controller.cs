using UnityEngine;
using System.Collections;

public class Basic2Controller : MonoBehaviour {
	
	void move(bool on)
	{
		Debug.Log("Basic2 controller!");
		if(on)
		{
			transform.Translate(new Vector3(2.0f, 0.0f, -3.0f));
		}
		else
		{
			transform.Translate(new Vector3(-2.0f, 0.0f, 3.0f));
		}
	}
}
