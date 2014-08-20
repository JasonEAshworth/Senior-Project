using UnityEngine;
using System.Collections;

public class GameCamera : MonoBehaviour 
{
	private Transform target;
	private Vector3	camTarget;
	Transform camCenter;


	// Camera focus stuff
	//private bool isOrtho;
	GameObject[] players;
	
	float curDist;
	Vector3 vecDist;
	
	//float lrgDist;
	//float avgDist;

	
	float height = 7;
	//float dist = 0;
	//float spd = 1;
	//float offset;
	
	void Start () 
	{
		//target = GameObject.FindGameObjectWithTag("Player").transform;
		players = GameObject.FindGameObjectsWithTag("Player");
	}
	void OnGUI()	
	{
		//GUILayout.Label("largest distance is = " +lrgDist.ToString());
		//GUILayout.Label("height = " + height.ToString());
		GUILayout.Label("Current distance is = " +curDist.ToString());
		GUILayout.Label("num players = " + players.Length.ToString());
	}
	
	// Update is called once per frame
	void Update () 
	{
		camTarget = new Vector3(target.position.x,transform.position.y,target.position.z + 10);
		transform.position = Vector3.Lerp(transform.position,camTarget,Time.deltaTime * 8);

	}
}
