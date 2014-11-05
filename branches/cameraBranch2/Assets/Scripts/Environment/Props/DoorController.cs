using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour {

	public void ActivateTrigger(bool state)
	{
		PlayerManager pm = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
		if(pm.haveKey)
		{
			pm.haveKey = false;
			Destroy(this.gameObject);
		}
	}
}
