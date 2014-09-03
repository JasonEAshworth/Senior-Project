using UnityEngine;
using System.Collections;

public class Move3 : MonoBehaviour
{
	void OnGUI()
	{
		if(Input.GetKeyDown("e"))
		{
			transform.Translate(4.0f, 0.0f, 1.0f);
		}
	}
}
