using UnityEngine;
using System.Collections;

public class KingCollapseBoulder : MonoBehaviour 
{
	private float fallSpeed = 3.0f;

	void OnTriggerEnter(Collider c)
	{
		if (c.tag == "Player")
		{

		}
	}
}
