using UnityEngine;
using System.Collections;

public class HawkAI : MonoBehaviour {


	public int mode;
	public GameObject woodsman;
	// a vector from the hawk to the player to use to increase radius of the idle rotation
	private Vector3 paddingVector;
	private Vector3 facingVector;
	private float timer = 0.0f;
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
	}
	
	// Update is called once per frame
	void Update () 
	{

		if (mode == 2) 
		{
			Vector3 initialPoint = woodsman.transform.forward * 7.0f;
			Vector3 moveVec = (initialPoint-transform.position);
			moveVec.Normalize();
			transform.up = woodsman.transform.forward;
			timer = timer + Time.deltaTime;
			transform.position = transform.position + (moveVec * 3.0f * Time.deltaTime);
			if(timer > 1.5f)
			{
				mode = 1;
				timer = 0.0f;
			}

		}
		if (mode == 1) 
		{
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

	}
}
