using UnityEngine;

public enum MiniBossAttack { Slam, Charge, Summon, FireRing, HookPull, Volley }

public enum VariantTrait { None, ExplodesOnDeath, SplitsOnDeath, Charges, Fast }

/// A recolored, rescaled, re-tuned version of one of the zombie archetypes.
[System.Serializable]
public class EnemyVariant
{
	public string name = "Restless";
	public enemyArchtype replaces = enemyArchtype.MELEE;
	public Color tint = Color.white;
	public float scale = 1.0f;
	public float healthMultiplier = 1.0f;
	public float damageMultiplier = 1.0f;
	public float speedMultiplier = 1.0f;
	public Color glowColor = Color.black;   // black = no glow light
	public VariantTrait trait = VariantTrait.None;
	public int points = 100;
}

[System.Serializable]
public class MiniBossConfig
{
	public string name = "Mini-boss";
	public Color tint = Color.white;
	public Color glowColor = Color.red;
	public float scale = 1.9f;
	public float health = 1200.0f;
	public float damage = 18.0f;
	public float speed = 3.2f;
	public MiniBossAttack[] attacks = { MiniBossAttack.Slam };
}

/// One chapter of the run: its dungeon, look, enemies, mini-boss, and music. The final level
/// is a chapter too, marked isFinal, with a single boss room and King Restless.
[CreateAssetMenu(menuName = "Rise of the Restless/Chapter")]
public class ChapterDefinition : ScriptableObject
{
	public string id = "chapter";
	public int sealIndex = 0;                 // which seal it awards; -1 for the final level
	public bool isFinal = false;
	public string title = "Chapter";
	public string tagline = "";

	[Header("Dungeon")]
	public string starterRoom = "Map_04_S_N";
	public string bossRoom = "Map_00_B_N";
	public int numRooms = 5;                  // main path, counting the starter and boss rooms
	public int hordeWeight = 2;
	public int puzzleWeight = 1;
	public int reactionWeight = 1;

	[Header("Look")]
	public Color ambient = new Color(0.25f, 0.24f, 0.22f);
	public Color lightTint = Color.white;
	public float lightIntensity = 1.0f;
	public Color fogColor = Color.black;
	public float fogDensity = 0.0f;
	public string atmosphere = "dust";        // dust, embers, mist, or none

	[Header("Enemies")]
	public EnemyVariant[] variants = new EnemyVariant[0];
	public MiniBossConfig miniBoss = new MiniBossConfig();
	public float enemyHealthScale = 1.0f;
	public float enemyDamageScale = 1.0f;

	[Header("Audio")]
	public string music = "Music/hub";
	public string bossMusic = "Music/boss";
}
