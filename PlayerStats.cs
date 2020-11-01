using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
	PhotonView PV;

	private int score = 0;
	public int Score
	{
		get { return score; }
		set { score = value; PV.RPC("RPC_Score", RpcTarget.All, value); }
	}

	private int defence = 0;
	public int Defence
	{
		get { return defence; }
		set { defence = value; PV.RPC("RPC_Defence", RpcTarget.All, value); }
	}

	private int assist = 0;
	public int Assist
	{
		get { return assist; }
		set { assist = value; PV.RPC("RPC_Assist", RpcTarget.All, value); }
	}


	[PunRPC] void RPC_Score(int value) { score = value; }
	[PunRPC] void RPC_Assist(int value) { assist = value; }
	[PunRPC] void RPC_Defence(int value) { defence = value; }


	public void AddScore()
	{
		Score = Score + 1;
	}

	public void AddAssist()
	{
		Assist = Assist + 1;
	}

	public void AddDefence()
	{
		Defence = Defence + 1;
	}

	public void Reset()
	{
		score = 0;
		assist = 0;
		defence = 0;
	}

	private void OnGameStarted()
	{
		Reset();
	}

	private void Awake()
	{
		PV = GetComponent<PhotonView>();
	}

	public void Start()
	{
		FrisbeeGame.Instance.GameStarted += OnGameStarted;
	}

	private void OnDestroy()
	{
		FrisbeeGame.Instance.GameStarted -= OnGameStarted;
	}
}
