using UnityEngine;
using System.Collections;

public class woodsBullet : MonoBehaviour {

	
	private float speed = 15.0f;
	public Vector3 playerForward;
	public GameObject woodsPlayer;
	// Use this for initialization
	void Start () 
	{
		GameObject playerManager = GameObject.FindGameObjectWithTag("PlayerManager");
		PlayerManager playerManagerScript = playerManager.GetComponent<PlayerManager> ();
		for (int i=0; i<playerManagerScript.numPlayers; i++)
		{
			if(playerManagerScript.players[i].GetComponent<PlayerBase>().classType == playerClass.WOODSMAN)
			{
				woodsPlayer = playerManagerScript.players[i];
			}
		}
		playerForward = woodsPlayer.transform.forward;
		transform.up = playerForward;
	}
	
	// Update is called once per frame
	void Update () 
	{
		Debug.Log (transform.up);
		transform.position = transform.position + (transform.up * speed * Time.deltaTime);
	}

	void OnCollisionEnter(Collision c)
	{
		if (c.gameObject.CompareTag("Enemy") || c.gameObject.CompareTag("wall"))
		{
			Destroy (gameObject);
		}
	}
}
