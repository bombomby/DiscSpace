using DuloGames.UI;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomListing : MonoBehaviour
{
	private string roomName;
	public string RoomName
	{
		get
		{
			return roomName;
		}
	}

	int PlayerCount { get; set; }
	int MaxPlayers { get; set; }
	int KeyCode { get; set; } = -1;
	bool HasKeyCode { get { return KeyCode != -1; } }

	public bool IsEmpty
	{
		get
		{
			return PlayerCount == 0;
		}
	}

	public void UpdateInfo(RoomInfo info)
	{
		roomName = info.Name;

		bool hasChat = false;
		if (info.CustomProperties != null && info.CustomProperties.ContainsKey(RoomMenu.LinkPropertyName))
			hasChat = true;

		if (info.CustomProperties != null && info.CustomProperties.ContainsKey(RoomMenu.KeyCodePropertyName))
			KeyCode = (int)info.CustomProperties[RoomMenu.KeyCodePropertyName];
		else
			KeyCode = -1;

		PlayerCount = info.PlayerCount;
		MaxPlayers = info.MaxPlayers;

		transform.Find("Server Name Text").GetComponent<Text>().text = Utils.ReplaceBadWords(info.Name);
		transform.Find("Server Count Text").GetComponent<Text>().text = string.Format("{0}/{1}", info.PlayerCount, info.MaxPlayers);
		Transform iconGroup = transform.Find("Icon Group");
		iconGroup.Find("Icon Chat").gameObject.SetActive(hasChat);
		iconGroup.Find("Icon Lock").gameObject.SetActive(HasKeyCode);
	}

	private Button joinButton;

	// Start is called before the first frame update
	void Start()
    {
		GameObject roomMenu = RoomMenu.Instance.gameObject;
		joinButton = transform.Find("Join Button").GetComponent<Button>();
		joinButton.onClick.AddListener(() =>
		{
			if (PlayerCount < MaxPlayers)
			{
				if (HasKeyCode)
				{
					PinCodeUI pinCode = UIWindow.GetWindow(UIWindowID.PinPopup).GetComponent<PinCodeUI>();
					pinCode.Show(RoomName, KeyCode);
				}
				else
				{
					RoomMenu.Instance.OnJoinRoom(RoomName);
					NetworkLobby.Instance.ExpectedPin = null;
				}
			}
		});
	}

	private void OnDestroy()
	{
		if (joinButton != null)
			joinButton.onClick.RemoveAllListeners();
	}
}
