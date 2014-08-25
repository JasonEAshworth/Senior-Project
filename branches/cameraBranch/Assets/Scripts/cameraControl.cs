using UnityEngine;
using System.Collections;
[RequireComponent (typeof (CharacterController))]

public class cameraControl : MonoBehaviour {
	private bool isOrthographic;
	GameObject[] targets;
	float currentDistance;
	float largestDistance;
	Camera theCamera;
	float height = 5.0;
	float avgDistance;
	float distance = 5.0;                    // Default Distance 
	float speed = 1;
	float offset;

	public void  Start() 
	{
		targets = GameObject.FindGameObjectsWithTag("Player");
		if (theCamera) 
		{
			isOrthographic = theCamera.orthographic;
		}
	}

	public void OnGUI() 
	{
	    GUILayout.Label("largest distance is = " + largestDistance.ToString());
	    GUILayout.Label("height = " + height.ToString());
	    GUILayout.Label("number of players = " + targets.length.ToString());
	}

	public void LastUpdate() 
	{
		targets = GameObject.FindGameObjectsWithTag("Player");
		if (!GameObject.FindWithTag("Player"))
		{
			return;
		}
		Vector3 sum = new Vector3(0,0,0);
		for (int i = 0; i < targets.length; i++) 
		{
			sum += targets[i].transform.position;
		}

		float avgDistance = sum / targets.length;

		float largestDifference = returnLargestDifference();

		distance = Mathf.Lerp(height,largestDifference,.5);

		if (largestDifference < 8)
		{
			distance = 8;
			theCamera.transform.position.y = 10;
		}

		if (largestDifference > 8)
		{
			distance = 8;
		}

		theCamera.transform.position.x = avgDistance.x - 2;
		theCamera.transform.position.z = avgDistance.z + distance+ .5*(largestDifference);
		theCamera.transform.position.y = distance;
		theCamera.transform.LookAt(avgDistance);
	}

	public float returnLargestDifference() 
	{
		currentDistance = 0.0;
		largestDistance = 0.0;
		for (int i = 0; i < targest.length; i++) 
		{
			for(int j = 0; j < targets.length; j++) 
			{
				currentDistance = Vector3.Distance(targets[i].transform.position, targets[j].transform.position);
				if (currentDistance > largestDistance) 
				{
					largestDistance = currentDistance;
				}
			}
		}
		return largestDistance;
	}
}
