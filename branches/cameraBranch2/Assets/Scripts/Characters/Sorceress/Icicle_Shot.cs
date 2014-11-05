using UnityEngine;
using System.Collections;

public class Icicle_Shot : MonoBehaviour {

	public float velocity = 45.0f;
	public float rotVel = 45.0f;

	void Update () {
		transform.Rotate (0, rotVel * Time.deltaTime, 0, Space.Self);
		transform.position = transform.position + (transform.up * velocity * Time.deltaTime);
	}

	void onCollisionEnter(Collision c){
		if (c.gameObject.CompareTag ("wall")) 
		{
				//THIS IS WHERE WE NEED A MESH EXPLODER
				Destroy (gameObject);
		} 
		else if (c.gameObject.CompareTag ("Enemy")) 
		{
			c.gameObject.SendMessage("TakeDamage", 0.1f);
		}
	}
}
