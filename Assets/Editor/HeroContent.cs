using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

/// Builds hero models from Mixamo characters kept in Assets/Store/Heroes. Mixamo's license lets
/// a game ship the characters but not share the files, so that folder is git-ignored like the
/// store packs, and a hero whose file is missing keeps its 2015 mesh. HeroSkin puts the model on
/// the hero at spawn.
public static class HeroContent
{
	const string Folder = "Assets/Store/Heroes";
	const string Out = "Assets/Store/Resources/Heroes";
	const string HeroPrefabs = "Assets/Resources/Prefabs/Character/";
	const string Dagger = "Assets/Resources/Models/Props/Prop_Sm_WeaponDagger_1.fbx";

	/// A material rebuilt from the textures Mixamo embeds. Unity extracts them but does not link
	/// them back to the materials, so each one is named here.
	class Look
	{
		public string material, diffuse, normal, specular;
		public bool cutout;           // eyelashes: a texture with holes
		public float smoothness = 0.25f;
	}

	class Hero
	{
		public string name, model, prefab;
		public bool daggers;
		public Look[] looks;
		// Parts the hero cannot use: a loose arrow that only Mixamo's own clips move (the hero
		// clips leave it at the feet), and an eye-gloss shell that renders as solid white.
		public string[] dropMaterials = new string[0];
	}

	static readonly Hero[] Heroes = {
		new Hero
		{
			name = "Rogue", model = Folder + "/Heraklios By A. Dizon.fbx", prefab = HeroPrefabs + "Rogue/Rogue.prefab", daggers = true,
			looks = new[] {
				new Look { material = "BattalionLeader_MAT", diffuse = "battalion-leader-heraklios_diffuse", normal = "battalion-leader-heraklios_normal", specular = "battalion-leader-heraklios_specular" },
				new Look { material = "phong1", diffuse = "battalion-leader-heraklios-body_diffuse" },
			},
		},
		new Hero
		{
			name = "Woodsman", model = Folder + "/Erika Archer With Bow Arrow.fbx", prefab = HeroPrefabs + "WoodsMan/Woodsman.prefab",
			looks = new[] {
				new Look { material = "Akai_MAT1", diffuse = "Erika_Archer_Clothes_diffuse", normal = "Erika_Archer_Clothes_normal" },
				new Look { material = "Body_MAT1", diffuse = "FemaleFitA_Body_diffuse", normal = "FemaleFitA_StdNM", smoothness = 0.35f },
				new Look { material = "Bow_MAT", diffuse = "Bow_DIFF", normal = "Bow_NM" },
				new Look { material = "phong1", diffuse = "FemaleFitA_eyelash_diffuse", cutout = true },
			},
			dropMaterials = new[] { "Arrow_MAT", "EyeSpec_MAT1" },
		},
	};

	[MenuItem("Tools/Rise of the Restless/Rebuild Hero Models")]
	public static void Rebuild()
	{
		Build(true);
	}

	/// Portraits are rendered only when missing unless forced: rendering needs a scratch scene.
	public static void Build(bool portraits = false)
	{
		float target = TargetHeight();
		foreach (Hero h in Heroes)
		{
			string path = Out + "/" + h.name + ".prefab";
			if (AssetDatabase.LoadAssetAtPath<GameObject>(h.model) == null)
			{
				// A model built from a file that is gone would show a hero with no body.
				if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
					AssetDatabase.DeleteAsset(path);
				Debug.Log("[content] " + h.name + " model is not installed; the hero keeps its 2015 mesh");
				continue;
			}
			Import(h, target);
			BuildModel(h, path, portraits);
		}
		AssetDatabase.SaveAssets();
	}

	/// The heroes' capsules are 2 m tall; the 2015 bodies stand a little under that.
	static float TargetHeight()
	{
		return 1.95f;
	}

