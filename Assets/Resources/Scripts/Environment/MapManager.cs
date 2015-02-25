using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

// Used to label the main room types
public enum RoomType
{
	STARTER,
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
	WEST,
	NONE
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
		default:
			Debug.Log("Error connecting rooms " + roomOne.name + " and " + roomTwo.name + ": no direction");
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
	public List<GameObject> playerRespawns;
	public List<EnemySpawner> enemySpawners;
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
		playerRespawns = new List<GameObject>();
		enemySpawners = new List<EnemySpawner>();
		// set up room type
		if (n.Contains("_S_"))
		{
			type = RoomType.STARTER;
		}
		else if (n.Contains("_H_"))
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
	public struct RoomPrefab
	{
		public string prefabName;
		public int[] position;
		public Direction exit;

		public RoomPrefab(string name, int[] pos, Direction ex)
		{
			prefabName = name;
			position = pos;
			exit = ex;
		}
	};

	// set to true for procedural map generation, false to use a preset order of rooms
	// note: preset order of rooms has to use the public roomsToLoad array and manually have the rooms
	// connected with map.connectRooms() after the map has been created
	public bool proceduralGeneration = false;	

	private RoomGraph map;
	private int numRooms = 9;
	public string[] roomsToLoad = new string[3];	// temp array used for loading premade dungeons

	private string RoomPrefabFilePath = "Assets/Resources/Prefabs/Environment/Resources/";
	DirectoryInfo dir;
	FileInfo[] info;

	private int playerSpawnRoom = 0;

	private PlayerManager pMan;

	void Awake()
	{
		// Set up the map and the rooms that will be used in the dungeon
		map = new RoomGraph();
		generateDungeon();
	}

	void Start()
	{
		foreach (GameObject player in GameObject.Find("PlayerManager").GetComponent<PlayerManager>().players)
		{
			player.GetComponent<PlayerBase>().roomIn = map.rooms[0];
		}
	}

	// Called on awake. Generates the dungeon and sets the players in the first room
	private void generateDungeon()
	{
		if (proceduralGeneration)
		{
			// Get references to all of the room prefab files
			dir = new DirectoryInfo(RoomPrefabFilePath);
			info = dir.GetFiles("*.prefab");

			// Generate the dungeon with a recursive function that works one room at a time
			List<RoomPrefab> roomsToUse = new List<RoomPrefab>();
			generateRoom(roomsToUse, new List<string>(), "");

			// Log an error message if the generation failed
			if (roomsToUse.Count == 0)
			{
				Debug.Log ("Error generating dungeon, could not generate dungeon from given room prefabs");
			}

			// Once all of the rooms have been selected, set up the map and connect the rooms
			foreach (RoomPrefab rp in roomsToUse)
			{
				string rName = rp.prefabName.Substring(0, rp.prefabName.Length - 7);
				map.addRoom(rName);
			}
			for (int i = 0; i < map.rooms.Count - 1; i++)
			{
				//Debug.Log (map.rooms[i].name + " " + roomsToUse[i].exit);
				map.connectRooms(map.rooms[i], map.rooms[i+1], roomsToUse[i].exit);
			}
			// Load the first room and its neighbors
			loadRoom(map.rooms[0]);
			//loadNeighbors(map.rooms[0]);
			for (int i = 0; i < map.rooms.Count; i++)
			{
				loadNeighbors(map.rooms[i]);
			}
			// Give the player manager the first room's spawn points
			pMan = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
			pMan.assignNewSpawnPoints(map.rooms[0].playerRespawns.ToArray());
		}
		else
		{
			for (int i = 0; i < roomsToLoad.Length; i++)
			{
				map.addRoom(roomsToLoad[i]);
			}
			// Set up the first room of the dungeon
			loadRoom(map.rooms[0]);
			// hardcoded room neighbors, this must be done by hand to get rooms to connect for this method 
			map.connectRooms(map.rooms[0], map.rooms[1], Direction.NORTH);
			map.connectRooms(map.rooms[1], map.rooms[2], Direction.NORTH);
			map.connectRooms(map.rooms[2], map.rooms[3], Direction.WEST);
			// Look at the first room's neighbors and set them up
			loadNeighbors(map.rooms[0]);
			// Give the player manager the first room's spawn points
			pMan = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
			pMan.assignNewSpawnPoints(map.rooms[0].playerRespawns.ToArray());
		}
	}

