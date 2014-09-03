using UnityEngine;
using System.Collections;

public class GameCamera : MonoBehaviour 
{
	/*private bool isOrtho;
	public GameObject[] targets;
	float curDist;
	float lrgDist;
	Camera mainCam;
	float height = 2.0f;
	Vector3 avgDist;
	float distance = 0.0f;
	int speed = 1;
	float offset;
	
	void start()
	{

		targets = GameObject.FindGameObjectsWithTag("Player");
		if(mainCam) isOrtho = mainCam.orthographic;
	}
	
	void OnGui()
	{
		GUILayout.Label("largest distance is = " + lrgDist.ToString());
 	    GUILayout.Label("height = " + height.ToString());
 	    GUILayout.Label("number of players = " + targets.Length.ToString());
	}
	
	void Update()
	{
		
		if(!GameObject.FindWithTag("Player")) return;
		
		Vector3 sum = new Vector3(0f,0f,0f);
		
//		foreach (GameObject go in targets)
		//	sum += go.transform.position;
		
		avgDist = sum / 2;
		float lrgDiff = retLrgDiff();
		if (lrgDiff < 5)
		{
			height = 5;
		}
		if (lrgDiff > 10)
		{
			Debug.Log(lrgDiff);
			height = 10;
			lrgDiff = 10;
		}
		//height = Mathf.Lerp(height,lrgDiff,.1f);
		
		Vector3 temp = mainCam.transform.position;
	/*	if (isOrtho)
		{
			temp.x = avgDist.x;
			temp.y = height;
			mainCam.orthographicSize = lrgDiff;
			
		//mainCam.transform.LookAt(avgDist);
	//	}
		//else
		//{ 
			temp.x = avgDist.x -5;
			temp.y = height;
			temp.z = avgDist.z + distance + lrgDiff;
			mainCam.transform.LookAt(avgDist);
	//	}
		mainCam.transform.position = temp;
	}
	
	float retLrgDiff()
	{
		float curDist = 0.0f;
		float lrgDist = 0.0f;
		
		for(int x = 0; x < 2; x++)
		{
			for(int z=0; z < 2; z++)
			{
				curDist = Vector3.Distance(targets[0].transform.position,targets[0].transform.position);
				if(curDist > lrgDist)
					lrgDist = curDist;
			}
		}
		return lrgDist;
	}
}*/

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

	
	float height = 5;
	//float dist = 0;
	//float spd = 1;
	//float offset;
	
	void Start () 
	{
		//targets = GameObject.FindGameObjectWithTag("Player").transform;
		//if (cam1)
		//	isOrtho = cam1.orthographic;
		target = GameObject.FindGameObjectWithTag("Player").transform;
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
		//height = Mathf.Lerp(height, curDist, Time.deltaTime);
		//curDist = Vector3.Distance(players[0].transform.position,players[1].transform.position);
//		transform.position.y = height;
		//transform.position = Vector3.Lerp(players[1].transform.position,players[0].transform.position,0.5f);
		
		//camCenter = new Vector3(
		//camTarget = new Vector3( target.position.x, transform.position.y,  target.position.z -3 );
		camTarget = new Vector3(vecDist.x, 10, vecDist.z -30);
	
		transform.LookAt(target.transform);
	}
}
