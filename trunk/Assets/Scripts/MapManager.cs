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

	public RoomGraph()
	{
		rooms = new List<RoomNode>();
	}

	public void addRoom(string name)
	{
		RoomNode room = new RoomNode(name);
		rooms.Add(room);
	}

	public void connectRooms(int roomOneIdx, int roomTwoIdx, Direction roomOneExit)
	{
		if (roomOneIdx >= rooms.Count || roomTwoIdx >= rooms.Count)
		{
			return;
		}
		switch(roomOneExit)
		{
		case Direction.NORTH:
			rooms[roomOneIdx].north = rooms[roomTwoIdx];
			rooms[roomTwoIdx].south = rooms[roomOneIdx];
			break;
		case Direction.SOUTH:
			rooms[roomOneIdx].south = rooms[roomTwoIdx];
			rooms[roomTwoIdx].north = rooms[roomOneIdx];
			break;
		case Direction.EAST:
			rooms[roomOneIdx].east = rooms[roomTwoIdx];
			rooms[roomTwoIdx].west = rooms[roomOneIdx];
			break;
		case Direction.WEST:
			rooms[roomOneIdx].west = rooms[roomTwoIdx];
			rooms[roomTwoIdx].east = rooms[roomOneIdx];
			break;
		}
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

	private GameObject currentRoomObj;
	private RoomNode currentRoomNode;

	void Awake()
	{
		map = new RoomGraph();
		for (int i = 0; i < roomsToLoad.Length; i++)
		{
			map.addRoom(roomsToLoad[i]);
		}
		currentRoomNode = map.rooms[0];
		currentRoomObj = Instantiate(Resources.Load(map.rooms[0].name)) as GameObject;

		//hardcoded for now
		map.connectRooms(0, 1, Direction.NORTH);
		map.connectRooms(1, 2, Direction.WEST);
	}

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
			currentRoomObj = Instantiate(Resources.Load(newRoom.name)) as GameObject;
			GameObject.Find("PlayerManager").GetComponent<PlayerManager>().getNewSpawnPoints();
			GameObject.Find("PlayerManager").GetComponent<PlayerManager>().respawnAllPlayers();
		}
	}
}
