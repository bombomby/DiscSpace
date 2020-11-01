using DuloGames.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialMenuUI : MonoBehaviour
{
	public Toggle SkipTutorialToggle;

	public void TryShow()
	{
		if (!IsSkipTutorial)
			GetComponent<UIWindow>().Show();
	}

	public void Close()
	{
		PlayerPrefs.SetInt(SkipTutorialVar, SkipTutorialToggle.isOn ? 1 : 0);
		GetComponent<UIWindow>().Hide();
	}

	const string SkipTutorialVar = "SkipTutorial";

	public static bool IsSkipTutorial
	{
		get { return PlayerPrefs.GetInt(SkipTutorialVar, 0) == 1; }
	}

	public void Start()
	{
		SkipTutorialToggle.isOn = IsSkipTutorial;
	}

	public void OnNextPage()
	{

	}
}
