using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioEvents : MonoBehaviour
{
	void OnFootstep()
	{
		if (transform.parent != null)
		{
			transform.parent.GetComponent<PlayerAudio>().OnAudioEvent(PlayerAudio.EventType.Footsteps);
		}
	}
}
