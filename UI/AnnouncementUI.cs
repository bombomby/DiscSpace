using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AnnouncementUI : MonoBehaviour
{
	private float TimeOut;
	private float TimeLeft;
	private string Message
	{
		set
		{
			Label.text = value;
		}
	}

	public Text Label;
	public RectTransform TimeoutTransform;

	public AudioSource Speaker;

	public enum SoundFX
	{
		None,
		Defence,
		Offence,
		Goal,
		GameOn,
		GameOver,
		Defeat,
		Victory,
		Tease,
		Death,
	}

	public SoundFXEntry GetSFX(SoundFX sfx)
	{
		List<SoundFXEntry> entries = AudioClips.FindAll(c => c.SFX == sfx);

		if (entries.Count == 0)
			return null;

		float totalProbability = entries.Sum(e => e.Probability);
		float selectedProbability = UnityEngine.Random.Range(0.0f, totalProbability);

		float currentSum = 0.0f;
		for (int i = 0; i < entries.Count; ++i)
		{
			currentSum += entries[i].Probability;
			if (selectedProbability < currentSum)
				return entries[i];
		}

		return entries.Last();
	}

	[Serializable]
	public class SoundFXEntry
	{
		public SoundFX SFX = SoundFX.None;
		public AudioClip Clip;
		public AudioClip ClipFX;
		public float Probability = 1.0f;
		public float Volume = 1.0f;
	}

	[SerializeField]
	public List<SoundFXEntry> AudioClips;

	class SFXClip
	{
		public AudioClip Clip;
		public float Volume = 1.0f;
	}

	Dictionary<String, SFXClip> AudioMap = new Dictionary<string, SFXClip>();

	public void Show(string message, float timeout, string sfx)
	{
		if (timeout > Mathf.Epsilon)
		{
			TimeOut = timeout;
			TimeLeft = timeout;
			Message = message;
			gameObject.SetActive(true);
		}

		SFXClip clip = null;
		if (!String.IsNullOrEmpty(sfx) && AudioMap.TryGetValue(sfx, out clip))
		{
			Speaker.PlayOneShot(clip.Clip, clip.Volume);
		}
	}

    // Start is called before the first frame update
    void Awake()
    {
        foreach (SoundFXEntry entry in AudioClips)
		{
			if (!AudioMap.ContainsKey(entry.Clip.name))
				AudioMap.Add(entry.Clip.name, new SFXClip() { Clip = entry.Clip, Volume = entry.Volume });

			if (!AudioMap.ContainsKey(entry.ClipFX.name))
				AudioMap.Add(entry.ClipFX.name, new SFXClip() { Clip = entry.ClipFX, Volume = entry.Volume });
		}
	}

    // Update is called once per frame
    void Update()
    {
		if (TimeLeft > 0.0f)
		{
			TimeLeft -= Time.deltaTime;

			TimeoutTransform.localScale = new Vector3(1.0f - TimeLeft / TimeOut, TimeoutTransform.localScale.y, TimeoutTransform.localScale.z);

			if (TimeLeft < 0.0f)
			{
				gameObject.SetActive(false);
			}
		}
	}
}
