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

	private string Version = "0.9";

	// Start is called before the first frame update
	void Start()
    {
	}

	public void Login(string username, string region = null)
	{
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

		PhotonNetwork.GameVersion = Version;
		PhotonNetwork.NickName = username;
	}

	public void Disconnect()
	{
		PhotonNetwork.LeaveRoom(false);
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
		string pin = null;
		if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(RoomMenu.PasswordPropertyName))
		{
			pin = PhotonNetwork.CurrentRoom.CustomProperties[RoomMenu.PasswordPropertyName] as string;
		}
			
		StatusLabel.text = string.Format("Server: [{0}] {1} {2}(v{3})", PhotonNetwork.NetworkingClient.CloudRegion, PhotonNetwork.CurrentRoom.Name, !string.IsNullOrEmpty(pin) ? string.Format(": Pin {0} ", pin) : string.Empty, PhotonNetwork.GameVersion);

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
}
