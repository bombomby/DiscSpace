using DuloGames.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void OnCharacterButtonClick()
	{
		GetComponent<UIWindow>().Hide();
		UIWindow.GetWindow(UIWindowID.CharacterMenu).Show();
	}

	public void OnSettingsButtonClick()
	{
		GetComponent<UIWindow>().Hide();
		UIWindow.GetWindow(UIWindowID.Settings).Show();
	}

	public void OnCreditsButtonClick()
	{
		GetComponent<UIWindow>().Hide();
		UIWindow.GetWindow(UIWindowID.Credits).Show();
	}

	public void OnTutorialButtonClick()
	{
		GetComponent<UIWindow>().Hide();
		UIWindow.GetWindow(UIWindowID.Tutorial).Show();
	}

	public void OnCloseButtonClick()
	{
		GetComponent<UIWindow>().Hide();
	}

	public void OnDisconnectButtonClick()
	{
		GetComponent<UIWindow>().Hide();
		NetworkLobby.Instance.Disconnect();
		InteractionMenu.Instance.Clear();
		RoomMenu.Instance.Clean();
		NetworkLobby.Instance.Reconnect();
		FrisbeeGame.Instance.CmdGameOver();
		UIWindow.GetWindow(UIWindowID.SelectServer).Show();
	}

	public void OnExitButtonClick()
	{
		Application.Quit();
	}
}
