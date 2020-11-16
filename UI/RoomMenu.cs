using Photon.Pun;
using Photon.Realtime;
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

	public Toggle RoomPinToggle;
	public Text PlayerLimitText;

	private List<RoomListing> _roomListingButtons = new List<RoomListing>();
	public List<RoomListing> RoomList { get { return _roomListingButtons; } }

	private static RoomMenu instance;
	public static RoomMenu Instance { get { return instance; } 	}

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
			PlayerLimitText.text = playerLimit > 5 ? "No Limit" : string.Format("{0}v{0}", playerLimit);
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
			CreateRoom("Test Room", 6);
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
		if (name != string.Empty)
		{
			string link = ZoomLinkInputText.text;
			string password = RoomPinToggle.isOn ? Random.Range(1000, 9999).ToString() : null;
			byte maxPlayers = (byte)(playerLimit > 5 ? 16 : playerLimit * 2);
			CreateRoom(name, maxPlayers, link, password);
		}
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
