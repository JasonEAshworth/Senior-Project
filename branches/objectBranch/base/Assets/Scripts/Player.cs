using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour 
{
	public float speed;

	private GameObject item;
	private float health = 100;

	void Update()
	{
		if (item) 
		{
			if (Input.GetKeyDown("space"))
			{
				switch(item.tag)
				{
					case "Potion":
						//Debug.Log (item.tag);
						health += item.GetComponent<Potion>().potionValue;
						item = null;
						break;
					case "AttackPotion":
						item = null;
						break;
					case "DefensePotion":
						item = null;
						break;
				}

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

	void addItem(GameObject p)
	{
		item = p;
	}
	
}
