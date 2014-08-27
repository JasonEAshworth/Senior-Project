using UnityEngine;
using System.Collections;

public class Move4 : MonoBehaviour
{
	void OnGUI()
	{
		if(Input.GetKeyDown("r"))
		{
			transform.Translate(4.0f, 0.0f, -1.0f);
		}
	}
}
