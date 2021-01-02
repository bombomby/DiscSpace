using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_STANDALONE
using Steamworks;
#endif

public class GameFeatures : MonoBehaviour
{
	static GameFeatures Instance;

	void UpdateFeatures()
	{
		UnlockedFeatures.Clear();
#if UNITY_STANDALONE
		if (SteamManager.Initialized)
		{
			for (int i = 0; i < SteamApps.GetDLCCount(); ++i)
			{
				AppId_t AppID;
				bool Available;
				string Name;
				bool ret = SteamApps.BGetDLCDataByIndex(i, out AppID, out Available, out Name, 128);
				if (ret && SteamApps.BIsDlcInstalled(AppID))
					UnlockedFeatures.Add(AppID.m_AppId);
			}
		}
#endif
		UnlockedFeatures.Add((uint)Feature.Default);
	}

	public static string AccountName
	{
		get
		{
#if UNITY_STANDALONE
			if (SteamManager.Initialized)
			{
				return SteamFriends.GetPersonaName();
			}
#endif
			return string.Empty;
		}
	}

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		UpdateFeatures();
	}

	public enum Feature : uint
	{
		Default = 0,
		DiscSpacePro = 1494770,
	}

	HashSet<uint> UnlockedFeatures = new HashSet<uint>();
	public static void AddFeature(Feature feature)
	{
		Instance.UnlockedFeatures.Add((uint)feature);
	}

	public static bool HasFeature(Feature feature)
	{
		return Instance.UnlockedFeatures.Contains((uint)feature);
	}

	public static void RequestFeature(Feature feature)
	{
		if (!HasFeature(feature))
		{
#if UNITY_STANDALONE
			if (SteamUtils.IsOverlayEnabled())
			{
				AppId_t appID = new AppId_t((uint)feature);
				SteamFriends.ActivateGameOverlayToStore(appID, EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
			}
			else
			{
				Utils.OpenURL("https://store.steampowered.com/app/1452830/Disc_Space/");
			}
#endif
		}
	}

	public static bool IsPro
	{
		get
		{
			return HasFeature(Feature.DiscSpacePro);
		}
	}
}
