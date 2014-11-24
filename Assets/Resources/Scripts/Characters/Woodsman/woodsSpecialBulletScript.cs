using UnityEngine;
using System.Collections;
using Exploder;

public class woodsSpecialBulletScript : MonoBehaviour {
	
	private float speed = 15.0f;
	private GameObject woodsPlayer;
	public int numPiercing = 0;
	public float heldTime = 0.0f;
	private bool infinitePierce = false;
	public Vector3 playerForward;
	private float dmg = 25.0f;
	private float timer = 7.0f;
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
		transform.up = new Vector3(playerForward.x, playerForward.y, playerForward.z);
		if (heldTime > 5.0f) 
		{
			infinitePierce = true;
		} 
		else 
		{
			numPiercing = Mathf.FloorToInt(heldTime);
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		transform.position = transform.position + (transform.up * speed * Time.deltaTime);
		timer -= Time.deltaTime;
		if(timer <= 0.0f)
		{
			Destroy (gameObject);
		}
	}
	
	void OnCollisionEnter(Collision c)
	{
		Debug.Log ("hi");
		if (c.collider.GetComponent<Explodable>() != null)
		{
			c.collider.SendMessage("Boom");
		}
		if(c.gameObject.CompareTag("wall"))
		{
			Destroy (gameObject);
		}
		if(c.gameObject.CompareTag("Enemy"))
		{
			Debug.Log ("hit enemy");
			if(infinitePierce == false)
			{
				if(numPiercing >0)
				{
					numPiercing = numPiercing -1;
					EnemyBase scr = c.gameObject.GetComponent<EnemyBase>();
					scr.takeDamage(dmg);
				}
				else
				{
					EnemyBase scr = c.gameObject.GetComponent<EnemyBase>();
					scr.takeDamage(dmg);
					Destroy(gameObject);
				}
			}
			else
			{
				EnemyBase scr = c.gameObject.GetComponent<EnemyBase>();
				scr.takeDamage(dmg);
			}
		}
	}
}

