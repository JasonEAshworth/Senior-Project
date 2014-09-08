using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour {

	public Transform checkpoint;
	
	void OnTriggerEnter(Collider col){
		if (col.tag == "Player")
		{
			checkpoint.position = transform.position;
		}
	}
}
