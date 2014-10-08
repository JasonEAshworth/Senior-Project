using UnityEngine;
using System.Collections;

public class Fireball : MonoBehaviour {

	public float speed = 15.0f;

	void Update () 
	{
		transform.position = transform.position + (transform.forward * speed * Time.deltaTime);
	}
	
	void OnCollisionEnter(Collision c)
	{
		//Unity Docs example on contact points
		ContactPoint contact = c.contacts[0];
		Quaternion rot = Quaternion.FromToRotation(Vector3.forward, contact.normal);
		Vector3 pos = contact.point;

		GameObject explosion = Instantiate (Resources.Load ("Prefabs/Character/Sorceress/Fireball_Explosion"), pos, rot) as GameObject;
		Destroy (gameObject);
	}
}
