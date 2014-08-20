using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour 
{
	public float speed;

	private GameObject item;
	private float health;
	public Potion p;

	void Update()
	{
		Debug.Log ("Health: " + health.ToString());
		if (Input.GetKeyDown ("space")) 
		{
			if(item)
			{
				p = item.GetComponent("Potion") as Potion;
				health += p.potionValue;
				Destroy(item);
			}
		}
	}

	void FixedUpdate()
	{
		float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");

		Vector3 move = new Vector3 (moveHorizontal, 0.0f, moveVertical);

		rigidbody.AddForce(move * speed * Time.deltaTime);
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Potion") 
		{
			item = other.gameObject;
			p = item.GetComponent("Potion") as Potion;
			other.gameObject.SetActive (false);
			Debug.Log ("Item attached: " + item.name);
		} 
	}


}
