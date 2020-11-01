using DuloGames.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class PinCodeUI : MonoBehaviour
{
	string RoomName { get; set; }
	string Password { get; set; }

	public InputField PasswordInput;

	public void Show(string roomName, string password)
	{
		RoomName = roomName;
		Password = password;
		GetComponent<UIWindow>().Show();
	}

	public void Login_OnClick()
	{
		string password = PasswordInput.text;

		if (password == Password)
		{
			RoomMenu.Instance.OnJoinRoom(RoomName);
			Close();
		}
	}

	public void Close_OnClick()
	{
		Close();
	}

	void Close()
	{
		GetComponent<UIWindow>().Hide();
	}
}
