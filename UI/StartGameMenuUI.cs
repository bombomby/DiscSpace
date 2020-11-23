using DuloGames.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartGameMenuUI : MonoBehaviour
{
	public Text TeamSizeLabel;
	public Text PointCapLabel;

	public Toggle AutoAddBots;
	public Toggle AutoRebalanceTeams;
	public Toggle EnableSpecializationChange;

	int pointCap = 11;
	int PointCap
	{
		get { return pointCap; }
		set
		{
			pointCap = Mathf.Clamp(value, 1, 25);
			PointCapLabel.text = pointCap.ToString();
		}
	}

	int teamSize = 3;
	int TeamSize
	{
		get { return teamSize; }
		set
		{
			teamSize = Mathf.Clamp(value, 3, 7);
			TeamSizeLabel.text = string.Format("{0}v{0}", teamSize);
		}
	}

	public void ChangePointCap(int count)
	{
		PointCap = PointCap + count;
	}

	public void ChangeTeamSize(int count)
	{
		TeamSize = TeamSize + count;
	}

	public void GameStart_Click()
	{
		FrisbeeGame.Instance.RequestNewGame(TeamSize, PointCap, AutoAddBots.isOn, AutoRebalanceTeams.isOn);
		Close();
	}

	public void Close()
	{
		GetComponent<UIWindow>().Hide();
	}
}
