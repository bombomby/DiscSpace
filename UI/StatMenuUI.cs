using DuloGames.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatMenuUI : MonoBehaviour
{
	public Transform Container;
	public GameObject ItemPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if (UIWindow.GetWindow(UIWindowID.CharacterMenu).IsOpen)
		{
			Container.gameObject.SetActive(true);

			List<GameObject> players = FrisbeeGame.Instance.GetPlayers();

			HashSet<GameObject> existing = new HashSet<GameObject>();

			bool foundSeparator = false;

			foreach (Transform child in Container.transform)
			{
				if (child.name != "Separator")
				{
					StatListItem item = child.GetComponent<StatListItem>();
					int team = item.Player != null ? item.Player.GetComponent<AimController>().Team : -1;
					if (!players.Contains(item.Player) || 
						(team == 0 && foundSeparator) || 
						(team == 1 && !foundSeparator) || 
						(team >= FrisbeeGame.NumPlayingTeams))
					{
						GameObject.Destroy(child.gameObject);
					}
					else
					{
						existing.Add(item.Player);
					}
				}
				else
				{
					foundSeparator = true;
				}
			}

			foreach (GameObject player in players)
			{
				if (!existing.Contains(player))
				{
					int team = player.GetComponent<AimController>().Team;
					if (team < FrisbeeGame.NumPlayingTeams)
					{
						GameObject item = Instantiate(ItemPrefab);
						item.GetComponent<StatListItem>().Player = player;
						item.transform.SetParent(Container, false);
						if (team == 0)
							item.transform.SetAsFirstSibling();
						else
							item.transform.SetAsLastSibling();
					}
				}
			}
		}
		else
		{
			Container.gameObject.SetActive(false);
		}
    }
}
