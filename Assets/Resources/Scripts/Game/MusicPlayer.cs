using UnityEngine;

/// Background music that lives across scenes and crossfades between tracks. Tracks are
/// AudioClips under Resources (for example "Music/hub"); a missing track is silence.
public class MusicPlayer : MonoBehaviour
{
	static MusicPlayer instance;

	AudioSource[] sources;
	int active;
	string currentTrack;
	float fadeTime = 1.5f;

	public static float MusicVolume
	{
		get { return PlayerPrefs.GetFloat("musicVolume", 0.55f); }
		set { PlayerPrefs.SetFloat("musicVolume", value); PlayerPrefs.Save(); }
	}

	public static float MasterVolume
	{
		get { return PlayerPrefs.GetFloat("masterVolume", 0.9f); }
		set { PlayerPrefs.SetFloat("masterVolume", value); AudioListener.volume = value; PlayerPrefs.Save(); }
	}

	static void Ensure()
	{
		if (instance != null)
			return;
		var go = new GameObject("MusicPlayer");
		DontDestroyOnLoad(go);
		instance = go.AddComponent<MusicPlayer>();
		instance.sources = new[] { go.AddComponent<AudioSource>(), go.AddComponent<AudioSource>() };
		foreach (AudioSource s in instance.sources)
		{
			s.loop = true;
			s.playOnAwake = false;
			s.volume = 0.0f;
		}
		AudioListener.volume = MasterVolume;
	}

	public static void Play(string track, bool loop = true)
	{
		Ensure();
		if (track == instance.currentTrack)
			return;
		instance.currentTrack = track;
		AudioClip clip = string.IsNullOrEmpty(track) ? null : Resources.Load<AudioClip>(track);
		instance.active = 1 - instance.active;
		AudioSource next = instance.sources[instance.active];
		next.clip = clip;
		next.loop = loop;
		next.volume = 0.0f;
		if (clip != null)
			next.Play();
	}

	public static void Stop()
	{
		Play(null);
	}

	void Update()
	{
		float step = Time.unscaledDeltaTime / fadeTime;
		for (int i = 0; i < sources.Length; i++)
		{
			float target = i == active && sources[i].clip != null ? MusicVolume : 0.0f;
			sources[i].volume = Mathf.MoveTowards(sources[i].volume, target, step * Mathf.Max(MusicVolume, 0.05f));
			if (i != active && sources[i].volume <= 0.0f && sources[i].isPlaying)
				sources[i].Stop();
		}
	}
}
