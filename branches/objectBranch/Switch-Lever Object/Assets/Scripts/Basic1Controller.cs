using UnityEngine;
using System.Collections;

public class Basic1Controller : MonoBehaviour
{
	void move(bool on)
	{
		Debug.Log("Basic1 controller!");
		if(on)
		{
			transform.Translate(new Vector3(0.0f, 5.0f, 0.0f));
		}
		else
		{
			transform.Translate(new Vector3(0.0f, -5.0f, 0.0f));
		}
	}
}
