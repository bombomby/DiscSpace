using DuloGames.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmojiMenuItem : MonoBehaviour
{
	public GameObject EmojiPrefab;
	public GameObject LockObject;
	public GameReward Reward;

	UIWindow ParentWindow;

	public void OnClick()
	{
		GameObject player = FrisbeeGame.Instance.MainPlayer;
		if (player != null)
		{
			if (Reward.IsUnlocked)
			{
				player.GetComponent<EmojiController>().Play(EmojiPrefab);
			}
			else
			{
				Reward.RequestUnlock();
			}
			ParentWindow.Hide();
		}
	}

	void UpdateLock()
	{
		LockObject.SetActive(!Reward.IsUnlocked);
	}

	private void Awake()
	{
		Reward = GetComponent<GameReward>();
		ParentWindow = transform.parent.parent.GetComponent<UIWindow>();
	}

	// Start is called before the first frame update
	void Start()
    {
		UpdateLock();
	}

	private void Update()
	{
		if (ParentWindow.IsOpen)
		{
			UpdateLock();
		}
	}
}
