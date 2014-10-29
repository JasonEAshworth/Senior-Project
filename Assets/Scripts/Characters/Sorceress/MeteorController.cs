using UnityEngine;
using System.Collections;

public class MeteorController : MonoBehaviour {

	private ParticleSystem p;
	public float rotVel = 45.0f;
	private Quaternion r;

	void Start(){
		r = transform.rotation;
	}

	void FixedUpdate(){
		/*float x = rotVel * Time.deltaTime;
		GameObject z = transform.GetChild (0).gameObject;

		transform.Rotate(x, x, x, Space.Self);
		z.transform.Rotate(-1 * x, -1 * x, -1 * x);*/
	}

	// Update is called once per frame
	void OnCollisionEnter(Collision c){
		//this is where we put mesh exploder
		p = transform.parent.gameObject.GetComponentInChildren<ParticleSystem> ();
		p.loop = false;
		Destroy (gameObject, 2.0f);
		Destroy (transform.parent.gameObject, 5.0f);
				
	}
}
