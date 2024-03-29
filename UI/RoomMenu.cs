﻿using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class RoomMenu : MonoBehaviourPunCallbacks
{
	public GameObject RoomListScrollList;

	public InputField RoomNameInputText;
	public InputField ZoomLinkInputText;

	public GameObject RoomListItemPrefab;

	public Text RegionName;

	public Toggle RoomPinToggle;
	public Text PlayerLimitText;

	private List<RoomListing> _roomListingButtons = new List<RoomListing>();
	public List<RoomListing> RoomList { get { return _roomListingButtons; } }

	private static RoomMenu instance;
	public static RoomMenu Instance { get { return instance; } 	}

	[Serializable]
	public class Region
	{
		public string Name;
		public string City;
		public string Token;
	}

	public List<Region> Regions;

	int currentRegion = -1;

	void UpdateRegionName()
	{
		RegionName.text = string.Format("Region: {0}", Regions[CurrentRegion].Name);
	}

	public int CurrentRegion
	{
		get { return currentRegion; }
		set
		{
			currentRegion = (value + Regions.Count) % Regions.Count;
			UpdateRegionName();

			NetworkLobby.Instance.Disconnect();
			Clean();
			NetworkLobby.Instance.Login(NetworkLobby.CurrentPlayerName, Regions[currentRegion].Token);
		}
	}

	public override void OnConnectedToMaster()
	{
		if (currentRegion == -1)
		{
			string region = PhotonNetwork.CloudRegion.Replace("/*", "");
			for (int i = 0; i < Regions.Count; ++i)
			{
				if (Regions[i].Token == region)
				{
					currentRegion = i;
					UpdateRegionName();
				}
			}
		}
	}

	public void NextRegion()
	{
		if (CurrentRegion != -1)
			CurrentRegion = CurrentRegion + 1;
	}

	public void PrevRegion()
	{
		if (CurrentRegion != -1)
			CurrentRegion = CurrentRegion - 1;
	}


	private void Awake()
	{
		if (instance == null)
			instance = this;
	}

	int playerLimit = 3;
	int PlayerLimit
	{
		get { return playerLimit; }
		set
		{
			playerLimit = Mathf.Clamp(value, 3, 7);
			PlayerLimitText.text = string.Format("{0}v{0}", playerLimit);
		}
	}

	public bool DevRoomAutoCreate = true;

	public void ChangetPlayerLimit(int change)
	{
		PlayerLimit = PlayerLimit + change;
	}

	// Update is called once per frame
	void Update()
    {
#if UNITY_EDITOR
		if (DevRoomAutoCreate && PhotonNetwork.InLobby)
		{
			CreateRoom("Test Room", 6, null, "0987");
			DevRoomAutoCreate = false;
		}
#endif
	}

	public const string LinkPropertyName = "Link";
	public const string KeyCodePropertyName = "KeyCode";

	public const string EmergencyHash = "";

	static Regex[] WhitelistedPatterns = new Regex[]{
		new Regex(@"https:\/\/[a-zA-Z0-9]+.zoom.us\/j\/[0-9]+\?pwd=[a-zA-Z0-9]+"),
		new Regex(@"https:\/\/discord.gg\/[a-zA-Z0-9]+")
	};

	bool IsLinkWhitelisted(string link)
	{
		foreach (Regex pattern in WhitelistedPatterns)
		{
			if (pattern.IsMatch(link))
				return true;
		}
		return false;
	}

	void CreateRoom(string name, byte maxPlayers, string link = null, string password = null)
	{
		RoomOptions options = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = maxPlayers };

		ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
		List<string> lobbyProperties = new List<string>();

		if (!string.IsNullOrEmpty(link) && IsLinkWhitelisted(link))
		{
			properties[LinkPropertyName] = link;
			lobbyProperties.Add(LinkPropertyName);
		}

		if (!string.IsNullOrEmpty(password))
		{
			NetworkLobby.Instance.ExpectedPin = password;
			int keyCode = Utils.GenerateKeyCode(password);
			properties[KeyCodePropertyName] = keyCode;
			lobbyProperties.Add(KeyCodePropertyName);
		}
		else
		{
			NetworkLobby.Instance.ExpectedPin = null;
		}

		if (properties.Count > 0)
		{
			options.CustomRoomProperties = properties;
			options.CustomRoomPropertiesForLobby = lobbyProperties.ToArray();
		}
			
		PhotonNetwork.CreateRoom(name, options);
	}

	public void CreateRoomButton_OnClick()
	{
		string name = RoomNameInputText.text;
		if (string.IsNullOrEmpty(name))
			name = "Room" + UnityEngine.Random.Range(0, 1000).ToString();

		string link = ZoomLinkInputText.text;
		string password = RoomPinToggle.isOn ? UnityEngine.Random.Range(1000, 9999).ToString() : null;
		byte maxPlayers = (byte)(playerLimit > 5 ? 16 : playerLimit * 2);
		if (RoomPinToggle.isOn)
			maxPlayers = 16;
		CreateRoom(name, maxPlayers, link, password);
	}

	public void OnJoinRoom(string name)
	{
		PhotonNetwork.JoinRoom(name);
	}

	private void OnUpdateRoom(RoomInfo roomInfo)
	{
		RoomListing roomItem = RoomList.Find(x => x.RoomName == roomInfo.Name);

		if (roomItem == null)
		{
			GameObject listItem = Instantiate(RoomListItemPrefab);
			listItem.transform.SetParent(RoomListScrollList.transform, false);
			roomItem = listItem.GetComponent<RoomListing>();
			RoomList.Add(roomItem);
		}

		roomItem.UpdateInfo(roomInfo);

		if (roomItem.IsEmpty)
		{
			RoomList.Remove(roomItem);
			Destroy(roomItem.gameObject);
		}
	}

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		roomList.ForEach(x => OnUpdateRoom(x));
	}

	public void Clean()
	{
		RoomList.ForEach(roomItem => Destroy(roomItem.gameObject));
		RoomList.Clear();
	}

	void OnGUI()
	{
		if ((RoomNameInputText.isFocused || ZoomLinkInputText.isFocused) && RoomNameInputText.text != string.Empty && Input.GetButtonDown("Submit"))
		{
			CreateRoomButton_OnClick();
		}
	}
}
