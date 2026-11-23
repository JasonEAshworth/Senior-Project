using System.Collections.Generic;
using UnityEngine;

/// Dresses a hero in a store model when one is installed (see HeroContent in the editor). The
/// hero's own Animator keeps its controller, so its clips, timing, and attack events are
/// unchanged; it only takes the model's humanoid avatar, which retargets the same clips onto the
/// new body. Without the model the hero keeps its 2015 mesh.
public static class HeroSkin
{
	const string Folder = "Heroes/";

	public static HeroSkinInfo Find(string heroName)
	{
		return Resources.Load<HeroSkinInfo>(Folder + heroName);
	}

	/// Returns the new model's renderers, or null when no model is installed for this hero.
	public static Renderer[] Apply(GameObject hero, string heroName)
	{
		HeroSkinInfo info = Find(heroName);
		Animator anim = hero.GetComponent<Animator>();
		if (info == null || info.avatar == null || anim == null)
			return null;

		foreach (SkinnedMeshRenderer r in hero.GetComponentsInChildren<SkinnedMeshRenderer>(true))
			r.enabled = false;
		// The 2015 heroes were imported with Optimize Game Objects, so their Animators drive a
		// hidden skeleton and would never move the new model's bones.
		if (!anim.hasTransformHierarchy)
			AnimatorUtility.DeoptimizeTransformHierarchy(hero);

		HeroSkinInfo model = Object.Instantiate(info, hero.transform, false);
		// The avatar finds its bones by their path from the Animator, so the model's top-level
		// children move up to sit directly under the hero, where the Animator is.
		var parts = new List<Transform>();
		foreach (Transform t in model.transform)
			parts.Add(t);
		foreach (Transform t in parts)
			t.SetParent(hero.transform, false);
		if (Application.isPlaying)
			Object.Destroy(model.gameObject);
		else
			Object.DestroyImmediate(model.gameObject);

		anim.avatar = info.avatar;
		anim.Rebind();

		var renderers = new List<Renderer>();
		foreach (Transform t in parts)
			renderers.AddRange(t.GetComponentsInChildren<Renderer>(true));
		CharacterBase cb = hero.GetComponent<CharacterBase>();
		if (cb != null)
			cb.flashRenderers = renderers.ToArray();
		return renderers.ToArray();
	}
}
