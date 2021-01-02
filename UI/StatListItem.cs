using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatListItem : MonoBehaviour
{
	public GameObject Player;

	public Text PlayerNameLabel;
	public List<Text> StatLabels;

    // Update is called once per frame
    void Update()
    {
		PlayerNameLabel.text = Player.GetComponent<PlayerTag>().Name;

		PlayerStats stats = Player.GetComponent<PlayerStats>();

		foreach (PlayerStats.Stat stat in Enum.GetValues(typeof(PlayerStats.Stat)))
		{
			int num = stats.GetCount(stat);
			Text slot = StatLabels[(int)stat];

			if (stat == PlayerStats.Stat.MoveDistance)
			{
				slot.text = (num / 1000.0f).ToString("F1");
			}
			else
			{
				slot.text = num.ToString();
			}
		}
    }
}
