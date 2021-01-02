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

	public string skinTone;
	public string SkinTone
	{
		get { return skinTone; }
		set
		{
			if (PV.IsSceneView)
				RPC_SetSkin(Gender, value);
			else
				PV.RPC("RPC_SetSkin", RpcTarget.AllBuffered, Gender, value);
		}
	}

	string[] headStyle = new string[2];
	public string HeadStyle
	{
		get { return headStyle[(int)Gender]; }
		set
		{
			if (PV.IsSceneView)
				RPC_SetHead(Gender, value);
			else
				PV.RPC("RPC_SetHead", RpcTarget.AllBuffered, Gender, value);
		}
	}

	string[] accessoryStyle = new string[2];
	public string AccessoryStyle
	{
		get { return accessoryStyle[(int)Gender]; }
		set
		{
			if (PV.IsSceneView)
				RPC_SetAccessory(Gender, value);
			else
				PV.RPC("RPC_SetAccessory", RpcTarget.AllBuffered, Gender, value);
		}
	}

	public CharacterCustomization CurrentCharacter
	{
		get { return Customization[(int)Gender]; }
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
				if (PV.IsSceneView)
					RPC_SetGender(value);
				else
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
		RPC_SetSkin(Gender, SkinTone);
		RPC_SetAccessory(Gender, AccessoryStyle);
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
		headStyle[(int)Gender] = name;
		Customization[(int)gender].SetStyle(CharacterCustomization.Style.Head, name);
	}

	[PunRPC]
	void RPC_SetAccessory(GenderType gender, string name)
	{
		accessoryStyle[(int)Gender] = name;
		Customization[(int)gender].SetStyle(CharacterCustomization.Style.Accessory, name);
	}

	[PunRPC]
	void RPC_SetSkin(GenderType gender, string name)
	{
		foreach (Material material in Skins)
		{
			if (material.name == name)
			{
				Customization[(int)gender].SetSkin(material);
				skinTone = name;
			}
		}
	}

	public void SetOutfit(string name)
	{
		foreach (CharacterCustomization cust in Customization)
		{
			cust.SetStyle(CharacterCustomization.Style.Body, name);
		}
	}

	const float SkinProbability = 0.8f;

	// Start is called before the first frame update
	void Start()
    {
        if (PV.IsMine || PV.IsSceneView)
		{
			if (PV.IsSceneView)
				GetComponent<AimController>().Team = UnityEngine.Random.Range(0, 2);

			Gender = (GenderType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(GenderType)).Length);

			Material skin = Skins[UnityEngine.Random.Range(0.0f, 1.0f) < SkinProbability ? 0 : 1];

			CharacterCustomization cust = Customization[(int)Gender];
			HeadStyle = cust.Head[UnityEngine.Random.Range(0, cust.Head.Count)].name;
			SkinTone = skin.name;
		}
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
