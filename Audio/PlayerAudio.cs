using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
	AudioSource AS;
	PlayerController PC;

	[Serializable]
	public class Entry
	{
		public EventType Type;
		public AudioClip Clip;
		public float Volume;
	}

	public List<Entry> AudioEntries;

	// Start is called before the first frame update
	void Awake()
    {
		AS = GetComponent<AudioSource>();
		PC = GetComponent<PlayerController>();
    }

	public enum EventType
	{
		Footsteps,
		Throw,
		Jump,
		Grounded,
	}

	private void PlaySound(EventType type)
	{
		foreach (Entry entry in AudioEntries)
			if (entry.Type == type)
				AS.PlayOneShot(entry.Clip, entry.Volume);
	}

	public void OnAudioEvent(EventType type)
	{
		if (type == EventType.Footsteps && !PC.IsGrounded)
			return;

		PlaySound(type);
	}
}
