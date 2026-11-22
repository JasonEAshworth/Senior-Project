using UnityEngine;

/// The names of the room prefabs the dungeon generator may use. Builds cannot list files in
/// the Assets folder, so the editor writes this list (Tools > Senior Project > Build Room Catalog,
/// and before every build).
public class RoomCatalog : ScriptableObject
{
	public string[] rooms = new string[0];

	static RoomCatalog cached;

	public static RoomCatalog Load()
	{
		if (cached == null)
			cached = Resources.Load<RoomCatalog>("RoomCatalog");
		return cached;
	}
}
