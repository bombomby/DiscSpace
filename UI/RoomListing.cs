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
	string Password { get; set; }

	public void UpdateInfo(RoomInfo info)
	{
		roomName = info.Name;

		bool hasChat = false;
		if (info.CustomProperties != null && info.CustomProperties.ContainsKey(RoomMenu.LinkPropertyName))
			hasChat = true;

		if (info.CustomProperties != null && info.CustomProperties.ContainsKey(RoomMenu.PasswordPropertyName))
			Password = info.CustomProperties[RoomMenu.PasswordPropertyName] as string;

		PlayerCount = info.PlayerCount;
		MaxPlayers = info.MaxPlayers;

		transform.Find("Server Name Text").GetComponent<Text>().text = info.Name;
		transform.Find("Server Count Text").GetComponent<Text>().text = string.Format("{0}/{1}", info.PlayerCount, info.MaxPlayers);
		Transform iconGroup = transform.Find("Icon Group");
		iconGroup.Find("Icon Chat").gameObject.SetActive(hasChat);
		iconGroup.Find("Icon Lock").gameObject.SetActive(!string.IsNullOrEmpty(Password));
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
				if (!string.IsNullOrEmpty(Password))
				{
					PinCodeUI pinCode = UIWindow.GetWindow(UIWindowID.PinPopup).GetComponent<PinCodeUI>();
					pinCode.Show(RoomName, Password);
				}
				else
				{
					RoomMenu.Instance.OnJoinRoom(RoomName);
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
