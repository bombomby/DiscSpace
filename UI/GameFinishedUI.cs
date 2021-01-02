using DuloGames.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameFinishedUI : MonoBehaviour
{
	public GameObject[] Stars;

	public Text Header;

	public Text[] TeamName;
	public Text[] TeamScore;

	public Text Score;
	public Text Assist;
	public Text Defence;
	public Text Experience;
	public Text GameTime;

	public void OnShow()
	{
		if (!GetComponent<UIWindow>().IsOpen)
		{
			FrisbeeGame game = FrisbeeGame.Instance;

			GameObject player = FrisbeeGame.Instance.MainPlayer;
			if (player != null)
			{
				int team = player.GetComponent<AimController>().Team;

				bool isVictory = (team >= FrisbeeGame.NumPlayingTeams) || (game.Teams[team].Score > game.Teams[(team + 1) % 2].Score);

				Header.text = isVictory ? "VICTORY" : "DEFEAT";
				foreach (GameObject star in Stars)
					star.SetActive(isVictory);

				for (int i = 0; i < FrisbeeGame.NumPlayingTeams; ++i)
				{
					TeamName[i].text = game.Teams[i].TeamName;
					TeamScore[i].text = game.Teams[i].Score.ToString();
				}

				PlayerStats stats = player.GetComponent<PlayerStats>();

				Score.text = string.Format("{0} Goal{1}", stats.GetCount(PlayerStats.Stat.Goal), stats.GetCount(PlayerStats.Stat.Goal) == 1 ? string.Empty : "s");
				Assist.text = string.Format("{0} Assist{1}", stats.GetCount(PlayerStats.Stat.Assist), stats.GetCount(PlayerStats.Stat.Assist) == 1 ? string.Empty : "s");
				Defence.text = string.Format("{0} D's", stats.GetCount(PlayerStats.Stat.Defence));
				Experience.text = string.Format("+{0} Exp", stats.CalcExpReward());

				TimeSpan gameDuration = DateTime.Now - game.GameStartedTimestamp;
				GameTime.text = gameDuration.ToString(@"mm\:ss");

				FrisbeeGame.Instance.SetAnnouncement(null, 0, isVictory ? AnnouncementUI.SoundFX.Victory : AnnouncementUI.SoundFX.Defeat);
			}
		}
	}

	public void Close()
	{
		GetComponent<UIWindow>().Hide();
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
