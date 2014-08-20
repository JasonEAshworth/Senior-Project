using UnityEngine;
using System.Collections;

public class Potion : MonoBehaviour {

	public float potionValue = 20.0f;

	void Update ()
	{
		transform.Rotate (new Vector3 (15, 30, 45) * Time.deltaTime);
	}

	public void UseItem()
	{
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		player.SendMessage ("addHealth", potionValue);
	}
}