	static void Import(Hero h, float target)
	{
		var imp = (ModelImporter)AssetImporter.GetAtPath(h.model);
		bool changed = false;
		if (imp.animationType != ModelImporterAnimationType.Human || imp.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
		{
			imp.animationType = ModelImporterAnimationType.Human;
			imp.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
			changed = true;
		}
		if (imp.importAnimation)
		{
			imp.importAnimation = false;
			changed = true;
		}
		if (imp.meshCompression != ModelImporterMeshCompression.Medium)
		{
			imp.meshCompression = ModelImporterMeshCompression.Medium;   // keeps the WebGL data file under 100 MB
			changed = true;
		}
		// Mixamo embeds the textures in the file. Unity needs them as files to show them, and
		// looks for them in the .fbm folder beside the model.
		string textures = Path.ChangeExtension(h.model, ".fbm");
		if (!AssetDatabase.IsValidFolder(textures))
		{
			SkinContent.EnsureFolder(textures);
			imp.ExtractTextures(textures);
			AssetDatabase.Refresh();
			changed = true;
		}
		if (changed)
			imp.SaveAndReimport();

		float height = Height(AssetDatabase.LoadAssetAtPath<GameObject>(h.model), h);
		float scale = imp.globalScale * target / height;
		if (Mathf.Abs(scale - imp.globalScale) > 0.005f * imp.globalScale)
		{
			imp.globalScale = scale;
			imp.SaveAndReimport();
		}
		FixSkeleton(imp, h.model);
		Debug.Log("[content] hero " + h.name + ": " + height.ToString("0.00") + " m tall at import scale, scaled by " + imp.globalScale.ToString("0.000") + " to " + target.ToString("0.00") + " m");
	}

	/// The avatar's skeleton in the import settings can come out in the file's centimeters while
	/// the bones are in meters, which makes the avatar a hundred times too large and throws the
	/// body far above the hero. Each entry is reset to its bone's real local pose.
	static void FixSkeleton(ModelImporter imp, string model)
	{
		GameObject go = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(model));
		var bones = go.GetComponentsInChildren<Transform>(true).GroupBy(t => t.name).ToDictionary(g => g.Key, g => g.First());
		HumanDescription hd = imp.humanDescription;
		bool off = false;
		SkeletonBone[] skeleton = hd.skeleton.Select(b =>
		{
			if (bones.TryGetValue(b.name, out Transform t) && t != go.transform)
			{
				off |= Vector3.Distance(b.position, t.localPosition) > 0.001f;
				b.position = t.localPosition;
				b.rotation = t.localRotation;
				b.scale = t.localScale;
			}
			return b;
		}).ToArray();
		Object.DestroyImmediate(go);
		if (!off)
			return;
		hd.skeleton = skeleton;
		imp.humanDescription = hd;
		imp.SaveAndReimport();
		Debug.Log("[content] reset the avatar skeleton of " + model);
	}

	/// Top to bottom of the posed vertices of every part the hero keeps.
	static float Height(GameObject model, Hero h)
	{
		GameObject go = Object.Instantiate(model);
		float lo = float.MaxValue, hi = float.MinValue;
		foreach (SkinnedMeshRenderer r in go.GetComponentsInChildren<SkinnedMeshRenderer>(true))
		{
			if (r.sharedMesh == null || Dropped(r, h))
				continue;
			var baked = new Mesh();
			r.BakeMesh(baked, true);
			foreach (Vector3 v in baked.vertices)
			{
				float y = r.transform.TransformPoint(v).y;
				lo = Mathf.Min(lo, y);
				hi = Mathf.Max(hi, y);
			}
			Object.DestroyImmediate(baked);
		}
		Object.DestroyImmediate(go);
		return hi > lo ? hi - lo : 1.8f;
	}

	static bool Dropped(Renderer r, Hero h)
	{
		return r.sharedMaterials.Any(m => m != null && h.dropMaterials.Contains(m.name));
	}

