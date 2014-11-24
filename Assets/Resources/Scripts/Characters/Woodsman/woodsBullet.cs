using UnityEngine;
using System.Collections;
using Exploder;

public class woodsBullet : MonoBehaviour {

	
	private float speed = 15.0f;
	public Vector3 playerForward;
	public GameObject woodsPlayer;
	private float timer;
	private float dmg = 7.5f;
	// Use this for initialization
	void Start () 
	{
		GameObject playerManager = GameObject.FindGameObjectWithTag("PlayerManager");
		PlayerManager playerManagerScript = playerManager.GetComponent<PlayerManager> ();
		Debug.Log (playerManagerScript.numPlayers);
		for (int i=0; i<playerManagerScript.numPlayers; i++)
		{
			if(playerManagerScript.players[i].GetComponent<PlayerBase>().classType == playerClass.WOODSMAN)
			{
				woodsPlayer = playerManagerScript.players[i];
			}
		}
		playerForward = woodsPlayer.transform.forward;
		transform.up = playerForward;
		timer = 1.0f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		transform.position = transform.position + (transform.up * speed * Time.deltaTime);
		timer = timer - Time.deltaTime;
		if (timer <= 0.0f) 
		{
			Destroy(gameObject);
		}
	}

	void OnCollisionEnter(Collision c)
	{
		if (c.collider.GetComponent<Explodable>() != null)
		{
			c.collider.SendMessage("Boom");
		}
		if (c.gameObject.CompareTag("Enemy"))
		{
			EnemyBase scr = c.gameObject.GetComponent<EnemyBase>();
			scr.takeDamage(dmg);
			Destroy(gameObject);
		}
		else if(c.gameObject.CompareTag("wall"))
		{
			Destroy (gameObject);
		}
	}
}
