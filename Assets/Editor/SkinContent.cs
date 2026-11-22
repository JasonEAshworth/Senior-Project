using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

/// Builds enemy skins from Asset Store packs kept in Assets/Store. That folder is git-ignored
/// because the store license forbids sharing the files, so a pack that is not installed is
/// skipped and the game keeps its zombies for those skins. Each skin is a pack prefab with its
/// own animator override and the measurements SkinSwap needs.
public static class SkinContent
{
	const string Store = "Assets/Store";
	const string Out = Store + "/Resources/Skins";
	const string Controllers = Store + "/Skins";
	const string Skeletons = Store + "/DungeonCharacters/Skeletons";
	const string Spiders = Store + "/GiantSpider";
	const string Monsters = Store + "/LowPolyMonsters";

	// Store packs import to fixed paths; each moves under Assets/Store so one ignore rule covers it.
	static readonly string[][] Packs = {
		new[] { "Assets/DungeonCharacters", Store + "/DungeonCharacters" },
		new[] { "Assets/GiantSpider", Spiders },
		new[] { "Assets/LowPoly Fantasy Monsters Pack Ver1.0_Demo", Monsters },
	};

	// Animator states. Every skin fills all twelve; a skin with fewer clips repeats them.
	static readonly string[] Slots = { "Idle", "Walk", "Run", "Attack0", "Attack1", "Attack2", "Hit0", "Hit1", "Hit2", "Die0", "Die1", "Die2" };
	const int FirstAttack = 3, FirstHit = 6, FirstDie = 9, Variations = 3;

	/// Where a skin's clips come from: .anim assets, or model files (the first clip inside).
	class Clips
	{
		public string idle, walk, run;
		public string[] attacks, hits, deaths;
	}

	class Skin
	{
		public string name, prefab;
		public Clips clips;
		public float heightScale = 1.0f;     // the skin's height next to the zombie it covers
		public float walkSpeed, runSpeed;     // m/s at the prefab's size; 0 measures them from the clips
	}

	static Clips SkeletonSet(string folder, string prefix, int attacks)
	{
		string p = Skeletons + "/Animation/" + folder + "/" + prefix;
		string[] abc = { "A", "B", "C" };
		return new Clips
		{
			idle = p + "idle_A.FBX", walk = p + "walk.FBX", run = p + "run.FBX",
			attacks = abc.Take(attacks).Select(x => p + "attack_" + x + ".FBX").ToArray(),
			hits = abc.Select(x => p + "damage_" + x + ".FBX").ToArray(),
			deaths = abc.Select(x => p + "death_" + x + ".FBX").ToArray(),
		};
	}

	static Clips MonsterSet(string monster, string attack)
	{
		string p = Monsters + "/Animations/Mecanim/" + monster + "/";
		return new Clips
		{
			idle = p + "idle01.anim", walk = p + "walk.anim", run = p + "run.anim",
			attacks = new[] { p + attack + ".anim" }, hits = new[] { p + "damage.anim" }, deaths = new[] { p + "dead.anim" },
		};
	}

	// The spiders have no run; the walk plays faster instead.
	static readonly Clips SpiderSet = new Clips
	{
		idle = Spiders + "/Animation/idle.anim", walk = Spiders + "/Animation/walk.anim", run = null,
		attacks = new[] { Spiders + "/Animation/bite.anim" },
		hits = new[] { Spiders + "/Animation/getHit.anim" },
		deaths = new[] { Spiders + "/Animation/die.anim" },
	};