	static void BuildModel(Hero h, string path, bool portraits)
	{
		GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(h.model);
		Avatar avatar = AssetDatabase.LoadAllAssetsAtPath(h.model).OfType<Avatar>().FirstOrDefault();
		if (avatar == null || !avatar.isHuman || !avatar.isValid)
		{
			Debug.LogError("[content] " + h.name + " model has no valid humanoid avatar; skipped");
			return;
		}
		GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(source);
		PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
		go.name = h.name;
		go.transform.position = Vector3.zero;
		// The hero's own Animator drives the model; a second one would fight it.
		Object.DestroyImmediate(go.GetComponent<Animator>());
		foreach (Renderer r in go.GetComponentsInChildren<Renderer>(true))
		{
			if (Dropped(r, h))
				Object.DestroyImmediate(r.gameObject);
			else
				r.sharedMaterials = r.sharedMaterials.Select(m => Material(h, m)).ToArray();
		}
		if (h.daggers)
		{
			AddDagger(go, avatar, "RightHand");
			AddDagger(go, avatar, "LeftHand");
		}
		HeroSkinInfo info = go.AddComponent<HeroSkinInfo>();
		info.avatar = avatar;
		SkinContent.EnsureFolder(Out);
		PrefabUtility.SaveAsPrefabAsset(go, path);
		Object.DestroyImmediate(go);

		string file = Out + "/" + h.name + "_profile.png";
		Texture2D portrait = AssetDatabase.LoadAssetAtPath<Texture2D>(file);
		if (portrait == null || portraits)
			Isolated(() => portrait = RenderPortrait(h, file));
		GameObject saved = PrefabUtility.LoadPrefabContents(path);
		saved.GetComponent<HeroSkinInfo>().portrait = portrait;
		PrefabUtility.SaveAsPrefabAsset(saved, path);
		PrefabUtility.UnloadPrefabContents(saved);
	}

