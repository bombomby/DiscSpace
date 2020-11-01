using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCustomization : MonoBehaviour
{
	PhotonView PV;

	public enum GenderType
	{
		Male,
		Female,
	}


	private GenderType gender = GenderType.Male;
	public GenderType Gender
	{
		get
		{
			return gender;
		}
		set
		{
			if (gender != value)
			{
				PV.RPC("RPC_SetGender", RpcTarget.AllBuffered, value);
			}
		}
	}

	private bool isGhost = false;
	public bool IsGhost
	{
		get
		{
			return isGhost;
		}
		set
		{
			isGhost = value;
			UpdateOutfit();
		}
	}

	void UpdateOutfit()
	{
		for (int i = 0; i < Characters.Length; ++i)
		{
			if (i == (int)gender && !isGhost)
			{
				Characters[i].SetActive(true);
				GetComponent<PlayerController>().InitPlayer(Characters[i].transform);
			}
			else
			{
				Characters[i].SetActive(false);
			}
		}
	}

	[PunRPC]
	void RPC_SetGender(GenderType value)
	{
		gender = value;
		UpdateOutfit();
	}

	public GameObject[] Characters;
	public CharacterCustomization[] Customization;
	public Material[] Skins;

	private void Awake()
	{
		PV = GetComponent<PhotonView>();
	}

	[PunRPC]
	void RPC_SetHead(GenderType gender, string name)
	{
		Customization[(int)gender].SetHead(name);
	}

	public void SetHead(GenderType gender, string name)
	{
		PV.RPC("RPC_SetHead", RpcTarget.AllBuffered, gender, name);
	}

	[PunRPC]
	void RPC_SetSkin(GenderType gender, string name)
	{
		foreach (Material material in Skins)
		{
			if (material.name == name)
				Customization[(int)gender].SetSkin(material);
		}
	}

	public void SetSkin(GenderType gender, string name)
	{
		PV.RPC("RPC_SetSkin", RpcTarget.AllBuffered, gender, name);
	}

	public void SetOutfit(string name)
	{
		foreach (CharacterCustomization cust in Customization)
		{
			cust.SetOutfit(name);
		}
	}

	const float SkinProbability = 0.8f;

	// Start is called before the first frame update
	void Start()
    {
        if (PV.IsMine)
		{
			//Gender = (GenderType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(GenderType)).Length);
			Gender = GenderType.Female;

			Material skin = Skins[UnityEngine.Random.Range(0.0f, 1.0f) < SkinProbability ? 0 : 1];

			for (int i = 0; i < Customization.Length; ++i)
			{
				CharacterCustomization cust = Customization[i];
				GenderType gender = (GenderType)i;

				string head = cust.Head[UnityEngine.Random.Range(0, cust.Head.Count)].name;
				SetHead(gender, head);
				SetSkin(gender, skin.name);
			}
		}
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