	private bool generateRoom(List<RoomPrefab> rooms, List<string> roomsNotToUse, string lastDirection)
	{
		// Figure out which entrance the next room has to have to connect with the previous room
		string lookingFor = "";
		int[] nextRoomPos = new int[2];
		if (rooms.Count > 0)
		{
			nextRoomPos[0] = rooms[rooms.Count-1].position[0];
			nextRoomPos[1] = rooms[rooms.Count-1].position[1];
		}
		else
		{
			nextRoomPos[0] = 0;
			nextRoomPos[1] = 0;
		}
		switch (lastDirection)
		{
		case "N":
			lookingFor = "S";
			nextRoomPos[1]++;
			break;
		case "W":
			lookingFor = "E";
			nextRoomPos[0]--;
			break;
		case "E":
			lookingFor = "W";
			nextRoomPos[0]++;
			break;
		case "S":
			lookingFor = "N";
			nextRoomPos[1]--;
			break;
		}

		// Make sure this room won't overlap with the others
		foreach (RoomPrefab rp in rooms)
		{
			if (rp.position[0] == nextRoomPos[0] && rp.position[1] == nextRoomPos[1])
			{
				roomsNotToUse.Add(rooms[rooms.Count-1].prefabName);
				rooms.RemoveAt(rooms.Count-1);
				return false;
			}
		}

		bool roomSet = false; // becomes true when the room is safe to use

		// Continue trying to pick a room until either a suitable one is found or all options are exhausted, upon which we scrap this room and back out to the previous one
		while (!roomSet)
		{
			// Look through all room prefabs and select all of the rooms that can connect with the previous one that haven't been used yet
			List<string> potentialRooms = new List<string>();
			foreach (FileInfo f in info)
			{
				if (roomsNotToUse.Contains(f.Name))
				{
					continue;
				}

				string fileName = f.Name.Substring(0, f.Name.Length - 7); // remove ".prefab"
				string e = fileName.Substring(f.Name.LastIndexOf("_") + 1); // get exit directions string

				// currently only supports rooms with two entrances
				if (e.Length > 2)
				{
					continue;
				}

				// First room
				if (rooms.Count == 0)
				{
					// first room must be starter type
					if (f.Name.Contains("_S_"))
					{
						potentialRooms.Add(fileName);
					}
				}
				// Final room
				else if (rooms.Count == numRooms - 1)
				{
					// makes sure the final room is a boss room
					if (f.Name.Contains("_B_") && e.Contains(lookingFor))
					{
						potentialRooms.Add(fileName);
					}
				}
				// Middle rooms
				else
				{
					// Room must have the required entrance to connect with the previous room, must have another exit in addition to that, and can't have been used already
					if (e.Contains(lookingFor) && e.Length > 1) //&& !rooms.Contains(f.Name))
					{
						potentialRooms.Add(fileName);	
					}
				}
			}
			// If no rooms work for branching from this one, then we have to scrap this one
			if (potentialRooms.Count == 0)
			{
				// If we have a room to go back to, scrap this room and continue generation from the previous one
				if (rooms.Count > 0)
				{
					roomsNotToUse.Add(rooms[rooms.Count-1].prefabName);
					rooms.RemoveAt(rooms.Count-1);
					return false;
				}
				// Otherwise, we are at the starter room, and generation has failed for all possible room combos, so we give up :(
				else
				{
					break;
				}
			}
			// Select one of these rooms at random and create the room
			int idx = Random.Range(0, potentialRooms.Count);
			string roomName = potentialRooms[idx];
			string thisExit = roomName.Substring(roomName.LastIndexOf("_") + 1);
			Direction exitDir = Direction.NONE;
			if (lookingFor != "")
			{
				thisExit = thisExit.Replace(lookingFor, "");	// currently only supports rooms with 2 entrances
			}
			switch (thisExit)
			{
			case "N":
				exitDir = Direction.NORTH;
				break;
			case "W":
				exitDir = Direction.WEST;
				break;
			case "E":
				exitDir = Direction.EAST;
				break;
			case "S":
				exitDir = Direction.SOUTH;
				break;
			}
			RoomPrefab newRoom = new RoomPrefab(roomName + ".prefab", nextRoomPos, exitDir);
			rooms.Add(newRoom);
			// If we have all rooms of the dungeon, return a success
			if (rooms.Count == numRooms)
			{
				return true;
			}
			// Set up generation of the next room if not done
			roomSet = generateRoom(rooms, new List<string>(), thisExit);
		}
		return true;
	}

	// Loads room and returns true if loading for first time
	private bool loadRoom(RoomNode roomNode)
	{
		if (!roomNode.hasBeenLoaded)
		{
			GameObject room = Instantiate(Resources.Load(roomNode.name), Vector3.zero, Quaternion.Euler(0.0f, 180.0f, 0.0f)) as GameObject;
			roomNode.obj = room;
			map.roomsActive.Add(roomNode);
			// Gather all of the room's respawn points
			GameObject[] respawns = GameObject.FindGameObjectsWithTag("Respawn");
			foreach (GameObject r in respawns)
			{
				if (r.transform.root == roomNode.obj.transform)
				{
					roomNode.playerRespawns.Add(r);
				}
			}
			EnemySpawner[] es = room.GetComponentsInChildren<EnemySpawner>();
			for (int i = 0; i < es.Length; i++)
			{
				roomNode.enemySpawners.Add(es[i]);
			}
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

	// Called whenever a player enters a room. Activates the spawners for that room if a player is entering it for the first time
	public void notifySpawners(RoomNode roomEntered)
	{
		foreach (EnemySpawner es in map.rooms[map.rooms.IndexOf(roomEntered)].enemySpawners)
		{
			if (!es.ableToSpawn)
			{
				es.enableSpawning();
				es.parentRoom = roomEntered.obj.transform;
			}
		}
	}

	// Returns a list of RoomNodes of the rooms that the players are currently located in
	public List<RoomNode> getRoomsPlayersIn()
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

	// Called whenever a player enters a room. If this room is closer to the end of the dungeon, it assigns new respawn points for the players
	public void updateRespawnPoints(RoomNode room)
	{
		int roomIdx = map.rooms.IndexOf(room);
		if (roomIdx > playerSpawnRoom)
		{
			playerSpawnRoom = roomIdx;
			pMan.assignNewSpawnPoints(room.playerRespawns.ToArray());
		}
	}
}
