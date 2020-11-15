using DuloGames.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_STANDALONE
using Steamworks;
#endif

public class LoginMenuUI : MonoBehaviour
{
	public bool DevAutoLogin;

	public InputField InputName;
	public Text RegionName;

    // Start is called before the first frame update
    void Start()
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

		CurrentRegion = 0;
	}

    // Update is called once per frame
    void Update()
    {
        if (InputName.isFocused && InputName.text != String.Empty && Input.GetButtonDown("Submit"))
		{
			OnLoginButtonClick();
		}
    }

	[Serializable]
	public class Region
	{
		public string Name;
		public string City;
		public string Token;
	}

	public List<Region> Regions;
	public int currentRegion = 0;

	public int CurrentRegion
	{
		get { return currentRegion; }
		set
		{
			currentRegion = (value + Regions.Count) % Regions.Count;
			RegionName.text = String.Format("Region: {0}", Regions[currentRegion].Name);
		}
	}


	public void NextRegion()
	{
		CurrentRegion = CurrentRegion + 1;
	}

	public void PrevRegion()
	{
		CurrentRegion = CurrentRegion - 1;
	}

	public void OnLoginButtonClick()
	{
		string name = InputName.text;
		if (name != String.Empty)
		{
			NetworkLobby.Instance.Login(name, Regions[CurrentRegion].Token);
		}
	}

	void OnGUI()
	{
		if (InputName.isFocused && InputName.text != string.Empty && Input.GetButtonDown("Submit"))
		{
			OnLoginButtonClick();
		}
	}

	public void OnDiscordLinkClicked()
	{
		Application.OpenURL("https://discord.gg/4b8fSEF9aT");
	}

	public void OnDontateLinkClicked()
	{
		Application.OpenURL("https://discspace.itch.io/play/donate");
	}
}