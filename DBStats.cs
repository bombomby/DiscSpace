using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_STANDALONE
using Steamworks;
#endif

public class DBStats : MonoBehaviour
{
	PhotonView PV;

#if UNITY_STANDALONE
	private Callback<UserStatsReceived_t> RequestResultCb;
	private Callback<UserStatsStored_t> StoreResultCb;
#endif

	public enum Stat
	{
		Assist,
		Goal,
		Defence,
		Experience,
	}

	int[] Values = new int[Enum.GetValues(typeof(Stat)).Length];

	[PunRPC]
	void RPC_OnStatChanged(Stat stat, int value)
	{
		Values[(int)stat] = value;
	}

	public int this[Stat stat]
	{
		get { return Values[(int)stat]; }
		set {
			if (PV.IsMine)
			{
				PV.RPC("RPC_OnStatChanged", RpcTarget.AllBuffered, stat, value);
			}
			else
			{
				Debug.LogError("Can't set stat value!");
			}
		}
	}

	public bool Save()
	{
#if UNITY_STANDALONE
		if (!PV.IsMine || !SteamManager.Initialized || !SteamUser.BLoggedOn())
			return false;

		foreach (Stat stat in Enum.GetValues(typeof(Stat)))
			SteamUserStats.SetStat(stat.ToString(), this[stat]);

		return SteamUserStats.StoreStats();
#else
		return false;
#endif
	}

	private void Awake()
	{
		PV = GetComponent<PhotonView>();
	}

	private bool Refresh()
	{
#if UNITY_STANDALONE
		if (!PV.IsMine || !SteamManager.Initialized || !SteamUser.BLoggedOn())
			return false;

		return SteamUserStats.RequestCurrentStats();
#else
		return false;
#endif
	}

#if UNITY_STANDALONE
	private void OnStatsReceived(UserStatsReceived_t result)
	{
		if (result.m_eResult != EResult.k_EResultFail)
		{
			foreach (Stat stat in Enum.GetValues(typeof(Stat)))
			{
				int value = 0;
				SteamUserStats.GetStat(stat.ToString(), out value);
				this[stat] = value;
			}
			Debug.Log("Stats receive: SUCCESS!");
		}
		else
		{
			Debug.Log("Stats receive: FAIL!");
		}
	}

	private void OnStatsStored(UserStatsStored_t result)
	{
		if (result.m_eResult != EResult.k_EResultFail)
		{
			Debug.Log("Stats store: SUCCESS!");
		}
		else
		{
			Debug.Log("Stats store: FAIL!");
		}
	}
#endif

	const int ExpStep = 10;
	const int MaxLevel = 30;
	static List<int> LevelToExp = new List<int>();

	static int ConvertLevelToExp(int level)
	{
		return ExpStep * level * (level - 1) / 2;
	}

	static int ConvertExpToLevel(int exp)
	{
		int value = LevelToExp.BinarySearch(exp + 1);
		if (value < 0)
			value = ~value;
		return value;
	}

	public int Level => ConvertExpToLevel(this[Stat.Experience]);
	public int CurrLevelExperience {
		get { return ConvertLevelToExp(Level); }
	}
	public int NextLevelExperience
	{
		get { return ConvertLevelToExp(Level + 1); }
	}
	public int Experience => this[Stat.Experience];

	static DBStats()
	{
		for (int i = 1; i <= MaxLevel; ++i)
		{
			LevelToExp.Add(ConvertLevelToExp(i));
		}
	}

	// Start is called before the first frame update
	void Start()
    {
#if UNITY_STANDALONE
		if (PV.IsMine)
		{
			RequestResultCb = Callback<UserStatsReceived_t>.Create(OnStatsReceived);
			StoreResultCb = Callback<UserStatsStored_t>.Create(OnStatsStored);
		}
#endif
		Refresh();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void Reward(PlayerStats stats)
	{
		this[Stat.Assist] += stats.GetCount(PlayerStats.Stat.Assist);
		this[Stat.Goal] += stats.GetCount(PlayerStats.Stat.Goal);
		this[Stat.Defence] += stats.GetCount(PlayerStats.Stat.Defence);
		this[Stat.Experience] += stats.CalcExpReward();
		Save();
	}
}
