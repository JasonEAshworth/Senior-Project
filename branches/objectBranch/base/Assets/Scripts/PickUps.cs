using UnityEngine;
using System.Collections;

public class PickUps : MonoBehaviour {
	
	// Update is called once per frame
	void Update ()
	{
		transform.Rotate (new Vector3 (15, 30, 45) * Time.deltaTime);
	}

	void OnTriggerEnter(Collider player)
	{
		player.gameObject.SendMessage ("addItem", gameObject);
		gameObject.SetActive (false);
	}	
}