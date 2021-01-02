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

	private PowerUpResult currentPowerUp = new PowerUpResult(PowerUp.None);

	AimController AC;

	public void Start()
	{
		AC = GetComponent<AimController>();
	}

	public struct PowerUpResult
	{
		public PowerUp Name;
		public float Scale;

		public PowerUpResult(PowerUp name, float scale = 1.0f)
		{
			Name = name;
			Scale = scale;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is PowerUpResult))
				return false;

			PowerUpResult other = (PowerUpResult)obj;
			return Name == other.Name && Mathf.Approximately(Scale, other.Scale);
		}

		public static bool operator==(PowerUpResult p1, PowerUpResult p2)
		{
			return p1.Equals(p2);
		}

		public static bool operator!=(PowerUpResult p1, PowerUpResult p2)
		{
			return !p1.Equals(p2);
		}
	}


	PowerUpResult CalcPowerUp()
	{
		if (AC.HasDiscInHands)
		{
			float doubleTeam = AC.DoubleTeamValue;
			if (doubleTeam > Mathf.Epsilon)
			{
				return new PowerUpResult(doubleTeam > 0.5f ? PowerUp.DiscUp2 : PowerUp.DiscUp1);
			}
		}
		else
		{
			foreach (GameObject opponent in AC.OpponentsInRange)
			{
				AimController oppoentAC = opponent.GetComponent<AimController>();
				if (oppoentAC.HasDiscInHands)
				{
					float doubleTeamWarning = oppoentAC.DoubleTeamWarningValue;
					if (doubleTeamWarning > Mathf.Epsilon)
					{
						return new PowerUpResult(PowerUp.Warning, Mathf.Sqrt(doubleTeamWarning));
					}
				}
					
			}
		}

		return new PowerUpResult(PowerUp.None);
	}

	public void Update()
	{
		PowerUpResult nextPowerUp = CalcPowerUp();

		if (currentPowerUp != nextPowerUp)
		{
			foreach (PowerUpItem item in Items)
			{
				if (item != null && item.VFX != null)
				{
					if (item.Name == nextPowerUp.Name)
					{
						item.VFX.SetActive(true);
						item.VFX.transform.localScale = Vector3.one * nextPowerUp.Scale;
					}
					else
					{
						item.VFX.SetActive(false);
					}
				}
			}
			currentPowerUp = nextPowerUp;
		}
	}
}
