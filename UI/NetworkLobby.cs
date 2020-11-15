using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.UI;
using DuloGames.UI;

public class NetworkLobby : MonoBehaviourPunCallbacks
{
	public static NetworkLobby Instance;

	public Text StatusLabel;

	private void Awake()
	{
		Instance = this;
	}

	public const string NetworkVersion = "0.9.3";
	public const string GameVersion = "0.9.5";
	

	// Start is called before the first frame update
	void Start()
    {
	}

	private string CurrentRegion;
	private string CurrentUserName;

	public void Login(string username, string region = null)
	{
		CurrentUserName = username;
		CurrentRegion = region;

		StatusLabel.enabled = true;
		StatusLabel.text = "Connecting to the server ...";

		if (string.IsNullOrEmpty(region))
		{
			PhotonNetwork.ConnectUsingSettings();
		}
		else
		{
			PhotonNetwork.NetworkingClient.AppId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime;
			PhotonNetwork.NetworkingClient.AppVersion = PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion;
			PhotonNetwork.ConnectToRegion(region);
		}

		PhotonNetwork.GameVersion = NetworkVersion;
		PhotonNetwork.NickName = username;
	}

	public void Disconnect()
	{
		PhotonNetwork.LeaveRoom(false);
		PhotonNetwork.Disconnect();
	}

	public void Reconnect()
	{
		Disconnect();
		Login(CurrentUserName, CurrentRegion);
	}

	public override void OnConnectedToMaster()
	{
		StatusLabel.text = string.Format("[{0}] Select room", PhotonNetwork.CloudRegion);
		PhotonNetwork.JoinLobby();
	}

	public override void OnJoinedLobby()
	{
		//PhotonNetwork.JoinRandomRoom();
		//StatusLabel.GetComponent<Text>().text = "Joining random room ...";
		//CreateRoom();

		UIWindow.GetWindow(UIWindowID.Login).Hide();
		UIWindow.GetWindow(UIWindowID.SelectServer).Show();
	}

	private void CreateRoom()
	{
		RoomOptions options = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = 10 };
		PhotonNetwork.CreateRoom("Room" + Random.Range(0, 1000).ToString(), options);
	}

	public override void OnJoinRandomFailed(short returnCode, string message)
	{
		Debug.Log("Failed to Join Random Room. Creating a a new one.");
		CreateRoom();
	}

	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		Debug.Log("Failed to Create a Room");
		CreateRoom();
	}

	public override void OnJoinedRoom()
	{
		if (PhotonNetwork.InLobby)
		{
			PhotonNetwork.LeaveLobby();
		}

		string pin = null;
		if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(RoomMenu.PasswordPropertyName))
		{
			pin = PhotonNetwork.CurrentRoom.CustomProperties[RoomMenu.PasswordPropertyName] as string;
		}
			
		StatusLabel.text = string.Format("Server: [{0}] {1} {2}", PhotonNetwork.NetworkingClient.CloudRegion, PhotonNetwork.CurrentRoom.Name, !string.IsNullOrEmpty(pin) ? string.Format(": Pin {0} ", pin) : string.Empty);

		UIWindow.GetWindow(UIWindowID.SelectServer).Hide();

//#if !UNITY_EDITOR
//		UIWindow.GetWindow(UIWindowID.CharacterMenu).Show();
//#endif

		if (!PhotonNetwork.IsMasterClient)
		{
			if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(RoomMenu.LinkPropertyName))
				UIWindow.GetWindow(UIWindowID.JoinChat).Show();
		}
	}

	// Update is called once per frame
	void Update()
    {
        
    }

	public override void OnDisconnected(DisconnectCause cause)
	{
		StatusLabel.text = string.Format("Disconnected: {0}", cause.ToString());
		UIWindow.GetWindow(UIWindowID.Login).Show();
		base.OnDisconnected(cause);
	}

	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		FrisbeeGame.Instance.SetAnnouncement("Ooops! Game Host has disconnected\nReturning to the lobby", 5);
		StatusLabel.text = string.Format("Disconnected: Game Host has left the game");
		Disconnect();
		Reconnect();
	}
}
