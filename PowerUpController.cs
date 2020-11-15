using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpController : MonoBehaviour
{
	public enum PowerUp
	{
		None,
		Warning,
		DiscUp1,
		DiscUp2
	}


	[Serializable]
	public class PowerUpItem
	{
		public PowerUp Name;
		public GameObject VFX;
	}

	public PowerUpItem[] Items;

	private PowerUp currentPowerUp = PowerUp.None;
	public PowerUp CurrentPowerUp
	{
		get
		{
			return currentPowerUp;
		}
		set
		{
			if (currentPowerUp != value)
			{
				foreach (PowerUpItem item in Items)
				{
					item.VFX.SetActive(item.Name == value);
				}
				currentPowerUp = value;
			}
		}
	}

	AimController AC;

	public void Start()
	{
		AC = GetComponent<AimController>();
	}

	PowerUp CalcPowerUp()
	{
		if (AC.HasDiscInHands)
		{
			if (AC.HasTrippleTeam)
				return PowerUp.DiscUp2;
			else if (AC.HasDoubleTeam)
				return PowerUp.DiscUp1;
		}
		else
		{
			foreach (GameObject opponent in AC.OpponentsInRange)
			{
				AimController oppoentAC = opponent.GetComponent<AimController>();
				if (oppoentAC.HasDiscInHands && oppoentAC.HasDoubleTeam)
					return PowerUp.Warning;
			}
		}

		return PowerUp.None;
	}

	public void Update()
	{
		CurrentPowerUp = CalcPowerUp();
	}
}
