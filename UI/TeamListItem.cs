using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamListItem : MonoBehaviour
{
	public GameObject Player;
	public GameObject MasterPanel;
	public Text Name;
	public Image SpecializationIcon;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		MasterPanel.SetActive(PhotonNetwork.IsMasterClient && Player != null && Player.GetComponent<BotController>() == null && !Player.GetComponent<PhotonView>().IsMine);

		RPGStats.Specialization specialization = Player.GetComponent<RPGStats>().CurrentSpecialization;
		Name.text = Player.GetComponent<PlayerTag>().Name;
		SpecializationIcon.sprite = TeamMenuUI.Instance.SpecializationIcons[(int)specialization];
	}

	public void Kick()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			if (Player != null)
			{
				foreach (Player player in PhotonNetwork.PlayerList)
				{
					if (player.TagObject.Equals(Player))
					{
						FrisbeeGame.Instance.KickPlayer(player);
					}
				}
			}
		}
	}
}
