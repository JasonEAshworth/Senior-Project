using UnityEngine;
using System.Collections;

public class KingFirestorm : MonoBehaviour 
{
	private float firestormDamage = 10.0f;

	void FixedUpdate()
	{
		Vector3 targetRotation = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + 20.0f, transform.eulerAngles.z);
		transform.eulerAngles = Vector3.RotateTowards(transform.eulerAngles, targetRotation, 0.2f, 0.2f);
	}

	void OnTriggerStay(Collider c)
	{
		if (c.tag == "Player")
		{
			c.SendMessage("takeDamage", firestormDamage);
		}
	}
}
