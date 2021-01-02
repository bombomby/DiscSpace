using DuloGames.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterStyleMenuUI : MonoBehaviour
{
	public Text GenderLabel;
	public Text SkinToneLabel;

	public Transform TeamOutfitList;
	public Transform CharacterStyleList;
	public Transform AccessoryList;

	public GameObject ItemListPrefab;

	public PlayerCustomization MainPlayerCustomization
	{
		get
		{
			GameObject obj = FrisbeeGame.Instance != null ? FrisbeeGame.Instance.MainPlayer : null;
			return obj != null ? obj.GetComponent<PlayerCustomization>() : null;
		}
	}

	void Refresh()
	{
		PlayerCustomization playerCustomization = MainPlayerCustomization;
		if (playerCustomization != null)
		{
			GenderLabel.text = playerCustomization.Gender.ToString();
			SkinToneLabel.text = playerCustomization.SkinTone;

			if (CharacterStyleList.childCount == 0)
				UpdateList(CharacterStyleList, CharacterCustomization.Style.Head);

			if (AccessoryList.childCount == 0)
				UpdateList(AccessoryList, CharacterCustomization.Style.Accessory);

			if (TeamOutfitList.childCount == 0)
				UpdateList(TeamOutfitList, CharacterCustomization.Style.Body);
		}
	}

	public static CharacterStyleMenuUI Instance;

	private void Awake()
	{
		Instance = this;
	}

	// Start is called before the first frame update
	void Start()
    {
		UpdateAllLists();
	}

    // Update is called once per frame
    void Update()
    {
		if (UIWindow.GetWindow(UIWindowID.CharacterMenu).IsOpen)
			Refresh();
	}

	void UpdateAllLists()
	{
		UpdateList(CharacterStyleList, CharacterCustomization.Style.Head);
		UpdateList(AccessoryList, CharacterCustomization.Style.Accessory);
		UpdateList(TeamOutfitList, CharacterCustomization.Style.Body);
	}

	public void OnNextGender(int dir)
	{
		PlayerCustomization playerCustomization = MainPlayerCustomization;
		if (playerCustomization != null)
		{
			int numGenders = Enum.GetValues(typeof(PlayerCustomization.GenderType)).Length;
			PlayerCustomization.GenderType newGender = (PlayerCustomization.GenderType)((int)(playerCustomization.Gender + dir + numGenders) % numGenders);
			playerCustomization.Gender = newGender;
			UpdateAllLists();
		}
	}

	public void OnNextSkinTone(int dir)
	{
		PlayerCustomization playerCustomization = MainPlayerCustomization;
		if (playerCustomization != null)
		{
			for (int i = 0; i < playerCustomization.Skins.Length; ++i)
			{
				if (playerCustomization.Skins[i].name == playerCustomization.SkinTone)
				{
					int numSkins = playerCustomization.Skins.Length;
					int newIndex = (i + dir + numSkins) % numSkins;
					playerCustomization.SkinTone = playerCustomization.Skins[newIndex].name;
					return;
				}
			}
		}
	}

	public void OnRandomizeTeamStyle()
	{
		PlayerCustomization playerCustomization = MainPlayerCustomization;
		if (playerCustomization != null)
		{
			CharacterCustomization character = playerCustomization.CurrentCharacter;
			string outfit = character.Body[UnityEngine.Random.Range(0, character.Body.Count)].name;
			int teamIndex = playerCustomization.gameObject.GetComponent<AimController>().Team;
			FrisbeeGame.Instance.Teams[teamIndex].TeamOutfit = outfit;
		}
	}

	public void OnRandomizeCharacterStyle()
	{
		PlayerCustomization playerCustomization = MainPlayerCustomization;
		if (playerCustomization != null)
		{
			CharacterCustomization character = playerCustomization.CurrentCharacter;
			playerCustomization.HeadStyle = character.Head[UnityEngine.Random.Range(0, character.Head.Count)].name;
			playerCustomization.AccessoryStyle = character.Accessory[UnityEngine.Random.Range(0, character.Accessory.Count)].name;
		}
	}

	void UpdateList(Transform container, CharacterCustomization.Style style)
	{
		foreach (Transform child in container)
			GameObject.Destroy(child.gameObject);

		PlayerCustomization playerCustomization = MainPlayerCustomization;
		if (playerCustomization != null)
		{
			List<GameObject> items = MainPlayerCustomization.CurrentCharacter.GetItems(style);
			foreach (GameObject item in items)
			{
				GameObject listItem = Instantiate(ItemListPrefab);
				CustomizationListItem customizationItem = listItem.GetComponent<CustomizationListItem>();
				customizationItem.ItemName = item.name;
				customizationItem.Style = style;
				customizationItem.ParentWindow = UIWindow.GetWindow(UIWindowID.CharacterMenu);
				listItem.transform.SetParent(container, false);
			}
		}
	}
}
