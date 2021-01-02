using DuloGames.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamMenuUI : MonoBehaviour
{
	public GameObject[] TeamItemListPrefab;
	public GameObject[] TeamListContent;
	public InputField[] TeamNames;
	public Text[] TeamOutfits;
	public string[] CachedTeamNames = new string[] { string.Empty, string.Empty };

	class PlayerItem
	{
		public int Team = -1;
		public GameObject ListItem;
	}

	UIWindow Window;

	Dictionary<GameObject, PlayerItem> playerMap = new Dictionary<GameObject, PlayerItem>();

	public static TeamMenuUI Instance;

	// Start is called before the first frame update
	void Awake()
    {
		Instance = this;
		Window = GetComponent<UIWindow>();
    }


	private void ChangeTeamOutfit(int teamIndex, int change)
	{
		Team team = FrisbeeGame.Instance.Teams[teamIndex];
		string outfit = team.TeamOutfit;
		int index = team.AvailableOutfits.FindIndex(o => o == outfit);
		if (index != -1)
		{
			index = (index + change + team.AvailableOutfits.Count) % team.AvailableOutfits.Count;
			team.TeamOutfit = team.AvailableOutfits[index];
		}
		else
		{
			team.TeamOutfit = team.AvailableOutfits[0];
		}
	}

	public void NextTeamOutfit(int teamIndex)
	{
		ChangeTeamOutfit(teamIndex, 1);
	}

	public void PrevTeamOutfit(int teamIndex)
	{
		ChangeTeamOutfit(teamIndex, -1);
	}

	GameObject CreateItem(GameObject player)
	{
		int team = player.GetComponent<AimController>().Team;

		if (team < FrisbeeGame.NumPlayingTeams)
		{
			GameObject item = Instantiate(TeamItemListPrefab[team]);
			item.GetComponent<TeamListItem>().Player = player;
			item.transform.SetParent(TeamListContent[team].transform, false);
			return item;
		}

		return null;
	}

	public List<Sprite> SpecializationIcons;

	// Update is called once per frame
	void Update()
    {
		if (!Window.IsVisible)
			return;

		List<GameObject> players = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));

		foreach (GameObject player in players)
		{
			PlayerItem item;
			if (!playerMap.TryGetValue(player, out item))
			{
				item = new PlayerItem();
				playerMap.Add(player, item);
			}

			int team = player.GetComponent<AimController>().Team;

			if (item.Team != team)
			{
				item.Team = team;
				Destroy(item.ListItem);
				item.ListItem = CreateItem(player);
			}
		}

		List<GameObject> toDelete = new List<GameObject>();
		foreach (GameObject player in playerMap.Keys)
		{
			if (!players.Contains(player))
				toDelete.Add(player);
		}

		foreach (GameObject player in toDelete)
		{
			Destroy(playerMap[player].ListItem);
			playerMap.Remove(player);
		}

		for (int i = 0; i < TeamNames.Length; ++i)
		{
			string name = FrisbeeGame.Instance.Teams[i].TeamName;
			if (CachedTeamNames[i] != name)
			{
				TeamNames[i].text = name;
				CachedTeamNames[i] = name;
			}
		}

		for (int i = 0; i < TeamOutfits.Length; ++i)
		{
			string name = FrisbeeGame.Instance.Teams[i].TeamOutfit;
			TeamOutfits[i].text = string.Format("Team Uniform: {0}", name);
		}
	}

	public void UpdateTeamName(int index)
	{
		string name = TeamNames[index].text;
		if (name != string.Empty && name != CachedTeamNames[index])
		{
			FrisbeeGame.Instance.Teams[index].TeamName = name;
		}
	}

	public void OnJoinTeam(int index)
	{
		FrisbeeGame.Instance.MainPlayer.GetComponent<AimController>().Team = index;
	}
}
