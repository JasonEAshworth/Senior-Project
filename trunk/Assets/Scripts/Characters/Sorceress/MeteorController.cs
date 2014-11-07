using UnityEngine;
using System.Collections;

public class MeteorController : MonoBehaviour {

	private ParticleSystem p;
	public float rotVel = 45.0f;

	void OnTriggerEnter(Collider c){
		Debug.Log ("Meteor Collision");
		p = transform.parent.gameObject.GetComponentInChildren<ParticleSystem> ();
		p.loop = false;
		Destroy (gameObject);
		Destroy (transform.parent.gameObject, 5.0f);
	}


}
