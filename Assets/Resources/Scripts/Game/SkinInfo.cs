using UnityEngine;

/// Measurements of a skin prefab, taken in the editor when the skin is built, so SkinSwap can
/// size it to the enemy and match its walk and run clips to how fast the enemy moves.
public class SkinInfo : MonoBehaviour
{
	public float height = 1.8f;      // body height at the prefab's own scale, without weapons
	public float heightScale = 1.0f;  // height next to the zombie it covers: a goblin is shorter, a troll taller
	public float walkSpeed = 1.4f;   // m/s at which the walk clip's feet do not slide
	public float runSpeed = 4.0f;
}
