using DuloGames.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardListItem : MonoBehaviour
{
	public GameReward Reward;
	public RawImage Icon;
	public Text Requirements;

	UIWindow ParentWindow;

    // Start is called before the first frame update
    void Start()
    {
		if (Reward != null)
		{
			Requirements.text = Reward.RequiredLevel.ToString();

			if (Reward.Icon != null)
				Icon.texture = Reward.Icon.texture;

			if (Reward.IconSprite != null)
				Icon.texture = Reward.IconSprite.mainTexture;
		}
		ParentWindow = UIWindow.GetWindow(UIWindowID.CharacterMenu);
	}

    // Update is called once per frame
    void Update()
    {
		if (ParentWindow.IsOpen && Reward != null)
		{
			if (Reward.IsUnlocked)
			{
				Requirements.gameObject.SetActive(false);
				Icon.gameObject.SetActive(true);
			}
			else
			{
				Requirements.gameObject.SetActive(true);
				//Icon.gameObject.SetActive(false);
			}
		}
	}
}