	static readonly Skin[] Skins = {
		new Skin { name = "SkeletonRaider", prefab = Skeletons + "/Prefabs/Skeleton_A.prefab", clips = SkeletonSet("1_one_handed", "DS_onehand_", 3) },
		new Skin { name = "SkeletonGuard", prefab = Skeletons + "/Prefabs/Skeleton_B.prefab", clips = SkeletonSet("2_shield", "DS_shield_", 3) },
		new Skin { name = "SkeletonSoldier", prefab = Skeletons + "/Prefabs/Skeleton_C.prefab", clips = SkeletonSet("2_shield", "DS_shield_", 3) },
		new Skin { name = "SkeletonKnight", prefab = Skeletons + "/Prefabs/Skeleton_F.prefab", clips = SkeletonSet("2_shield", "DS_shield_", 3) },
		new Skin { name = "SkeletonBrute", prefab = Skeletons + "/Prefabs/Skeleton_TwoHanded_A.prefab", clips = SkeletonSet("3_two_handed", "DS_twohanded_", 2) },
		new Skin { name = "SkeletonWarden", prefab = Skeletons + "/Prefabs/Skeleton_TwoHanded_B.prefab", clips = SkeletonSet("3_two_handed", "DS_twohanded_", 2) },
		new Skin { name = "SkeletonArcher", prefab = Skeletons + "/Prefabs/Skeleton_archer_A.prefab", clips = SkeletonSet("4_bow", "DS_bow_", 1) },
		new Skin { name = "SkeletonRanger", prefab = Skeletons + "/Prefabs/Skeleton_archer_B.prefab", clips = SkeletonSet("4_bow", "DS_bow_", 1) },
		new Skin { name = "GiantSpider", prefab = Spiders + "/Prefabs/spider_low_004.prefab", clips = SpiderSet, heightScale = 0.32f },
		new Skin { name = "Tarantula", prefab = Spiders + "/Prefabs/tarantula_low_004.prefab", clips = SpiderSet, heightScale = 0.22f },
		new Skin { name = "Goblin", prefab = Monsters + "/Prefabs/goblin_01_Mecanim.prefab", clips = MonsterSet("goblin", "attack01"), heightScale = 0.7f },
		new Skin { name = "Hobgoblin", prefab = Monsters + "/Prefabs/Hobgoblin_002_Mecanim.prefab", clips = MonsterSet("hobgoblin", "attack02") },
		new Skin { name = "Troll", prefab = Monsters + "/Prefabs/troll_02_01_Mecanim.prefab", clips = MonsterSet("troll", "attack03"), heightScale = 1.35f },
		new Skin { name = "Wolf", prefab = Monsters + "/Prefabs/wolf_02_Mecanim.prefab", clips = MonsterSet("wolf", "attack01"), heightScale = 0.75f },
	};

	[MenuItem("Tools/Rise of the Restless/Rebuild Enemy Skins")]
	public static void Rebuild()
	{
		Build(true);
	}

	public static void Build(bool force = false)
	{
		foreach (string[] pack in Packs)
			AdoptPack(pack[0], pack[1]);
		if (AssetDatabase.IsValidFolder(Store))
			ProjectTools.CrunchWebGLTextures(Store);
		AnimatorController baseController = null;
		int missing = 0;
		foreach (Skin s in Skins)
		{
			if (AssetDatabase.LoadAssetAtPath<GameObject>(s.prefab) == null)
			{
				missing++;
				continue;
			}
			string path = Out + "/" + s.name + ".prefab";
			if (!force && File.Exists(path))
				continue;
			AnimationClip[] clips = Resolve(s.clips);
			if (clips.Any(c => c == null))
			{
				Debug.LogError("[content] skin " + s.name + " is missing clips; skipped");
				continue;
			}
			if (baseController == null)
			{
				EnsureFolder(Out);
				baseController = EnsureController(force);
			}
			BuildSkin(s, Override(s.name, baseController, clips), clips, path);
		}
		if (missing > 0)
			Debug.Log("[content] " + missing + " skins skipped: their store packs are not installed, so those enemies stay zombies");
		AssetDatabase.SaveAssets();
	}

