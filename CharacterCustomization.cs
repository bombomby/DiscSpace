using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCustomization : MonoBehaviour
{
	[Serializable]
	public class DressItem
	{
		public string Name;
		public GameObject Model;
	}

	public List<DressItem> Outfit;
	public List<GameObject> Head;
	public List<GameObject> Glasses;

	public void SetOutfit(string name)
	{
		foreach (DressItem item in Outfit)
			item.Model.SetActive(item.Name == name || item.Model.name == name);
	}

	public void SetHead(string name)
	{
		foreach (GameObject head in Head)
			head.SetActive(head.name == name);
	}

	public void SetGlasses(string name)
	{
		foreach (GameObject glasses in Glasses)
			glasses.SetActive(glasses.name == name);
	}

	public void SetSkin(Material material)
	{
		foreach (DressItem dress in Outfit)
			dress.Model.GetComponent<SkinnedMeshRenderer>().material = material;

		foreach (GameObject head in Head)
			head.GetComponent<MeshRenderer>().material = material;
	}
}
