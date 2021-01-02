using DuloGames.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameReward : MonoBehaviour
{
	public int RequiredLevel = 0;
	public GameFeatures.Feature RequiredFeature = GameFeatures.Feature.Default;
	public RawImage Icon;
	public Image IconSprite;

	private void Start()
	{
		if (Icon == null)
			Icon = GetComponent<RawImage>();
	}

	public bool IsUnlocked
	{
		get
		{
			if (FrisbeeGame.Instance == null)
				return false;

			GameObject player = FrisbeeGame.Instance.MainPlayer;
			if (player != null)
			{
				if (RequiredLevel >= 0 && player.GetComponent<DBStats>().Level >= RequiredLevel)
					return true;

				if (GameFeatures.HasFeature(RequiredFeature))
					return true;
			}
			return false;
		}
	}

	public void RequestUnlock()
	{
		UIWindow window = UIWindow.GetWindow(UIWindowID.CharacterMenu);
		window.Show();

		UITab tab = window.gameObject.transform.Find("Content/Tabs Menu/Tab 2").GetComponent<UITab>();
		tab.isOn = true;

		//GameFeatures.RequestFeature(UnlockFeature);
	}
}
