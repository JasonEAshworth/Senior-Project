using UnityEngine;
using System.Collections;

public class Doorway : MonoBehaviour 
{
	private MapManager mapMan;

	public Direction doorwayDirection;
	private bool transitioning = false;
	private float transitionTimer = 1.5f;

	void Start()
	{
		mapMan = GameObject.Find("MapManager").GetComponent<MapManager>();
	}

	void Update()
	{
		if (transitioning && transitionTimer > 0.0f)
		{
			transitionTimer -= Time.deltaTime;
			if (transitionTimer <= 0.0f)
			{
				mapMan.moveToNewRoom(doorwayDirection);
			}
		}
	}

	void OnCollisionEnter(Collision c)
	{
		if (!transitioning && c.gameObject.tag == "Player")
		{
			transitioning = true;
			c.gameObject.SendMessage("disableControl");
		}
	}
}
