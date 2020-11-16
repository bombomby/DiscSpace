using DuloGames.UI;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class PinCodeUI : MonoBehaviour
{
	string RoomName { get; set; }
	int ExpectedKeyCode { get; set; }

	public InputField PasswordInput;

	public static PinCodeUI Instance;

	public void Awake()
	{
		Instance = this;
	}

	public string Pin
	{
		get
		{
			return PasswordInput.text;
		}
	}

	public int KeyCode
	{
		get
		{
			return Utils.GenerateKeyCode(Pin);
		}
	}

	public void Show(string roomName, int expectedKeyCode)
	{
		RoomName = roomName;
		ExpectedKeyCode = expectedKeyCode;
		GetComponent<UIWindow>().Show();
	}

	public void Login_OnClick()
	{
		if (KeyCode == ExpectedKeyCode)
		{
			if (!String.IsNullOrEmpty(Pin))
			{
				string signature = Utils.GenerateSignature(PhotonNetwork.LocalPlayer, RoomName, Pin);
				PhotonNetwork.NickName = String.Format("{0}[#{1}]", NetworkLobby.CurrentPlayerName, signature);
				NetworkLobby.Instance.ExpectedPin = Pin;
			}
			RoomMenu.Instance.OnJoinRoom(RoomName);
			Close();
		}
	}

	public void FocusInput()
	{
		PasswordInput.Select();
		PasswordInput.ActivateInputField();
	}

	public void OnActivateTransition()
	{
		FocusInput();
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
