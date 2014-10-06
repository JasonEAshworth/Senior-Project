using UnityEngine;
using System.Collections;

public class HawkAI2 : MonoBehaviour {
	
	
	public int mode;
	public GameObject woodsman;
	// a vector from the hawk to the player to use to increase radius of the idle rotation
	private Vector3 paddingVector;
	private Vector3 facingVector;
	private float timer = 0.0f;
	private float speed = 8.0f;
	private Vector3 initialPoint;
	private bool arrived = false;
	private bool arrivedPerch = false;
	private Transform perchPos;
	private float timerPerch;
	private float timerIdle;
	// Use this for initialization
	void Start () 
	{
		PlayerManager pManager = GameObject.FindGameObjectWithTag ("PlayerManager").GetComponent<PlayerManager>();
		for (int i=0; i<pManager.players.Count; i++) 
		{
			Woodsman wScript = pManager.players[i].GetComponent<Woodsman>();
			if(wScript)
			{
				woodsman = pManager.players[i];
				break;
			}
		}
		mode = 1;
		initialPoint = Vector3.zero;
		perchPos = woodsman.transform.Find ("perchPos");
		timerPerch = Random.Range(15.0f,22.0f);
		timerIdle = Random.Range (15.0f,22.0f);
	}
	
	// Update is called once per frame
	void Update () 
	{

		if (mode == 2) 
		{
			if (Vector3.Distance(transform.position,initialPoint) <= 0.1 && !arrived)
			{
				transform.position = initialPoint;
				arrived = true;
			}
			else
			{
				Vector3 hawkXZ = new Vector3(transform.position.x,0,transform.position.z);
				Vector3 woodsXZ = new Vector3(woodsman.transform.position.x,0,woodsman.transform.position.z);
				
				if(initialPoint == Vector3.zero)
				{
					initialPoint = transform.position + (woodsman.transform.forward * 12.0f) + (woodsXZ-hawkXZ);
				}
				Vector3 moveVec = (initialPoint-transform.position);
				moveVec.Normalize();
				transform.up = moveVec;
				timer = timer + Time.deltaTime;
				transform.position = transform.position + (moveVec * speed * Time.deltaTime);
			}
			if (arrived)
			{
				if(Vector3.Distance(woodsman.transform.position,transform.position) > 2.4f)
				{
					mode = 3;
				}
				else 
				{
					mode = 1;
				}
				timer = 0.0f;
				initialPoint = Vector3.zero;
				arrived = false;
			}
			
		}
		if (mode == 1) 
		{

			timerPerch = timerPerch - Time.deltaTime;
			if(timerPerch <= 0.0f)
			{
				mode = 0;
				timerPerch = Random.Range(15.0f,22.0f);
			}
			paddingVector = (new Vector3 (transform.position.x, 0, transform.position.z)) - (new Vector3 (woodsman.transform.position.x, 0, woodsman.transform.position.z));
			paddingVector.Normalize();
			
			facingVector = Vector3.Cross (paddingVector, Vector3.up);
			transform.up = facingVector;
			
			float dist = Vector3.Distance(new Vector3 (transform.position.x, 0, transform.position.z),new Vector3 (woodsman.transform.position.x, 0, woodsman.transform.position.z));
			
			if(dist < 2.0f)
			{
				transform.position += paddingVector * Time.deltaTime;
			}
			else if(dist  > 2.2f)
			{
				transform.position += -paddingVector * 1.5f * Time.deltaTime;
			}
			transform.RotateAround (woodsman.transform.position, Vector3.up, 120 * Time.deltaTime);

		}
		
		if (mode == 3) 
		{
			
			Vector3 movement = woodsman.transform.position - transform.position;
			if(Vector3.Distance(woodsman.transform.position,transform.position) < 2.4f)
			{
				mode = 1;
			}
			movement.Normalize();
			transform.up = movement;
			transform.position = transform.position + (movement * speed * Time.deltaTime);
		}

		if (mode == 0) 
		{

			if(Vector3.Distance(perchPos.position,transform.position) < 0.1f)
			{
				transform.position = perchPos.position;
				arrivedPerch = true;
				transform.forward = woodsman.transform.forward;
			}
			else
			{
				Vector3 movement = perchPos.position - transform.position;
				movement.Normalize();
				transform.up = movement;
				transform.position = transform.position + (movement * speed * Time.deltaTime);
			}

			timerIdle = timerIdle - Time.deltaTime;
			if(timerIdle <= 0.0f)
			{
				mode = 1;
				timerIdle = Random.Range(15.0f,22.0f);
			}
		}

	}

	void OnCollisionEnter(Collision c)
	{
		if(c.gameObject.CompareTag("wall") || c.gameObject.CompareTag("door"))
		{
			Debug.Log ("We hit");
			if(mode == 2)
			{
				mode = 3;
				timer = 0.0f;
				initialPoint = Vector3.zero;
				arrived = false;
			}
		}
	}
}
