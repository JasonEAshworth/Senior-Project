using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Used to label the main room types
public enum RoomType
{
	HORDE,
	PUZZLE,
	BOSS,
	TREASURE,
	REACTION
}

// Used to label the four main cardinal directions
public enum Direction
{
	NORTH,
	SOUTH,
	EAST,
	WEST
}

// This graph makes up all of the connected rooms in a dungeon
public class RoomGraph
{
	public List<RoomNode> rooms;
	public List<RoomNode> roomsActive;

	public RoomGraph()
	{
		rooms = new List<RoomNode>();
		roomsActive = new List<RoomNode>();
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

	// Creates a neighboring connection in the graph structure between the two given rooms
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

// These nodes make up the contents of the RoomGraph. Each RoomNode represents a room in the graph
public class RoomNode
{
	public string name;
	public GameObject obj;
	public RoomType type;
	public bool hasBeenLoaded = false; // used to keep track of the first time a room is loaded for item management
	public List<GameObject> doors;
	public List<GameObject> items;
	public RoomNode[] neighbors;
	public RoomNode north
	{
		get
		{
			return neighbors[0];
		}
		set
		{
			neighbors[0] = value;
		}
	}
	public RoomNode east
	{
		get
		{
			return neighbors[1];
		}
		set
		{
			neighbors[1] = value;
		}
	}
	public RoomNode south
	{
		get
		{
			return neighbors[2];
		}
		set
		{
			neighbors[2] = value;
		}
	}
	public RoomNode west
	{
		get
		{
			return neighbors[3];
		}
		set
		{
			neighbors[3] = value;
		}
	}

	public RoomNode(string n)
	{
		name = n;
		neighbors = new RoomNode[4] {null, null, null, null};
		doors = new List<GameObject>();
		items = new List<GameObject>();
		// set up room type
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
		else if (n.Contains("_R_"))
		{
			type = RoomType.REACTION;
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

	// Loads room and returns true if loading for first time
	private bool loadRoom(RoomNode roomNode)
	{
		if (!roomNode.hasBeenLoaded)
		{
			GameObject room = Instantiate(Resources.Load(roomNode.name), Vector3.zero, Quaternion.Euler(0.0f, 180.0f, 0.0f)) as GameObject;
			roomNode.obj = room;
			map.roomsActive.Add(roomNode);
			roomNode.hasBeenLoaded = true;
			return true;
		}
		else
		{
			roomNode.obj.SetActive(true);
			return false;
		}
	}

	// Looks at all of the neighbors of the given RoomNode and loads them up if they aren't already loaded. Also performs any extra room setup needed
	public void loadNeighbors(RoomNode roomNode)
	{
		if (roomNode.north != null && !map.roomsActive.Contains(roomNode.north))
		{
			if (loadRoom(roomNode.north))
			{
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
		}
		if (roomNode.south != null && !map.roomsActive.Contains(roomNode.south))
		{
			if (loadRoom(roomNode.south))
			{
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
		}
		if (roomNode.east != null && !map.roomsActive.Contains(roomNode.east))
		{
			if (loadRoom(roomNode.east))
			{
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
		}
		if (roomNode.west != null && !map.roomsActive.Contains(roomNode.west))
		{
			if (loadRoom(roomNode.west))
			{
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
	}

	// Called every time a player walks into a new room. Looks at all currently loaded rooms and unloads the ones that players aren't in or adjacent to
	public void unloadEmptyRooms()
	{
		List<RoomNode> roomsToUnload = new List<RoomNode>();
		foreach (RoomNode loadedRoom in map.roomsActive)
		{
			bool keep = false;
			foreach (RoomNode playerRoom in getRoomsPlayersIn())
			{
				if (playerRoom == loadedRoom)
				{
					keep = true;
				}
				foreach (RoomNode neighbor in playerRoom.neighbors)
				{
					if (neighbor == loadedRoom)
					{
						keep = true;
					}
				}
			}
			if (!keep)
			{
				roomsToUnload.Add(loadedRoom);
			}
		}
		foreach (RoomNode n in roomsToUnload)
		{
			unloadRoom(n);
		}
	}

	private void unloadRoom(RoomNode roomNode)
	{
		roomNode.obj.SetActive (false);
		map.roomsActive.Remove(roomNode);
	}

	// Returns a list of RoomNodes of the rooms that the players are currently located in
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
