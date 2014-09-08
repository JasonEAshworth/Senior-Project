using UnityEngine;
using System.Collections;

public class woodsBullet : MonoBehaviour {


	public Vector3 playerForward;
	private float speed = 15.0f;
	private GameObject woodsPlayer;
	// Use this for initialization
	void Start () 
	{
		/*GameObject playerManager = GameObject.FindGameObjectWithTag("PlayerManager");
		PlayerManager playerManagerScript = playerManager.GetComponent<PlayerManager> ();
		for (int i=0; i<playerManagerScript.numPlayers; i++)
		{
			if(playerManagerScript.players[i].GetComponent<PlayerBase>().classType == playerClass.WOODSMAN)
			{
				woodsPlayer = playerManagerScript.players[i];
			}
		}
		playerForward = woodsPlayer.transform.forward;
		transform.forward = playerForward;*/
	}
	
	// Update is called once per frame
	void Update () 
	{
		transform.position = transform.position + (transform.up * speed * Time.deltaTime);
	}

	void OnCollisionEnter(Collision c)
	{
		Destroy (gameObject);
	}
}
