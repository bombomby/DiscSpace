using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCustomization : MonoBehaviour
{
	public List<GameObject> Body;
	public List<GameObject> Head;
	public List<GameObject> Accessory;

	public enum Style
	{
		Body,
		Head,
		Accessory,
	}


	List<List<GameObject>> items;
	List<List<GameObject>> Items
	{
		get
		{
			if (items == null)
			{
				items = new List<List<GameObject>>() { Body, Head, Accessory };
			}
			return items;
		}
	}
	List<String> selectedStyle;
	List<String> SelectedStyle
	{
		get
		{
			if (selectedStyle == null)
			{
				selectedStyle = new List<string>() { Body[0].name, Head[0].name, Accessory[0].name };
			}
			return selectedStyle;
		}
	}

	public List<GameObject> GetItems(Style style)
	{
		return Items[(int)style];
	}

	private void Awake()
	{
		// VS: DON'T PUT ANYTHING HERE - initialization could happen before Awake()
	}

	public void SetStyle(Style style, string styleName)
	{
		SelectedStyle[(int)style] = styleName;
		Items[(int)style].ForEach(item => item.SetActive(item.name == styleName));
	}

	public string GetStyle(Style style)
	{
		return SelectedStyle[(int)style];
	}

	public void SetSkin(Material material)
	{
		Items[(int)Style.Body].ForEach(item => item.GetComponent<SkinnedMeshRenderer>().material = material);
		Items[(int)Style.Head].ForEach(item => item.GetComponent<MeshRenderer>().material = material);
	}
}
