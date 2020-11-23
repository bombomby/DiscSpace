using DuloGames.UI;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JoinVideoChatMenuUI : MonoBehaviourPunCallbacks
{
	public Text LinkNameText;
	string Link = string.Empty;

	public override void OnJoinedRoom()
	{
		object link = null;
		if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(RoomMenu.LinkPropertyName, out link))
		{
			Link = link as string;
		}
		else
		{
			Link = "Invalid Link";
		}
		LinkNameText.text = Link;
	}

	void Close()
	{
		GetComponent<UIWindow>().Hide();
	}

	public void OnJoinButtonClick()
	{
		if (!string.IsNullOrEmpty(Link))
		{
			Utils.OpenURL(Link);
		}
		Close();
	}

	public void OnCloseButtonClick()
	{
		Close();
	}
}