	/// MoveAsset keeps GUIDs, so a pack's own references survive the move.
	static void AdoptPack(string from, string to)
	{
		if (!AssetDatabase.IsValidFolder(from) || AssetDatabase.IsValidFolder(to))
			return;
		EnsureFolder(Path.GetDirectoryName(to).Replace('\\', '/'));
		string error = AssetDatabase.MoveAsset(from, to);
		if (!string.IsNullOrEmpty(error))
			Debug.LogError("[content] could not move " + from + ": " + error);
	}

	static void EnsureFolder(string path)
	{
		if (AssetDatabase.IsValidFolder(path))
			return;
		string parent = Path.GetDirectoryName(path).Replace('\\', '/');
		EnsureFolder(parent);
		AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
	}

	static AnimationClip[] Resolve(Clips c)
	{
		var r = new AnimationClip[Slots.Length];
		r[0] = Load(c.idle);
		r[1] = Load(c.walk);
		r[2] = c.run != null ? Load(c.run) : r[1];
		for (int i = 0; i < Variations; i++)
		{
			r[FirstAttack + i] = Load(c.attacks[i % c.attacks.Length]);
			r[FirstHit + i] = Load(c.hits[i % c.hits.Length]);
			r[FirstDie + i] = Load(c.deaths[i % c.deaths.Length]);
		}
		return r;
	}

	static AnimationClip Load(string path)
	{
		AnimationClip clip = path.EndsWith(".anim")
			? AssetDatabase.LoadAssetAtPath<AnimationClip>(path)
			: AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().FirstOrDefault(c => !c.name.StartsWith("__preview__"));
		if (clip == null)
			Debug.LogError("[content] missing clip " + path);
		return clip;
	}

	/// Empty stand-ins for the twelve states. Every skin overrides all of them, so the shared
	/// controller does not depend on any one pack being installed.
	static AnimationClip[] SlotClips()
	{
		string folder = Controllers + "/Slots";
		EnsureFolder(folder);
		return Slots.Select(n =>
		{
			string p = folder + "/" + n + ".anim";
			AnimationClip c = AssetDatabase.LoadAssetAtPath<AnimationClip>(p);
			if (c == null)
			{
				c = new AnimationClip();
				AssetDatabase.CreateAsset(c, p);
			}
			return c;
		}).ToArray();
	}

	static AnimatorController EnsureController(bool force)
	{
		string path = Controllers + "/SkinBase.controller";
		EnsureFolder(Controllers);
		AnimatorController existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
		if (existing != null && !force)
			return existing;
		if (existing != null)
			AssetDatabase.DeleteAsset(path);

		AnimatorController c = AnimatorController.CreateAnimatorControllerAtPath(path);
		foreach (string p in new[] { "walking", "running" })
			c.AddParameter(p, AnimatorControllerParameterType.Bool);
		foreach (string p in new[] { "attack", "hit", "die" })
			c.AddParameter(p, AnimatorControllerParameterType.Trigger);
		foreach (string p in new[] { "attackIndex", "hitIndex", "dieIndex" })
			c.AddParameter(p, AnimatorControllerParameterType.Int);
		c.AddParameter(new AnimatorControllerParameter { name = "moveSpeed", type = AnimatorControllerParameterType.Float, defaultFloat = 1.0f });

		AnimationClip[] slotClips = SlotClips();
		AnimatorStateMachine sm = c.layers[0].stateMachine;
		var s = new AnimatorState[Slots.Length];
		for (int i = 0; i < Slots.Length; i++)
		{
			s[i] = sm.AddState(Slots[i]);
			s[i].motion = slotClips[i];
		}
		AnimatorState idle = s[0], walk = s[1], run = s[2];
		walk.speedParameterActive = run.speedParameterActive = true;
		walk.speedParameter = run.speedParameter = "moveSpeed";
		sm.defaultState = idle;

		Link(idle, walk, AnimatorConditionMode.If, "walking");
		Link(walk, idle, AnimatorConditionMode.IfNot, "walking");
		Link(walk, run, AnimatorConditionMode.If, "running");
		Link(run, walk, AnimatorConditionMode.IfNot, "running");
		Link(run, idle, AnimatorConditionMode.IfNot, "walking");
		for (int i = 0; i < Variations; i++)
		{
			FromAny(sm, s[FirstAttack + i], "attack", "attackIndex", i, 0.08f);
			Back(s[FirstAttack + i], idle, 0.9f);
			FromAny(sm, s[FirstHit + i], "hit", "hitIndex", i, 0.05f);
			Back(s[FirstHit + i], idle, 0.85f);
			// A death has no way back.
			FromAny(sm, s[FirstDie + i], "die", "dieIndex", i, 0.1f);
		}
		return c;
	}

