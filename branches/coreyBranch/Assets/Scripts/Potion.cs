using UnityEngine;
using System.Collections;

public class Potion : MonoBehaviour {

	public int potionValue = 20;

	void OnTriggerEnter(Collider player)
	{
		if (player.gameObject.CompareTag ("Player")) 
		{
			player.gameObject.SendMessage ("addItem", gameObject);
			gameObject.SetActive (false);
		}

	}
}
