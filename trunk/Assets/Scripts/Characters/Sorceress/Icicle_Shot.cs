using UnityEngine;
using System.Collections;

public class Icicle_Shot : MonoBehaviour {

	public float velocity = 15.0f;
	public float rotVel = 45.0f;

	void Update () {
		transform.Rotate (0, rotVel * Time.deltaTime, 0);
		transform.position += (transform.up * velocity * Time.deltaTime);
	}

	void onCollisionEnter(Collision c){

	}
}