	static void Link(AnimatorState from, AnimatorState to, AnimatorConditionMode mode, string param)
	{
		AnimatorStateTransition t = from.AddTransition(to);
		t.AddCondition(mode, 0, param);
		t.hasExitTime = false;
		t.duration = 0.15f;
	}

	static void FromAny(AnimatorStateMachine sm, AnimatorState to, string trigger, string index, int value, float duration)
	{
		AnimatorStateTransition t = sm.AddAnyStateTransition(to);
		t.AddCondition(AnimatorConditionMode.If, 0, trigger);
		t.AddCondition(AnimatorConditionMode.Equals, value, index);
		t.duration = duration;
		t.canTransitionToSelf = false;
	}

	static void Back(AnimatorState from, AnimatorState to, float exitTime)
	{
		AnimatorStateTransition t = from.AddTransition(to);
		t.hasExitTime = true;
		t.exitTime = exitTime;
		t.duration = 0.15f;
	}

	static RuntimeAnimatorController Override(string name, AnimatorController baseController, AnimationClip[] clips)
	{
		string path = Controllers + "/" + name + ".overrideController";
		AnimatorOverrideController o = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(path);
		if (o == null)
		{
			o = new AnimatorOverrideController(baseController);
			AssetDatabase.CreateAsset(o, path);
		}
		o.runtimeAnimatorController = baseController;
		AnimationClip[] slotClips = SlotClips();
		o.ApplyOverrides(slotClips.Select((slot, i) => new KeyValuePair<AnimationClip, AnimationClip>(slot, clips[i])).ToList());
		EditorUtility.SetDirty(o);
		return o;
	}

	static void BuildSkin(Skin s, RuntimeAnimatorController controller, AnimationClip[] clips, string path)
	{
		GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(s.prefab));
		PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
		go.name = s.name;
		go.transform.position = Vector3.zero;
		// Pack demo scripts would run in game, and some use the old Input class this project
		// does not allow. A legacy Animation component would fight the Animator.
		foreach (MonoBehaviour mb in go.GetComponentsInChildren<MonoBehaviour>(true))
			Object.DestroyImmediate(mb);
		foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
			GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
		foreach (Animation legacy in go.GetComponentsInChildren<Animation>(true))
			Object.DestroyImmediate(legacy);
		foreach (Renderer r in go.GetComponentsInChildren<Renderer>(true))
			r.sharedMaterials = r.sharedMaterials.Select(BuiltIn).ToArray();

		Animator a = go.GetComponent<Animator>();
		if (a == null)
			a = go.AddComponent<Animator>();
		if (a.avatar == null)
			Debug.LogWarning("[content] skin " + s.name + " has no avatar");
		a.runtimeAnimatorController = controller;
		a.applyRootMotion = false;
		a.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

