using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
	PhotonView PV;

	public int GetCount(Stat stat)
	{
		return (int)Mathf.Round(Values[(int)stat]);
	}

	public float GetValue(Stat stat)
	{
		return Values[(int)stat];
	}

	[PunRPC]
	void RPC_AddGlobal(Stat stat, float value) {
		AddLocal(stat, value);
	}

	public enum Stat
	{
		Assist,
		Goal,
		Defence,

		Throw,
		Turnover,
		Hammer,

		Layouts,
		Barrel,
		Emoji,

		MoveDistance,
	}

	List<float> Values;

	public void AddLocal(Stat stat, float value = 1.0f)
	{
		if (FrisbeeGame.IsInState(FrisbeeGame.GameState.Game))
		{
			int index = (int)stat;
			if (index < Values.Count)
				Values[index] = Values[index] + value;
		}
	}

	public void AddGlobal(Stat stat, float value = 1.0f)
	{
		if (FrisbeeGame.IsInState(FrisbeeGame.GameState.Game))
		{
			PV.RPC("RPC_AddGlobal", RpcTarget.All, stat, value);
		}
	}

	public void Reset()
	{
		for (int i = 0; i < Values.Count; ++i)
			Values[i] = 0.0f;
	}

	private void OnGameStarted()
	{
		Reset();
	}

	private void Awake()
	{
		PV = GetComponent<PhotonView>();

		int numValues = Enum.GetValues(typeof(Stat)).Length;
		Values = new List<float>(numValues);
		for (int i = 0; i < numValues; ++i)
		{
			Values.Add(0.0f);
		}
	}

	public void Start()
	{
		FrisbeeGame.Instance.GameStarted += OnGameStarted;
	}

	private void OnDestroy()
	{
		FrisbeeGame.Instance.GameStarted -= OnGameStarted;
	}

	public int CalcExpReward()
	{
		return GetCount(Stat.Assist) + GetCount(Stat.Goal) + GetCount(Stat.Defence);
	}
}
