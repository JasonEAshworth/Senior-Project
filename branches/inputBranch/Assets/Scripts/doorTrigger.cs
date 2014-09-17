using UnityEngine;
using System.Collections;

public class doorTrigger : MonoBehaviour {

	void OnTriggerEnter(Collider player)
	{
		if (player.gameObject.CompareTag ("Player"))
		{
			bool pManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>().haveKey;

			if(pManager)
			{
				GameObject door = GameObject.Find ("door");
				Destroy(door);
				pManager = false;
			}
		}
	}
}