	static Material Material(Hero h, Material source)
	{
		Look look = source != null ? h.looks.FirstOrDefault(l => l.material == source.name) : null;
		if (look == null)
			return source;
		string folder = Folder + "/Materials/" + h.name;
		SkinContent.EnsureFolder(folder);
		string path = folder + "/" + look.material + ".mat";
		Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
		bool created = m == null;
		if (created)
			m = new Material(Shader.Find("Standard (Specular setup)"));
		string textures = Path.ChangeExtension(h.model, ".fbm");
		m.SetTexture("_MainTex", Texture(textures, look.diffuse));
		m.color = Color.white;
		Texture normal = Texture(textures, look.normal);
		if (normal != null && AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(normal)) is TextureImporter ti && ti.textureType != TextureImporterType.NormalMap)
		{
			ti.textureType = TextureImporterType.NormalMap;
			ti.SaveAndReimport();
		}
		m.SetTexture("_BumpMap", normal);
		SetKeyword(m, "_NORMALMAP", normal != null);
		Texture spec = Texture(textures, look.specular);
		m.SetTexture("_SpecGlossMap", spec);
		SetKeyword(m, "_SPECGLOSSMAP", spec != null);
		m.SetColor("_SpecColor", new Color(0.12f, 0.12f, 0.12f));
		m.SetFloat("_Glossiness", look.smoothness);
		m.SetFloat("_GlossMapScale", look.smoothness);
		m.SetFloat("_Mode", look.cutout ? 1 : 0);
		m.SetFloat("_Cutoff", 0.4f);
		SetKeyword(m, "_ALPHATEST_ON", look.cutout);
		m.renderQueue = look.cutout ? (int)UnityEngine.Rendering.RenderQueue.AlphaTest : -1;
		if (created)
			AssetDatabase.CreateAsset(m, path);
		else
			EditorUtility.SetDirty(m);
		return m;
	}

	static void SetKeyword(Material m, string keyword, bool on)
	{
		if (on)
			m.EnableKeyword(keyword);
		else
			m.DisableKeyword(keyword);
	}

	static Texture Texture(string folder, string name)
	{
		if (string.IsNullOrEmpty(name))
			return null;
		foreach (string ext in new[] { ".png", ".jpg", ".tga" })
		{
			Texture t = AssetDatabase.LoadAssetAtPath<Texture>(folder + "/" + name + ext);
			if (t != null)
				return t;
		}
		Debug.LogError("[content] missing hero texture " + folder + "/" + name);
		return null;
	}

	static Transform Bone(GameObject go, Avatar avatar, string human)
	{
		HumanBone b = avatar.humanDescription.human.FirstOrDefault(x => x.humanName == human);
		return string.IsNullOrEmpty(b.boneName) ? null : go.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == b.boneName);
	}

	/// The repo's own dagger, held in a fist with the blade out of the thumb side.
	static void AddDagger(GameObject go, Avatar avatar, string hand)
	{
		Transform bone = Bone(go, avatar, hand);
		GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Dagger);
		if (bone == null || prefab == null)
			return;
		GameObject d = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
		PrefabUtility.UnpackPrefabInstance(d, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
		d.name = "Dagger";
		Material steel = DaggerMaterial();
		foreach (Renderer r in d.GetComponentsInChildren<Renderer>(true))
			r.sharedMaterial = steel;
		// Placed in world space while the model stands in its T-pose, palms down: the fist sits
		// just past the hand bone toward the fingers, and the thumb side faces forward. Hand bone
		// axes differ between rigs, so they are not trusted. The prop's pivot is its pommel.
		const float Scale = 0.55f;
		Transform middle = bone.Cast<Transform>().FirstOrDefault(t => t.name.Contains("Middle"));
		Vector3 fingers = middle != null ? (middle.position - bone.position).normalized : bone.up;
		Vector3 grip = bone.position + fingers * 0.07f + Vector3.down * 0.02f;
		d.transform.SetPositionAndRotation(grip - Vector3.forward * 0.07f * Scale, Quaternion.LookRotation(fingers, Vector3.forward));
		d.transform.localScale = Vector3.one * Scale;
		d.transform.SetParent(bone, true);
	}

	// The prop's own material lost its texture in 2015; the texture is still in the repo.
	static Material DaggerMaterial()
	{
		string path = Folder + "/Materials/Dagger.mat";
		Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
		if (m != null)
			return m;
		SkinContent.EnsureFolder(Folder + "/Materials");
		m = new Material(Shader.Find("Standard (Specular setup)"));
		m.SetTexture("_MainTex", AssetDatabase.LoadAssetAtPath<Texture>("Assets/Resources/Textures/Props/Txt_WeaponSet_1_A_diff.png"));
		Texture normal = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Resources/Textures/Props/Txt_WeaponSet_1_A_norm.png");
		m.SetTexture("_BumpMap", normal);
		SetKeyword(m, "_NORMALMAP", normal != null);
		m.SetColor("_SpecColor", new Color(0.35f, 0.35f, 0.35f));
		m.SetFloat("_Glossiness", 0.55f);
		AssetDatabase.CreateAsset(m, path);
		return m;
	}

	/// Poses a hero with its model at a clip's time, for the portrait and the preview.
	static GameObject PosedHero(Hero h, string clipPart, float t, bool withSkin = true)
	{
		GameObject hero = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(h.prefab));
		PrefabUtility.UnpackPrefabInstance(hero, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
		foreach (MonoBehaviour mb in hero.GetComponentsInChildren<MonoBehaviour>(true))
			mb.enabled = false;
		// The facing ring, the aim line, and the arrow template only clutter the picture.
		foreach (Renderer r in hero.GetComponentsInChildren<Renderer>(true))
			if (!(r is SkinnedMeshRenderer))
				r.enabled = false;
		hero.transform.position = Vector3.zero;
		if (withSkin)
			HeroSkin.Apply(hero, h.name);
		Animator anim = hero.GetComponent<Animator>();
		// Nothing has rendered yet, so a culling Animator would skip the pose.
		anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		AnimationClip clip = clipPart == null ? null : anim.runtimeAnimatorController.animationClips.FirstOrDefault(c => c.name.ToLower().Contains(clipPart.ToLower()));
		if (clip != null && withSkin)
		{
			PlayableGraph graph = PlayableGraph.Create("pose");
			var output = AnimationPlayableOutput.Create(graph, "pose", anim);
			var play = AnimationClipPlayable.Create(graph, clip);
			play.SetTime(t * clip.length);
			output.SetSourcePlayable(play);
			graph.Evaluate();
			graph.Destroy();
		}
		return hero;
	}

	/// Runs work that needs a scratch scene, then reopens the scenes that were open. Unsaved
	/// changes are offered for saving first, since the scratch scene replaces them.
	static void Isolated(System.Action work)
	{
		if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			return;
		var setup = EditorSceneManager.GetSceneManagerSetup();
		try
		{
			work();
		}
		finally
		{
			if (setup.Length > 0)
				EditorSceneManager.RestoreSceneManagerSetup(setup);
		}
	}

	static void Stage()
	{
		EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
		RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.55f);
		Light sun = new GameObject("Sun").AddComponent<Light>();
		sun.type = LightType.Directional;
		sun.intensity = 1.1f;
		sun.transform.rotation = Quaternion.Euler(35, 150, 0);
	}

	static Texture2D Render(Camera cam, int w, int h)
	{
		var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
		rt.antiAliasing = 4;
		cam.targetTexture = rt;
		cam.Render();
		RenderTexture.active = rt;
		var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
		tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
		tex.Apply();
		RenderTexture.active = null;
		cam.targetTexture = null;
		Object.DestroyImmediate(rt);
		return tex;
	}

	/// A head shot in a dark disc on a clear square, laid out like the 2015 portraits so the HUD
	/// emblem frames it the same way.
	static Texture2D RenderPortrait(Hero h, string file)
	{
		Stage();
		// The rest pose: the idle clips turn the head away from the camera.
		GameObject hero = PosedHero(h, null, 0.0f);
		Animator anim = hero.GetComponent<Animator>();
		Transform head = anim.GetBoneTransform(HumanBodyBones.Head);
		Vector3 face = head.position + Vector3.up * 0.1f;
		Camera cam = new GameObject("Camera").AddComponent<Camera>();
		cam.clearFlags = CameraClearFlags.SolidColor;
		cam.backgroundColor = new Color(0.08f, 0.08f, 0.09f, 1.0f);
		cam.fieldOfView = 18.0f;
		Vector3 view = Quaternion.Euler(0, -22, 0) * hero.transform.forward;
		cam.transform.position = face + view * 2.3f + Vector3.up * 0.1f;
		cam.transform.LookAt(face);
		const int Size = 540;
		Texture2D tex = Render(cam, Size, Size);
		Vector2 c = new Vector2(266, 266);
		const float R = 176.0f;
		Color32[] px = tex.GetPixels32();
		for (int y = 0; y < Size; y++)
			for (int x = 0; x < Size; x++)
			{
				float d = Vector2.Distance(new Vector2(x, y), c);
				byte a = (byte)(255 * Mathf.Clamp01(R + 0.5f - d));
				int i = y * Size + x;
				px[i].a = a;
			}
		tex.SetPixels32(px);
		File.WriteAllBytes(file, tex.EncodeToPNG());
		Object.DestroyImmediate(tex);
		AssetDatabase.ImportAsset(file);
		var ti = (TextureImporter)AssetImporter.GetAtPath(file);
		ti.alphaIsTransparency = true;
		ti.mipmapEnabled = false;
		ti.SaveAndReimport();
		return AssetDatabase.LoadAssetAtPath<Texture2D>(file);
	}

	/// Renders the old and new heroes side by side in a few poses to Build/hero_preview.png,
	/// with the Warrior and the Sorceress for scale.
	[MenuItem("Tools/Rise of the Restless/Render Hero Previews")]
	public static void RenderPreviews()
	{
		Build(true);
		Stage();
		float x = 0.0f;
		const float Spacing = 1.9f;
		foreach (string other in new[] { "Warrior/Warrior.prefab", "Sorceress/Sorceress.prefab" })
		{
			GameObject g = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(HeroPrefabs + other));
			g.transform.position = new Vector3(x, 0, 0);
			x += Spacing;
		}
		foreach (Hero h in Heroes)
		{
			PosedHero(h, "", 0, false).transform.position = new Vector3(x, 0, 0);
			x += Spacing;
			if (HeroSkin.Find(h.name) == null)
				continue;
			foreach (var pose in new[] { ("idle", 0.3f), ("walk", 0.25f), ("run", 0.4f), ("attack", 0.45f), (h.daggers ? "stealth" : "shoot", 0.5f), (h.daggers ? "attack2" : "hawk", 0.5f) })
			{
				PosedHero(h, pose.Item1, pose.Item2).transform.position = new Vector3(x, 0, 0);
				x += Spacing;
			}
		}
		Camera cam = new GameObject("Camera").AddComponent<Camera>();
		cam.clearFlags = CameraClearFlags.SolidColor;
		cam.backgroundColor = new Color(0.25f, 0.27f, 0.3f);
		cam.orthographic = true;
		float mid = (x - Spacing) / 2.0f;
		cam.transform.position = new Vector3(mid, 1.6f, 10.0f);
		cam.transform.LookAt(new Vector3(mid, 1.1f, 0.0f));
		int w = 3600, hgt = 500;
		cam.aspect = (float)w / hgt;
		cam.orthographicSize = Mathf.Max(1.3f, (x + Spacing * 0.2f) / (2.0f * cam.aspect));
		Texture2D tex = Render(cam, w, hgt);
		string file = Path.GetFullPath(Path.Combine(Application.dataPath, "../Build/hero_preview.png"));
		File.WriteAllBytes(file, tex.EncodeToPNG());
		Debug.Log("[content] hero preview written to " + file);
	}
}
