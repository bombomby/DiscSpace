using DuloGames.UI;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EmojiController : MonoBehaviour
{
	PhotonView PV;

	public Transform Slot;
	public List<GameObject> EmojiList;

    void Awake()
    {
		PV = GetComponent<PhotonView>();
    }

	public void Play(GameObject prefab)
	{
		if (EmojiList.Contains(prefab))
			PV.RPC("RPC_Play", RpcTarget.All, prefab.name);
	}

	[PunRPC]
	public void RPC_Play(string name)
	{
		GameObject prefab = EmojiList.Find(o => o.name == name);
		GameObject emoji = Instantiate(prefab, Slot.position, Quaternion.identity, Slot);
	}
}
