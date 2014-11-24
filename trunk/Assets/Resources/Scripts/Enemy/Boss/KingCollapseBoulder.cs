using UnityEngine;
using System.Collections;

public class KingCollapseBoulder : MonoBehaviour 
{
	private float boulderDamage = 20.0f;
	private float fallSpeed = 3.0f;

	void OnTriggerEnter(Collider c)
	{
		if (c.tag == "Player")
		{
			c.SendMessage("takeDamage", boulderDamage);
		}
		if (c.tag == "Player" || c.gameObject.layer == LayerMask.GetMask("Default"))
		{
			Destroy(gameObject);
		}
	}
}
