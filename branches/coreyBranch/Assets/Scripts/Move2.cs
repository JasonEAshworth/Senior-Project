using UnityEngine;
using System.Collections;

public class Move2 : MonoBehaviour
{
	void OnGUI()
	{
		if(Input.GetKeyDown("w"))
		{
			transform.Translate(2.5f, 0.0f, 1.0f);
		}
	}
}
