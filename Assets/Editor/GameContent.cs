using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// Creates the data and scenes the arcade game needs: the room catalog, the chapter assets,
/// and the Title, Hub, and Ending scenes. Safe to run again: existing chapter assets and scenes
/// are left as they are, so edits made in the editor survive.
public static class GameContent
{
	public static readonly string[] Scenes = {
		"Assets/Scenes/Title.unity", "Assets/Scenes/Hub.unity", "Assets/Scenes/MainScene.unity", "Assets/Scenes/Ending.unity" };

	const string RoomFolder = "Assets/Resources/Prefabs/Environment/Resources";
	const string ChapterFolder = "Assets/Resources/Chapters";

	// Map_02_B_S is an empty boss room: no boss, no spawn points.
	static readonly string[] excludedRooms = { "Map_02_B_S" };

	[MenuItem("Tools/Senior Project/Prepare Game Content")]
	public static void Prepare()
	{
		BuildRoomCatalog();
		CreateChapters();
		EnsureScene(Scenes[0], typeof(TitleMenu));
		EnsureScene(Scenes[1], typeof(HubMenu));
		EnsureScene(Scenes[3], typeof(EndingScreen));
		EditorBuildSettings.scenes = Scenes.Select(s => new EditorBuildSettingsScene(s, true)).ToArray();
		AssetDatabase.SaveAssets();
	}

	[MenuItem("Tools/Senior Project/Build Room Catalog")]
	public static void BuildRoomCatalog()
	{
		string[] rooms = Directory.GetFiles(RoomFolder, "Map_*.prefab")
			.Select(Path.GetFileNameWithoutExtension)
			.Where(n => !excludedRooms.Contains(n))
			.OrderBy(n => n)
			.ToArray();
		const string path = "Assets/Resources/RoomCatalog.asset";
		RoomCatalog catalog = AssetDatabase.LoadAssetAtPath<RoomCatalog>(path);
		if (catalog == null)
		{
			catalog = ScriptableObject.CreateInstance<RoomCatalog>();
			AssetDatabase.CreateAsset(catalog, path);
		}
		catalog.rooms = rooms;
		EditorUtility.SetDirty(catalog);
		Debug.Log("[content] room catalog: " + rooms.Length + " rooms");
	}

	static void EnsureScene(string path, System.Type menu)
	{
		if (File.Exists(path))
			return;
		Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
		var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
		camera.tag = "MainCamera";
		camera.clearFlags = CameraClearFlags.SolidColor;
		camera.backgroundColor = Color.black;
		new GameObject(menu.Name, menu);
		EditorSceneManager.SaveScene(scene, path);
	}

	static EnemyVariant V(string name, enemyArchtype type, Color tint, float scale, float hp, float dmg, float spd, int points,
		VariantTrait trait = VariantTrait.None, Color? glow = null)
	{
		return new EnemyVariant
		{
			name = name, replaces = type, tint = tint, scale = scale, healthMultiplier = hp, damageMultiplier = dmg,
			speedMultiplier = spd, points = points, trait = trait, glowColor = glow ?? Color.black,
		};
	}

