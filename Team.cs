using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Team : MonoBehaviour
{
	public int Index;

	public List<string> AvailableOutfits;

	string teamOutfit;
	public string TeamOutfit
	{
		get { return teamOutfit; }
		set { teamOutfit = value; PV.RPC("RPC_RequestSetTeamOutfit", RpcTarget.MasterClient, value); }
	}

	[PunRPC]
	private void RPC_RequestSetTeamOutfit(string value)
	{
		PV.RPC("RPC_SetTeamOutfit", RpcTarget.All, value);
	}

	[PunRPC]
	private void RPC_SetTeamOutfit(string value)
	{
		teamOutfit = value;
		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
		foreach (GameObject player in players)
		{
			if (player.GetComponent<AimController>().Team == Index)
			{
				player.GetComponent<PlayerCustomization>().SetOutfit(value);
			}
		}
	}

	public string teamName = "<None>";
	public string TeamName
	{
		get { return teamName; }
		set { teamName = value; PV.RPC("RPC_RequestSetTeamName", RpcTarget.MasterClient, value); }
	}

	[PunRPC]
	private void RPC_RequestSetTeamName(string value)
	{
		PV.RPC("RPC_SetTeamName", RpcTarget.All, value);
	}

	[PunRPC]
	private void RPC_SetTeamName(string value)
	{
		teamName = Utils.ReplaceBadWords(value);
		foreach (GameObject nameLabel in ScoreboardNameLabel)
			nameLabel.GetComponent<Text>().text = teamName;
	}

	private DateTime scoreTimestamp = new DateTime();
	public DateTime ScoreTimestamp { get { return scoreTimestamp; } }

	private int score = 0;
	public int Score
	{
		get { return score; }
		set { score = value; scoreTimestamp = DateTime.Now; PV.RPC("RPC_SetScore", RpcTarget.All, value); }
	}


	[PunRPC]
	private void RPC_SetScore(int value)
	{
		score = value;
		foreach (GameObject scoreLabel in ScoreboardScoreLabel)
			scoreLabel.GetComponent<Text>().text = value.ToString("00");
	}

	public int MaxTeamSize { get; set; } = 3;

	public List<GameObject> ScoreboardNameLabel;
	public List<GameObject> ScoreboardScoreLabel;

	public List<GameObject> Players = new List<GameObject>();
	public List<GameObject> Bots = new List<GameObject>();

	public int PlayerCount
	{
		get { return Players.Count + Bots.Count; }
	}

	PhotonView PV;

	// Start is called before the first frame update
	void Start()
    {
		PV = GetComponent<PhotonView>();
		foreach (GameObject nameLabel in ScoreboardNameLabel)
			nameLabel.GetComponent<Text>().text = TeamName;
	}

    // Update is called once per frame
    void Update()
    {
		if (PhotonNetwork.IsMasterClient && string.IsNullOrEmpty(teamOutfit))
		{
			TeamOutfit = AvailableOutfits[UnityEngine.Random.Range(0, AvailableOutfits.Count)];
		}
	}

	public void ForceUpdate(Player player)
	{
		PV.RPC("RPC_SetTeamOutfit", player, TeamOutfit);
		PV.RPC("RPC_SetTeamName", player, TeamName);
		PV.RPC("RPC_SetScore", player, Score);
	}

	public void Reset()
	{
		Score = 0;
		Players.Clear();
		Bots.Clear();
	}
}
