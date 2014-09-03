using UnityEngine;
using System.Collections;

public class Move1 : MonoBehaviour
{
	void OnGUI()
	{
		if(Input.GetKeyDown("q"))
		{
			transform.Translate(2.5f, 0.0f, -1.0f);
		}
	}
}
