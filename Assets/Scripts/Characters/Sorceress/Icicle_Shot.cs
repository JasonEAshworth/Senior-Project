using UnityEngine;
using System.Collections;

public class Icicle_Shot : MonoBehaviour {

	public float velocity = 45.0f;
	public float rotVel = 45.0f;

	void Start(){
		Destroy (gameObject, 5.0f);
	}

	void Update () {
		//transform.Rotate (0, rotVel * Time.deltaTime, 0, Space.Self);
		transform.position += (transform.up * velocity * Time.deltaTime);
	}

	/*void OnCollisionEnter(Collision c){
		if (c.gameObject.CompareTag ("Enemy")) {
			Debug.Log ("hit something");
			c.gameObject.SendMessage ("takeDamage", 10.0f);
			c.gameObject.SendMessage ("slow");
			Destroy (gameObject);
		} 
		else {
			Destroy(gameObject);
		}
	}*/

	void OnTriggerEnter(Collider c){
		if (c.gameObject.CompareTag ("Enemy")) {
			c.gameObject.SendMessage ("takeDamage", 10.0f);
			//c.gameObject.SendMessage ("slow");
			Destroy (gameObject);
		} 
	}
}
