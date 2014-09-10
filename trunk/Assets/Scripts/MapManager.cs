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
	public List<RoomNode> playersIn;
	public List<RoomNode> roomsLoaded;

	public RoomGraph()
	{
		rooms = new List<RoomNode>();
		playersIn = new List<RoomNode>();
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
		for (int i = 0; i < GameObject.Find("PlayerManager").GetComponent<PlayerManager>().players.Count; i++)
		{
			map.playersIn.Add(map.rooms[0]);
		}
		// TEMP: hardcoded room neighbors, will be set up by file read or randomization or something later
		map.connectRooms(map.rooms[0], map.rooms[1], Direction.NORTH);
		map.connectRooms(map.rooms[1], map.rooms[2], Direction.WEST);
		// Look at the first room's neighbors and set them up
		loadNeighbors(map.rooms[0]);

		//GameObject room2 = Instantiate (Resources.Load (map.rooms[1].name), currentRoomObj.transform.Find ("N_transition").position, Quaternion.Euler(new Vector3(0.0f, 180.0f, 0.0f))) as GameObject;
		//room2.transform.position -= room2.transform.Find ("S_transition").position - room2.transform.position;
	}

	void Start()
	{
		pMan = GameObject.Find ("PlayerManager").GetComponent<PlayerManager> ();
	}

	private void loadRoom(RoomNode roomNode)
	{
		GameObject room = Instantiate(Resources.Load(roomNode.name)) as GameObject;
		roomNode.obj = room;
		map.roomsLoaded.Add(roomNode);
	}

	private void loadNeighbors(RoomNode roomNode)
	{
		if (roomNode.north != null && !map.roomsLoaded.Contains(roomNode.north))
		{
			loadRoom(roomNode.north);
		}
		if (roomNode.south != null && !map.roomsLoaded.Contains(roomNode.south))
		{
			
		}
		if (roomNode.east != null && !map.roomsLoaded.Contains(roomNode.east))
		{
			
		}
		if (roomNode.west != null && !map.roomsLoaded.Contains(roomNode.west))
		{
			
		}
	}

	private void unloadRoom(RoomNode roomNode)
	{
		Destroy(roomNode.obj);
		map.roomsLoaded.Remove(roomNode);
	}

	/*
	public List<string> getRoomsPlayersIn()
	{
		List<string> roomList = new List<string> ();
		for (int i = 0; i < pMan.players.Count; i++)
		{
			string playerRoom = pMan.players[i].GetComponent<PlayerBase>().roomIn;
			if (!roomList.Contains(playerRoom))
			{
				roomList.Add(playerRoom);
			}
		}
		return roomList;
	}*/
	
	public void playerSwitchRoom(RoomNode roomEnter, RoomNode roomLeave)
	{
		map.playersIn.Add(roomEnter);
		map.playersIn.Remove(roomLeave);
	}

	/*
	public void moveToNewRoom(Direction doorDir)
	{
		RoomNode newRoom = null;
		switch(doorDir)
		{
		case Direction.NORTH:
			newRoom = currentRoomNode.north;
			break;
		case Direction.SOUTH:
			newRoom = currentRoomNode.south;
			break;
		case Direction.EAST:
			newRoom = currentRoomNode.east;
			break;
		case Direction.WEST:
			newRoom = currentRoomNode.west;
			break;
		}

		if (newRoom != null)
		{
			currentRoomNode = newRoom;
			DestroyImmediate(currentRoomObj);
			currentRoomObj = Instantiate(Resources.Load(newRoom.name), Vector3.zero, Quaternion.Euler(new Vector3(0.0f, 180.0f, 0.0f))) as GameObject;
			GameObject.Find("PlayerManager").GetComponent<PlayerManager>().getNewSpawnPoints();
			GameObject.Find("PlayerManager").GetComponent<PlayerManager>().respawnAllPlayers();
		}
	}
	*/
}
