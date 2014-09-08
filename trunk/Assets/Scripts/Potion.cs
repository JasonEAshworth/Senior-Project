using UnityEngine;
using System.Collections;

public class Potion : MonoBehaviour {

	public int potionValue = 20;

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
}
