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

public enum Direction
{
	NORTH,
	SOUTH,
	EAST,
	WEST
}

public class RoomGraph
{
	public List<RoomNode> rooms;
	public List<RoomNode> roomsLoaded;

	public RoomGraph()
	{
		rooms = new List<RoomNode>();
		roomsLoaded = new List<RoomNode>();
	}

	public void addRoom(string name)
	{
		RoomNode room = new RoomNode(name);
		rooms.Add(room);
	}

	public RoomNode getNodeByRoomName(string roomName)
	{
		for (int i = 0; i < rooms.Count; i++)
		{
			if (rooms[i].name == roomName)
			{
				return rooms[i];
			}
		}
		return null;
	}

	public void connectRooms(RoomNode roomOne, RoomNode roomTwo, Direction roomOneExit)
	{
		switch (roomOneExit)
		{
		case Direction.NORTH:
			roomOne.north = roomTwo;
			roomTwo.south = roomOne;
			break;
		case Direction.SOUTH:
			roomOne.south = roomTwo;
			roomTwo.north = roomOne;
			break;
		case Direction.EAST:
			roomOne.east = roomTwo;
			roomTwo.west = roomOne;
			break;
		case Direction.WEST:
			roomOne.west = roomTwo;
			roomTwo.east = roomOne;
			break;
		}
	}
}

public class RoomNode
{
	public string name;
	public GameObject obj;
	public RoomType type;
	public RoomNode[] neighbors;
	public RoomNode north;
	/*{
		get
		{
			return neighbors[0];
		}
		set
		{
			neighbors[0] = value;
		}
	}*/
	public RoomNode east;
	public RoomNode south;
	public RoomNode west;

	public RoomNode(string n)
	{
		name = n;
		neighbors = new RoomNode[4] {null, null, null, null};
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

	private PlayerManager pMan;

	void Awake()
	{
		// Set up the map and the rooms that will be used in the dungeon
		map = new RoomGraph();
		for (int i = 0; i < roomsToLoad.Length; i++)
		{
			map.addRoom(roomsToLoad[i]);
		}
		// Set up the first room of the dungeon
		loadRoom(map.rooms[0]);
		// TEMP: hardcoded room neighbors, will be set up by file read or randomization or something later
		map.connectRooms(map.rooms[0], map.rooms[1], Direction.NORTH);
		map.connectRooms(map.rooms[1], map.rooms[2], Direction.WEST);
		// Look at the first room's neighbors and set them up
		loadNeighbors(map.rooms[0]);
	}

	void Start()
	{
		pMan = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();

		foreach (GameObject player in GameObject.Find("PlayerManager").GetComponent<PlayerManager>().players)
		{
			player.GetComponent<PlayerBase>().roomIn = map.rooms[0];
		}
	}

	private void loadRoom(RoomNode roomNode)
	{
		GameObject room = Instantiate(Resources.Load(roomNode.name), Vector3.zero, Quaternion.Euler(0.0f, 180.0f, 0.0f)) as GameObject;
		roomNode.obj = room;
		map.roomsLoaded.Add(roomNode);
	}

	public void loadNeighbors(RoomNode roomNode)
	{
		if (roomNode.north != null && !map.roomsLoaded.Contains(roomNode.north))
		{
			loadRoom(roomNode.north);
			// align positions of doorways properly
			GameObject rNorthObj = roomNode.north.obj;
			rNorthObj.transform.position = roomNode.obj.transform.Find("N_transition").position;
			rNorthObj.transform.position -= rNorthObj.transform.Find("S_transition").position - rNorthObj.transform.position;
			// set up doorway triggers
			GameObject trigger = roomNode.obj.transform.Find("N_transition").FindChild("trigger").gameObject;
			trigger.SetActive(true);
			trigger.GetComponent<Doorway>().sideA = roomNode;
			trigger.GetComponent<Doorway>().sideB = roomNode.north;
		}
		if (roomNode.south != null && !map.roomsLoaded.Contains(roomNode.south))
		{
			loadRoom(roomNode.south);
			// align positions of doorways properly
			GameObject rSouthObj = roomNode.south.obj;
			rSouthObj.transform.position = roomNode.obj.transform.Find("S_transition").position;
			rSouthObj.transform.position -= rSouthObj.transform.Find("N_transition").position - rSouthObj.transform.position;
			// set up doorway triggers
			GameObject trigger = roomNode.obj.transform.Find("S_transition").FindChild("trigger").gameObject;
			trigger.SetActive(true);
			trigger.GetComponent<Doorway>().sideA = roomNode;
			trigger.GetComponent<Doorway>().sideB = roomNode.south;
		}
		if (roomNode.east != null && !map.roomsLoaded.Contains(roomNode.east))
		{
			loadRoom(roomNode.east);
			// align positions of doorways properly
			GameObject rEastObj = roomNode.east.obj;
			rEastObj.transform.position = roomNode.obj.transform.Find("E_transition").position;
			rEastObj.transform.position -= rEastObj.transform.Find("W_transition").position - rEastObj.transform.position;
			// set up doorway triggers
			GameObject trigger = roomNode.obj.transform.Find("E_transition").FindChild("trigger").gameObject;
			trigger.SetActive(true);
			trigger.GetComponent<Doorway>().sideA = roomNode;
			trigger.GetComponent<Doorway>().sideB = roomNode.east;
		}
		if (roomNode.west != null && !map.roomsLoaded.Contains(roomNode.west))
		{
			loadRoom(roomNode.west);
			// align positions of doorways properly
			GameObject rWestObj = roomNode.west.obj;
			rWestObj.transform.position = roomNode.obj.transform.Find("W_transition").position;
			rWestObj.transform.position -= rWestObj.transform.Find("E_transition").position - rWestObj.transform.position;
			// set up doorway triggers
			GameObject trigger = roomNode.obj.transform.Find("W_transition").FindChild("trigger").gameObject;
			trigger.SetActive(true);
			trigger.GetComponent<Doorway>().sideA = roomNode;
			trigger.GetComponent<Doorway>().sideB = roomNode.west;
		}
	}

	public void unloadEmptyRooms()
	{
		foreach (RoomNode node in getRoomsPlayersIn())
		{

		}
	}

	private void unloadRoom(RoomNode roomNode)
	{
		Destroy(roomNode.obj);
		map.roomsLoaded.Remove(roomNode);
	}

	private List<RoomNode> getRoomsPlayersIn()
	{
		List<RoomNode> roomsIn = new List<RoomNode>();
		foreach (GameObject player in GameObject.Find("PlayerManager").GetComponent<PlayerManager>().players)
		{
			RoomNode location = player.GetComponent<PlayerBase>().roomIn;
			if (!roomsIn.Contains(location))
			{
				roomsIn.Add(location);
			}
		}
		return roomsIn;
	}
}
