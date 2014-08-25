using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
	void FixedUpdate ()
	{
		float x = Input.GetAxis("Horizontal");
		float z = Input.GetAxis("Vertical");
		Vector3 movement = new Vector3(x, 0.0f, z);
		rigidbody.AddForce(movement * 500 * Time.deltaTime);
	}
	
	void OnTriggerStay(Collider other)
	{
		if(other.gameObject.tag == "Lever")
		{
			if(Input.GetKeyDown("space"))
			{
				other.SendMessage("MoveLever");
			}
		}
	}
}