		SkinInfo info = go.AddComponent<SkinInfo>();
		info.height = BodyHeight(go);
		info.heightScale = s.heightScale;
		float hips = HipHeight(a);
		float fallback = info.height / 1.8f;
		info.walkSpeed = s.walkSpeed > 0 ? s.walkSpeed : Pace(clips[1], hips, 1.4f * fallback);
		info.runSpeed = s.runSpeed > 0 ? s.runSpeed : Pace(clips[2], hips, 4.0f * fallback);
		if (clips[2] == clips[1])
			info.runSpeed = info.walkSpeed * 2.2f;
		Debug.Log("[content] skin " + s.name + ": height " + info.height.ToString("0.00") + ", hips " + hips.ToString("0.00") +
			", walk " + info.walkSpeed.ToString("0.00") + ", run " + info.runSpeed.ToString("0.00"));
		PrefabUtility.SaveAsPrefabAsset(go, path);
		Object.DestroyImmediate(go);
	}

	/// Store packs often ship URP materials, which render magenta in this built-in pipeline
	/// project. Rebuild each as a Standard (Specular setup) material from its saved textures.
	static Material BuiltIn(Material source)
	{
		if (source == null)
			return null;
		string shader = source.shader != null ? source.shader.name : "";
		if (shader != "Hidden/InternalErrorShader" && !shader.StartsWith("Universal Render Pipeline"))
			return source;
		string folder = Controllers + "/Materials";
		string path = folder + "/" + source.name + ".mat";
		Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
		if (m != null)
			return m;
		EnsureFolder(folder);
		var so = new SerializedObject(source);
		m = new Material(Shader.Find("Standard (Specular setup)"));
		m.SetTexture("_MainTex", SavedTexture(so, "_BaseMap") ?? SavedTexture(so, "_MainTex"));
		m.SetColor("_Color", SavedColor(so, "_BaseColor", Color.white));
		Texture spec = SavedTexture(so, "_SpecGlossMap");
		if (spec != null)
		{
			m.SetTexture("_SpecGlossMap", spec);
			m.EnableKeyword("_SPECGLOSSMAP");
		}
		m.SetColor("_SpecColor", SavedColor(so, "_SpecColor", new Color(0.2f, 0.2f, 0.2f)));
		float smooth = SavedFloat(so, "_Smoothness", 0.3f);
		m.SetFloat("_Glossiness", smooth);
		m.SetFloat("_GlossMapScale", smooth);
		Texture normal = SavedTexture(so, "_BumpMap");
		if (normal != null)
		{
			m.SetTexture("_BumpMap", normal);
			m.EnableKeyword("_NORMALMAP");
		}
		AssetDatabase.CreateAsset(m, path);
		return m;
	}

	static SerializedProperty Saved(SerializedObject so, string list, string name)
	{
		SerializedProperty entries = so.FindProperty("m_SavedProperties." + list);
		if (entries == null)
			return null;
		for (int i = 0; i < entries.arraySize; i++)
		{
			SerializedProperty e = entries.GetArrayElementAtIndex(i);
			if (e.FindPropertyRelative("first").stringValue == name)
				return e.FindPropertyRelative("second");
		}
		return null;
	}

	static Texture SavedTexture(SerializedObject so, string name)
	{
		SerializedProperty p = Saved(so, "m_TexEnvs", name);
		return p != null ? p.FindPropertyRelative("m_Texture").objectReferenceValue as Texture : null;
	}

	static Color SavedColor(SerializedObject so, string name, Color fallback)
	{
		SerializedProperty p = Saved(so, "m_Colors", name);
		return p != null ? p.colorValue : fallback;
	}

	static float SavedFloat(SerializedObject so, string name, float fallback)
	{
		SerializedProperty p = Saved(so, "m_Floats", name);
		return p != null ? p.floatValue : fallback;
	}

	/// The body mesh alone (the one with the most vertices): weapons and helmets would change
	/// the size the skin is scaled to.
	static float BodyHeight(GameObject go)
	{
		SkinnedMeshRenderer body = go.GetComponentsInChildren<SkinnedMeshRenderer>(true)
			.Where(r => r.sharedMesh != null)
			.OrderByDescending(r => r.sharedMesh.vertexCount)
			.FirstOrDefault();
		if (body == null)
			return 1.8f;
		// The posed vertices themselves. Renderer and mesh bounds both sit in rig space, and some
		// rigs are turned on their side or scaled, so a wolf's length can read as its height.
		var baked = new Mesh();
		// Without scale: the result is then in the renderer's rotated, unscaled space.
		body.BakeMesh(baked, false);
		Matrix4x4 m = Matrix4x4.TRS(body.transform.position, body.transform.rotation, Vector3.one);
		float lo = float.MaxValue, hi = float.MinValue;
		foreach (Vector3 v in baked.vertices)
		{
			float y = m.MultiplyPoint3x4(v).y;
			lo = Mathf.Min(lo, y);
			hi = Mathf.Max(hi, y);
		}
		Object.DestroyImmediate(baked);
		return hi > lo ? hi - lo : 1.8f;
	}

	static float HipHeight(Animator a)
	{
		Transform hips = a.isHuman ? a.GetBoneTransform(HumanBodyBones.Hips) : null;
		return hips != null ? hips.position.y - a.transform.position.y : 0.0f;
	}

	/// Humanoid root motion is stored per unit of hip height, so the clip's world speed is its
	/// average speed times the hip height. Falls back when the clip carries no forward motion,
	/// as in-place clips on generic rigs do.
	static float Pace(AnimationClip clip, float hips, float fallback)
	{
		if (clip == null || hips <= 0.05f)
			return fallback;
		float pace = new Vector2(clip.averageSpeed.x, clip.averageSpeed.z).magnitude * hips;
		return pace > 0.2f && pace < 10.0f ? pace : fallback;
	}

	/// Renders every built skin beside the stock zombie to Build/skin_preview.png, to check
	/// facing, size, and gear without a full build. Sizes here are each pack's own, not the
	/// in-game size, which SkinSwap sets from heightScale.
	[MenuItem("Tools/Rise of the Restless/Render Skin Previews")]
	public static void RenderPreviews()
	{
		Build(true);
		EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
		RenderSettings.ambientLight = new Color(0.55f, 0.55f, 0.6f);
		Light sun = new GameObject("Sun").AddComponent<Light>();
		sun.type = LightType.Directional;
		sun.transform.rotation = Quaternion.Euler(40, 160, 0);

		const float Spacing = 2.4f;
		float x = 0.0f;
		var shown = new List<string> { "Assets/Resources/Prefabs/Enemies/Zombie/Restless-Melee.prefab" };
		shown.AddRange(Skins.Select(s => Out + "/" + s.name + ".prefab"));
		foreach (string path in shown)
		{
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			if (prefab == null)
				continue;
			GameObject g = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
			g.transform.position = new Vector3(x, 0, 0);
			// Show each skin at its in-game height next to a 1.8 m zombie.
			SkinInfo info = g.GetComponent<SkinInfo>();
			if (info != null && info.height > 0.01f)
				g.transform.localScale *= 1.8f * info.heightScale / info.height;
			x += Spacing;
		}

		Camera cam = new GameObject("Camera").AddComponent<Camera>();
		cam.clearFlags = CameraClearFlags.SolidColor;
		cam.backgroundColor = new Color(0.25f, 0.27f, 0.3f);
		cam.orthographic = true;
		float mid = (x - Spacing) / 2.0f;
		cam.transform.position = new Vector3(mid, 1.2f, 10.0f);
		cam.transform.LookAt(new Vector3(mid, 1.2f, 0.0f));
		int w = 3000, h = 400;
		cam.aspect = (float)w / h;
		cam.orthographicSize = Mathf.Max(1.6f, (x + Spacing) / (2.0f * cam.aspect));
		var rt = new RenderTexture(w, h, 24);
		cam.targetTexture = rt;
		cam.Render();
		RenderTexture.active = rt;
		var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
		tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
		tex.Apply();
		RenderTexture.active = null;
		string file = Path.GetFullPath(Path.Combine(Application.dataPath, "../Build/skin_preview.png"));
		File.WriteAllBytes(file, tex.EncodeToPNG());
		Debug.Log("[content] preview written to " + file);
	}
}
