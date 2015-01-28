using UnityEngine;
using System.Collections;

public class PotionBase : MonoBehaviour 
{

	void Update ()
	{
		transform.Rotate (new Vector3 (0, 30, 0) * Time.deltaTime);
	}

	void OnTriggerEnter(Collider player)
	{
		if (player.gameObject.CompareTag ("Player")) 
		{
			player.gameObject.SendMessage ("addItem", gameObject);
			gameObject.SetActive (false);
		}
	}

	public virtual void itemAbility(PlayerBase p)
	{

	}
}
