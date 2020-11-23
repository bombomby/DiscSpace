using DuloGames.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterMenuUI : MonoBehaviour
{
	public Sprite SelectedSpecializationForeground;
	public Sprite NormalSpecializationForeground;
	public Transform SpecializationGroup;

	public void SelectSpecialization(int specialization)
	{
		RPGStats stats = FrisbeeGame.Instance.MainPlayer.GetComponent<RPGStats>();
		stats.CurrentSpecialization = (RPGStats.Specialization)specialization;
	}

	RPGStats.Stats DefaultStats = new RPGStats.Stats();

	public Text StaminaStatText;
	public Text SpeedStatText;
	public Text RecoveryStatText;
	public Text DiscSpaceStatText;
	public Text MaxDiscCurveStatText;

	Color BetterStatColor = Color.green;
	Color DefaultStatColor = new Color(0.827f, 0.675f, 0.494f, 1.0f);
	Color WorseStatColor = Color.red;

	public UIWindow Window;

	public List<UITab> Tabs;

	private void Awake()
	{
		Window = GetComponent<UIWindow>();
	}

	void UpdateStat(Text statText, float ratio)
	{
		statText.text = String.Format("{0}%", Mathf.RoundToInt(100.0f * ratio));
		statText.color = Mathf.Abs(ratio - 1.0f) < Mathf.Epsilon ? DefaultStatColor : (ratio > 1.0f ? BetterStatColor : WorseStatColor );
	}

	void SelectTab(float dir)
	{
		for (int i = 0; i < Tabs.Count; ++i)
		{
			UITab tab = Tabs[i];
			if (tab.isOn)
			{
				if (dir > Mathf.Epsilon && i + 1 < Tabs.Count)
				{
					tab.isOn = false;
					Tabs[i + 1].Activate();
					return;
				}

				if (dir < -Mathf.Epsilon && i - 1 >= 0)
				{
					tab.isOn = false;
					Tabs[i - 1].Activate();
					return;
				}
			}
		}
	}

	// Update is called once per frame
	void Update()
    {
		if (Window.IsOpen)
		{
			GameObject mainPlayer = FrisbeeGame.Instance != null ? FrisbeeGame.Instance.MainPlayer : null;
			if (mainPlayer != null)
			{
				RPGStats.Stats stats = mainPlayer.GetComponent<RPGStats>().CurrentStats;
				UpdateStat(StaminaStatText, stats.MaxStamina / DefaultStats.MaxStamina);
				UpdateStat(SpeedStatText, stats.MoveSpeed / DefaultStats.MoveSpeed);
				UpdateStat(RecoveryStatText, stats.StaminaRecoverySpeed / DefaultStats.StaminaRecoverySpeed);
				UpdateStat(DiscSpaceStatText, stats.DiscSpace / DefaultStats.DiscSpace);
				UpdateStat(MaxDiscCurveStatText, stats.MaxDiscCurve / DefaultStats.MaxDiscCurve);

				int specialization = (int)mainPlayer.GetComponent<RPGStats>().CurrentSpecialization;

				for (int i = 0; i < SpecializationGroup.childCount; ++i)
				{
					SpecializationGroup.GetChild(i).Find("Button").Find("Foreground").GetComponent<Image>().sprite = (i == specialization ? SelectedSpecializationForeground : NormalSpecializationForeground);
				}
			}

			GameObject player = FrisbeeGame.Instance.MainPlayer;
			if (player != null)
			{
				GameObject iconCamera = player.transform.Find("IconCamera").gameObject;
				iconCamera.SetActive(Window.IsOpen);
			}

			if (Window.IsFocused && Input.GetButtonDown("Select Tab"))
				SelectTab(Input.GetAxis("Select Tab"));
		}
	}

	public void OnCloseButtonClick()
	{
		GetComponent<UIWindow>().Hide();
	}
}
