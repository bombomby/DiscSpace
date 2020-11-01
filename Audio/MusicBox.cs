using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicBox : MonoBehaviour
{
	PhotonView PV;
	AudioSource AS;

	public List<AudioClip> ActiveTracks;
	public List<AudioClip> ChillTracks;
	public List<AudioClip> IntroTracks;

	Dictionary<string, AudioClip> Tracks = new Dictionary<string, AudioClip>();

	AudioClip CurrentClip;
	AudioClip TransitionClip;

	float TransitionRatio = 0.0f;
	const float TransitionTime = 1.0f;

	List<AudioClip> AllTracks;


	public void CmdPlayNext()
	{
		PV.RPC("RPC_PlayNext", RpcTarget.MasterClient);
	}

	public void CmdStop()
	{
		PV.RPC("RPC_Stop", RpcTarget.All);
	}

	[PunRPC]
	void RPC_Stop()
	{
		AS.Stop();
		AS.clip = null;
		AS.enabled = false;
		CurrentClip = null;
	}

	AudioClip GetRandomClip()
	{
		if (FrisbeeGame.Instance.CurrentState == FrisbeeGame.GameState.None)
			return IntroTracks[Random.Range(0, IntroTracks.Count)];

		if ((FrisbeeGame.Instance.CurrentState & FrisbeeGame.GameState.Game) != 0)
			return ActiveTracks[Random.Range(0, ActiveTracks.Count)];

		return ChillTracks[Random.Range(0, ChillTracks.Count)];
	}

	AudioClip GetClip(string name)
	{
		foreach (AudioClip clip in ActiveTracks)
			if (clip.name == name)
				return clip;

		foreach (AudioClip clip in ChillTracks)
			if (clip.name == name)
				return clip;

		return null;
	}

	[PunRPC]
	void RPC_PlayNext()
	{
		if (PV.IsMine)
		{
			AudioClip clip = GetRandomClip();
			PV.RPC("RPC_Play", RpcTarget.All, clip.name);
		}
	}

	[PunRPC]
	void RPC_Play(string name)
	{
		AudioClip clip = GetClip(name);
		if (clip != null)
		{
			AS.enabled = true;
			TransitionClip = clip;
			TransitionRatio = -1.0f;
		}
			
	}

    // Start is called before the first frame update
    void Awake()
    {
		PV = GetComponent<PhotonView>();
		AS = GetComponent<AudioSource>();
    }

	private void AddTracks(List<AudioClip> clips)
	{
		foreach (AudioClip clip in ActiveTracks)
			if (!Tracks.ContainsKey(clip.name))
				Tracks.Add(clip.name, clip);
	}

	private void Start()
	{
		AddTracks(IntroTracks);
		AddTracks(ActiveTracks);
		AddTracks(ChillTracks);
		TransitionClip = GetRandomClip();
	}

	bool IsTransition
	{
		get
		{
			return (TransitionRatio < 1.0f) || (TransitionClip != CurrentClip);
		}
	}


	// Update is called once per frame
	void Update()
    {
		if (PV.IsMine && AS.enabled)
		{
			if ((!AS.isPlaying || (AS.time > CurrentClip.length - TransitionTime)) && !IsTransition)
			{
				CmdPlayNext();
			}
		}

		if (AS.enabled && IsTransition)
		{
			TransitionRatio = Mathf.Clamp(TransitionRatio + Time.deltaTime / TransitionTime, -1.0f, 1.0f);
			AS.volume = Mathf.Abs(TransitionRatio);
			if (TransitionClip != CurrentClip && TransitionRatio > 0.0f)
			{
				CurrentClip = TransitionClip;
				if (TransitionClip != null)
				{
					AS.clip = TransitionClip;
					AS.Play();
				}
			}
		}
    }
}
