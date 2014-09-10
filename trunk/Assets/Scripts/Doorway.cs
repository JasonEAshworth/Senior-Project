using UnityEngine;
using System.Collections;

public class Doorway : MonoBehaviour 
{
	private MapManager mapMan;

	public Direction doorwayDirection;

	public RoomNode sideA;
	public RoomNode sideB;

	void Start()
	{
		mapMan = GameObject.Find("MapManager").GetComponent<MapManager>();
	}

	void OnTriggerExit(Collider c)
	{
		if (c.gameObject.tag == "Player")
		{
			Vector3 playerToDoorway = (transform.position - c.transform.position).normalized;

			// If player is leaving towards side B
			if (Vector3.Dot(playerToDoorway, c.transform.forward) > 0)
			{
				mapMan.playerSwitchRoom(sideB, sideA);
			}
			else // side A
			{
				mapMan.playerSwitchRoom(sideA, sideB);
			}
		}
	}

	void Update()
	{
		/*
		if (transitioning && transitionTimer > 0.0f)
		{
			transitionTimer -= Time.deltaTime;
			if (transitionTimer <= 0.0f)
			{
				mapMan.moveToNewRoom(doorwayDirection);
			}
		}
		*/
	}
}
