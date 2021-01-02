using DuloGames.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomizationListItem : MonoBehaviour
{
	public UIWindow ParentWindow;
	public Text NameLabel;
	public GameObject SelectedSlot;

	public CharacterCustomization.Style Style;

	string itemName;
	public string ItemName
	{
		get { return itemName; }
		set
		{
			itemName = value;
			NameLabel.text = value;
		}
	}

	bool isSelected = false;
	public bool IsSelected
	{
		get { return isSelected; }
		set
		{
			isSelected = value;
			SelectedSlot.SetActive(value);
		}
	}

	public void OnClick()
	{
		PlayerCustomization playerCustomization = CharacterStyleMenuUI.Instance.MainPlayerCustomization;

		if (playerCustomization != null)
		{
			switch (Style)
			{
				case CharacterCustomization.Style.Accessory:
					playerCustomization.AccessoryStyle = ItemName;
					break;

				case CharacterCustomization.Style.Head:
					playerCustomization.HeadStyle = ItemName;
					break;

				case CharacterCustomization.Style.Body:
					int teamIndex = playerCustomization.gameObject.GetComponent<AimController>().Team;
					FrisbeeGame.Instance.Teams[teamIndex].TeamOutfit = ItemName;
					break;
			}
		}
	}

	public void Update()
	{
		if (ParentWindow.IsOpen)
		{
			PlayerCustomization playerCustomization = CharacterStyleMenuUI.Instance.MainPlayerCustomization;
			if (playerCustomization != null)
			{
				IsSelected = playerCustomization.CurrentCharacter.GetStyle(Style) == ItemName;
			}
		}
	}
}
