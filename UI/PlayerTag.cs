using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Crosstales.BWF;

public class PlayerTag : MonoBehaviour
{
	public string PlayerName = "Player";

	public GameObject TagUI;

	private PhotonView PV;

	void SetNameToUI(string value)
	{
		TextMesh mesh = TagUI.GetComponent<TextMesh>();
		if (mesh != null)
			mesh.text = value;
	}

	void Awake()
	{
		PV = GetComponent<PhotonView>();
	}

	// Start is called before the first frame update
	void Start()
    {
		Name = PlayerName;
	}

	[PunRPC]
	private void RPC_SetName(string name)
	{
		PlayerName = Utils.ReplaceBadWords(name);
		SetNameToUI(PlayerName);
	}

	public string Name
	{
		get
		{
			return PlayerName;
		}
		set
		{
			PlayerName = Utils.ReplaceBadWords(value);
			SetNameToUI(PlayerName);

			if (PV.IsMine)
				PV.RPC("RPC_SetName", RpcTarget.OthersBuffered, value);
			else
				Debug.LogError("Can't set name for the remote object!");
		}
	}
}
