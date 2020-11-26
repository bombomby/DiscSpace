using DuloGames.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using UnityEngine.Networking;

#if UNITY_STANDALONE
using Steamworks;
#endif

public class LoginMenuUI : MonoBehaviour
{
	public bool DevAutoLogin;

	public InputField InputName;

	public TMPro.TMP_Text AnnouncementText;

	WWW request;

	/*IEnumerator*/ void Start()
    {
#if UNITY_EDITOR
		if (DevAutoLogin)
		{
			NetworkLobby.Instance.Login("Player");
		}
#endif

#if UNITY_STANDALONE
		if (SteamManager.Initialized)
		{
			string name = SteamFriends.GetPersonaName();
			InputName.text = name;
			Debug.Log(name);
		}
#endif

		/// VS: UnityWebRequest breask WebGL version!!!
		//using (UnityWebRequest request = UnityWebRequest.Get(@"https://raw.githubusercontent.com/bombomby/discspace-public/master/announcements.txt"))
		//{
		//	yield return request.SendWebRequest();

		//	if (request.result == UnityWebRequest.Result.Success)
		//	{
		//		AnnouncementText.text = request.downloadHandler.text;
		//	}
		//	else
		//	{
		//		AnnouncementText.text = "Failed to retrieve tournament data :(";
		//	}
		//}
		request = new WWW(@"https://raw.githubusercontent.com/bombomby/discspace-public/master/announcements.txt");
	}

	bool isWaitingForRequest = true;

	// Update is called once per frame
	void Update()
    {
        if (InputName.isFocused && InputName.text != String.Empty && Input.GetButtonDown("Submit"))
		{
			OnLoginButtonClick();
		}

		if (isWaitingForRequest && request.isDone)
		{
			AnnouncementText.text = request.text;
			isWaitingForRequest = false;
		}
	}

	public void OnLoginButtonClick()
	{
		string name = InputName.text;
		if (name != String.Empty)
		{
			NetworkLobby.Instance.Login(name);
		}
	}

	void OnGUI()
	{
		if (InputName.isFocused && InputName.text != string.Empty && Input.GetButtonDown("Submit"))
		{
			OnLoginButtonClick();
		}
	}
}