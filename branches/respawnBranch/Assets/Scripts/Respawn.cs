using UnityEngine;
using System.Collections;

public class Respawn : MonoBehaviour {

	public Transform spawns;

	GameObject Player;

	void OnTriggerEnter(Collider col){
		if (col.tag == "Player")
		{
			Player=col.gameObject;
			Player.transform.position = spawns.position;
		}
	}
}
