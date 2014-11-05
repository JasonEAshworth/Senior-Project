using UnityEngine;
using System.Collections;

public class Coin : MonoBehaviour {
	
	public int goldValue = 1;
	
	void Update ()
	{
		transform.Rotate (new Vector3 (0, 0, 30) * Time.deltaTime);
	}
	
	void OnTriggerEnter(Collider player)
	{
		if (player.gameObject.CompareTag ("Player")) 
		{
			//player.gameObject.SendMessage ("addGold", gameObject);
			Destroy(gameObject);
		}
		
	}
}
