using UnityEngine;
using System.Collections;

public enum PotionType
{
	NONE,
	HEALTH,
	HASTE
}

public class PotionBase : MonoBehaviour 
{	
	public PotionType type;	// set in the inspector to assign potion type

	void Update()
	{
		transform.Rotate (new Vector3 (0, 30, 0) * Time.deltaTime);
	}

	void OnTriggerEnter(Collider player)
	{
		if (player.gameObject.CompareTag("Player")) 
		{
			player.gameObject.SendMessage("addItem", type);
			Destroy(gameObject);
		}
	}
}
