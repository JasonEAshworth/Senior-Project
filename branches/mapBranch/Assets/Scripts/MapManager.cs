using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum RoomType
{
	HORDE,
	PUZZLE,
	BOSS,
	TREASURE,
	SQUIRREL
}

public class RoomGraph
{
	public List<RoomNode> rooms;

	public RoomGraph()
	{
		rooms = new List<RoomNode>();
	}

	public void addRoom(string name)
	{
		RoomNode room = new RoomNode(name);
	}
}

public class RoomNode
{
	public string name;
	public RoomType type;
	public RoomNode north;
	public RoomNode east;
	public RoomNode south;
	public RoomNode west;

	public RoomNode(string n)
	{
		name = n;
		// room type
		if (n.Contains("_H_"))
		{
			type = RoomType.HORDE;
		}
		else if (n.Contains("_P_"))
		{
			type = RoomType.PUZZLE;
		}
		else if (n.Contains("_B_"))
		{
			type = RoomType.BOSS;
		}
		else if (n.Contains("_T_"))
		{
			type = RoomType.TREASURE;
		}
		else
		{
			type = RoomType.SQUIRREL;
		}
	}
}

public class MapManager : MonoBehaviour 
{
	private RoomGraph map;
	public string[] roomsToLoad = new string[3];

	void Start()
	{
		map = new RoomGraph();
		for (int i = 0; i < roomsToLoad.Length; i++)
		{
			map.addRoom(roomsToLoad[i]);
		}
	}

	void moveToNewRoom()
	{

	}
}