	static void CreateChapters()
	{
		if (!AssetDatabase.IsValidFolder(ChapterFolder))
			AssetDatabase.CreateFolder("Assets/Resources", "Chapters");

		Save("1_Catacombs", c =>
		{
			c.id = "catacombs"; c.sealIndex = 0; c.title = "The Catacombs"; c.tagline = "The restless dead do not sleep.";
			c.starterRoom = "Map_04_S_N"; c.bossRoom = "Map_00_B_S"; c.numRooms = 5;
			c.hordeWeight = 2; c.puzzleWeight = 1; c.reactionWeight = 1;
			c.ambient = new Color(0.13f, 0.16f, 0.24f); c.lightTint = new Color(0.55f, 0.75f, 1.0f); c.lightIntensity = 1.1f;
			c.fogColor = new Color(0.04f, 0.06f, 0.11f); c.fogDensity = 0.012f; c.atmosphere = "mist";
			c.variants = new[] {
				V("Restless", enemyArchtype.MELEE, new Color(0.75f, 0.85f, 1.0f), 1.0f, 1.0f, 1.0f, 1.0f, 100),
				V("Crawler", enemyArchtype.HORDE, new Color(0.6f, 0.7f, 0.6f), 0.75f, 0.5f, 0.6f, 1.5f, 60, VariantTrait.Fast),
				V("Bone Archer", enemyArchtype.RANGED, new Color(0.95f, 0.95f, 0.9f), 1.0f, 0.8f, 1.0f, 1.0f, 150, VariantTrait.None, new Color(0.4f, 0.6f, 1.0f)),
			};
			c.miniBoss = new MiniBossConfig { name = "The Gravewarden", tint = new Color(0.55f, 0.65f, 0.95f), glowColor = new Color(0.4f, 0.6f, 1.0f),
				scale = 2.0f, health = 1400, damage = 18, speed = 3.0f,
				attacks = new[] { MiniBossAttack.Slam, MiniBossAttack.Summon, MiniBossAttack.Slam, MiniBossAttack.Charge } };
			c.music = "Music/catacombs"; c.bossMusic = "Music/boss";
		});
		Save("2_TortureHalls", c =>
		{
			c.id = "torture"; c.sealIndex = 1; c.title = "The Torture Halls"; c.tagline = "Every cell has a key. Every key has a price.";
			c.starterRoom = "Map_00_S_E"; c.bossRoom = "Map_00_B_N"; c.numRooms = 6;
			c.hordeWeight = 1; c.puzzleWeight = 2; c.reactionWeight = 1;
			c.ambient = new Color(0.22f, 0.12f, 0.1f); c.lightTint = new Color(1.0f, 0.55f, 0.4f); c.lightIntensity = 1.15f;
			c.fogColor = new Color(0.14f, 0.05f, 0.03f); c.fogDensity = 0.01f; c.atmosphere = "dust";
			c.variants = new[] {
				V("Jailer", enemyArchtype.MELEE, new Color(0.9f, 0.6f, 0.5f), 1.25f, 1.8f, 1.4f, 0.85f, 180, VariantTrait.Charges),
				V("Prisoner", enemyArchtype.HORDE, new Color(1.0f, 0.85f, 0.75f), 0.9f, 0.6f, 0.8f, 1.35f, 70),
				V("Crossbowman", enemyArchtype.RANGED, new Color(0.8f, 0.5f, 0.45f), 1.0f, 1.0f, 1.2f, 1.0f, 150),
			};
			c.miniBoss = new MiniBossConfig { name = "The Warden", tint = new Color(0.85f, 0.45f, 0.35f), glowColor = new Color(1.0f, 0.3f, 0.2f),
				scale = 2.1f, health = 1700, damage = 20, speed = 3.2f,
				attacks = new[] { MiniBossAttack.Charge, MiniBossAttack.HookPull, MiniBossAttack.Slam, MiniBossAttack.Summon } };
			c.music = "Music/torture"; c.bossMusic = "Music/boss";
		});
		Save("3_Cellars", c =>
		{
			c.id = "cellars"; c.sealIndex = 2; c.title = "The Cellars"; c.tagline = "Something has been eating the stores.";
			c.starterRoom = "Map_00_S_S"; c.bossRoom = "Map_00_B_W"; c.numRooms = 6;
			c.hordeWeight = 3; c.puzzleWeight = 1; c.reactionWeight = 1;
			c.ambient = new Color(0.2f, 0.18f, 0.1f); c.lightTint = new Color(1.0f, 0.85f, 0.5f); c.lightIntensity = 1.0f;
			c.fogColor = new Color(0.08f, 0.07f, 0.03f); c.fogDensity = 0.008f; c.atmosphere = "dust";
			c.variants = new[] {
				V("Ghoul", enemyArchtype.MELEE, new Color(0.7f, 0.9f, 0.5f), 1.0f, 1.1f, 1.0f, 1.1f, 120, VariantTrait.SplitsOnDeath),
				V("Gnawer", enemyArchtype.HORDE, new Color(0.6f, 0.55f, 0.45f), 0.6f, 0.35f, 0.5f, 1.7f, 40, VariantTrait.Fast),
				V("Spitter", enemyArchtype.RANGED, new Color(0.6f, 1.0f, 0.4f), 1.0f, 0.9f, 1.0f, 1.0f, 150, VariantTrait.None, new Color(0.4f, 1.0f, 0.3f)),
			};
			c.miniBoss = new MiniBossConfig { name = "The Butcher", tint = new Color(1.0f, 0.7f, 0.6f), glowColor = new Color(1.0f, 0.6f, 0.3f),
				scale = 2.3f, health = 1900, damage = 22, speed = 3.4f,
				attacks = new[] { MiniBossAttack.HookPull, MiniBossAttack.Charge, MiniBossAttack.Volley, MiniBossAttack.Slam } };
			c.music = "Music/cellars"; c.bossMusic = "Music/boss";
		});
		Save("4_ForgeDepths", c =>
		{
			c.id = "forge"; c.sealIndex = 3; c.title = "The Forge Depths"; c.tagline = "The fires below never go out.";
			c.starterRoom = "Map_00_S_W"; c.bossRoom = "Map_00_B_S"; c.numRooms = 7;
			c.hordeWeight = 2; c.puzzleWeight = 1; c.reactionWeight = 2;
			c.ambient = new Color(0.25f, 0.12f, 0.05f); c.lightTint = new Color(1.0f, 0.6f, 0.25f); c.lightIntensity = 1.3f;
			c.fogColor = new Color(0.2f, 0.08f, 0.02f); c.fogDensity = 0.01f; c.atmosphere = "embers";
			c.variants = new[] {
				V("Ember", enemyArchtype.MELEE, new Color(1.0f, 0.5f, 0.25f), 1.0f, 0.9f, 1.0f, 1.2f, 140, VariantTrait.ExplodesOnDeath, new Color(1.0f, 0.45f, 0.1f)),
				V("Smith", enemyArchtype.MELEE, new Color(0.55f, 0.5f, 0.5f), 1.3f, 2.2f, 1.5f, 0.8f, 200, VariantTrait.Charges),
				V("Cinder Thrower", enemyArchtype.RANGED, new Color(1.0f, 0.6f, 0.3f), 1.0f, 1.0f, 1.3f, 1.0f, 160, VariantTrait.None, new Color(1.0f, 0.5f, 0.2f)),
			};
			c.miniBoss = new MiniBossConfig { name = "The Forgemaster", tint = new Color(1.0f, 0.55f, 0.3f), glowColor = new Color(1.0f, 0.4f, 0.1f),
				scale = 2.2f, health = 2100, damage = 22, speed = 3.0f,
				attacks = new[] { MiniBossAttack.FireRing, MiniBossAttack.Slam, MiniBossAttack.Volley, MiniBossAttack.Charge } };
			c.music = "Music/forge"; c.bossMusic = "Music/boss";
		});
		Save("5_ThroneRoom", c =>
		{
			c.id = "throne"; c.sealIndex = -1; c.isFinal = true; c.title = "The Throne Room"; c.tagline = "Only the king remains.";
			c.starterRoom = ""; c.bossRoom = "Map_03_B_E"; c.numRooms = 1;
			c.ambient = new Color(0.2f, 0.16f, 0.22f); c.lightTint = new Color(0.9f, 0.7f, 1.0f); c.lightIntensity = 1.2f;
			c.fogColor = new Color(0.08f, 0.04f, 0.1f); c.fogDensity = 0.01f; c.atmosphere = "embers";
			c.music = "Music/final"; c.bossMusic = "Music/final";
		});
	}

	static void Save(string file, System.Action<ChapterDefinition> fill)
	{
		string path = ChapterFolder + "/" + file + ".asset";
		if (AssetDatabase.LoadAssetAtPath<ChapterDefinition>(path) != null)
			return;
		ChapterDefinition c = ScriptableObject.CreateInstance<ChapterDefinition>();
		fill(c);
		AssetDatabase.CreateAsset(c, path);
		Debug.Log("[content] created " + path);
	}
}
