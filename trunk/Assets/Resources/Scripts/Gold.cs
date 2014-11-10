using UnityEngine;
using System.Collections;

public class Gold : MonoBehaviour {
	
	public int goldValue = 1;
	
	void Update ()
	{
		transform.Rotate (new Vector3 (0, 30, 0) * Time.deltaTime);
	}
	
	void OnTriggerEnter(Collider player)
	{
		if (player.gameObject.CompareTag ("Player")) 
		{	
			player.gameObject.SendMessage ("addScore", gameObject);
			Destroy(gameObject);
		}
		
	}
}
