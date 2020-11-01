using DuloGames.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmojiMenuItem : MonoBehaviour
{
	public GameObject EmojiPrefab;

	public void OnClick()
	{
		GameObject player = FrisbeeGame.Instance.MainPlayer;
		if (player != null)
		{
			player.GetComponent<EmojiController>().Play(EmojiPrefab);
			transform.parent.parent.GetComponent<UIWindow>().Hide();
		}
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
