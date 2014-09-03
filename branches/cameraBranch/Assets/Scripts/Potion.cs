using UnityEngine;
using System.Collections;

public class Potion : PickUps {

	public float potionValue = 20.0f;

	void OnTriggerEnter(Collider player)
	{
		player.gameObject.SendMessage ("addItem", gameObject);
		gameObject.SetActive (false);
	}
}
