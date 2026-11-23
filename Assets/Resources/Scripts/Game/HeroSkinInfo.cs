using UnityEngine;

/// Sits on a hero model built in the editor from a store character. The model's own Animator is
/// removed in the build, so the avatar it needs is kept here for the hero's Animator to take.
public class HeroSkinInfo : MonoBehaviour
{
	public Avatar avatar;
	public Texture2D portrait;   // HUD portrait rendered from this model; null keeps the 2015 one
}
