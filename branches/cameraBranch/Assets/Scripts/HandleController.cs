using UnityEngine;
using System.Collections;

public class HandleController : MonoBehaviour
{
	void move(bool on)
	{
		if(on)
		{
			this.transform.Rotate(0.0f, 0.0f, 60.0f);
		} 
		else
		{
			this.transform.Rotate(0.0f, 0.0f, -60.0f);
		}
	}
}
