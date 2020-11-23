using ExitGames.Client.Photon;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RPGStats : MonoBehaviour
{
	PhotonView PV;

	public GameObject LowStaminaVFX;

	[PunRPC]
	void RPC_SelectSpecialization(int specialization)
	{
		currentSpecialization = (Specialization)specialization;
		SetModificator("Specialization", SpecializationModificators[specialization]);
	}

	public enum Specialization
	{
		Balanced,
		Handler,
		Cutter,
		Newbie,
		Cannon,
		DMachine,
	}

	Specialization currentSpecialization = Specialization.Balanced;
	public Specialization CurrentSpecialization
	{
		get
		{
			return currentSpecialization;
		}
		set
		{
			if (currentSpecialization != value && PV.IsMine)
			{
				PlayerPrefs.SetInt(SpecializationVar, (int)value);
				PlayerPrefs.Save();
			}

			currentSpecialization = value;
			PV.RPC("RPC_SelectSpecialization", RpcTarget.AllBuffered, (int)currentSpecialization);
		}
	}

	Action<RPGStats.Stats>[] SpecializationModificators = new Action<RPGStats.Stats>[]
	{
		// Balanced
		null,
		// Handler
		(RPGStats.Stats stats) =>
		{
			stats.DiscSpace *= 1.2f;
			stats.StaminaRecoverySpeed *= 0.9f;
			stats.MaxDiscCurve *= 1.15f;
		},
		// Cutter
		(RPGStats.Stats stats) =>
		{
			stats.DiscSpace *= 0.9f;
			stats.MaxDiscCurve *= 0.85f;
			stats.MoveSpeed *= 1.1f;
			stats.StaminaRecoverySpeed *= 1.1f;
		},
		// Newbie
		(RPGStats.Stats stats) =>
		{
			stats.DiscSpace *= 0.9f;
			stats.MaxDiscCurve *= 0.8f;
			stats.MoveSpeed *= 0.9f;
			stats.MaxStamina *= 0.8f;
			stats.StaminaRecoverySpeed *= 2.0f;
		},
		// Cannon
		(RPGStats.Stats stats) =>
		{
			stats.DiscSpace *= 1.3f;
			stats.MaxDiscCurve *= 1.5f;
			stats.StaminaRecoverySpeed *= 0.8f;
			stats.MaxStamina *= 0.8f;
			stats.MoveSpeed *= 0.9f;
		},
		// D-Machine
		(RPGStats.Stats stats) =>
		{
			stats.DiscSpace *= 0.8f;
			stats.MaxDiscCurve *= 0.5f;
			stats.MaxStamina *= 1.5f;
			stats.StaminaRecoverySpeed *= 1.5f;
		},
	};


	public class Stats
	{
		static float DEV_STAMINA_MULTIPLIER = 1.0f;

		public void Reset()
		{
			Health = 100.0f;
			MaxHealth = 100.0f;

			Stamina = 100.0f;
			MaxStamina = 100.0f;
			StaminaRecoverySpeed = 10.0f;

#if UNITY_EDITOR
			Stamina *= DEV_STAMINA_MULTIPLIER;
			MaxStamina *= DEV_STAMINA_MULTIPLIER;
#endif

			MoveSpeed = 5.0f;
			MoveSpeedBurstMultiplier = 1.4f;
			LayoutSpeedMultiplier = 1.4f;

			DiscSpace = 1.6f;
			MaxDiscCurve = 0.86f;
		}

		public float Health;
		public float MaxHealth;

		public float Stamina;
		public float MaxStamina;
		public float StaminaRecoverySpeed;

		public float MoveSpeed;
		public float MoveSpeedBurstMultiplier;
		public float LayoutSpeedMultiplier;

		public float DiscSpace;
		public float MaxDiscCurve;

		public float BurstMode = 0.0f;

		// Constants
		public const float StaminaBurstDrainSpeed = 20.0f;
		public const float HealthBurstDrainSpeed = 10.0f;
		public const float StaminaJumpDrain = 15.0f;
		public const float StaminaLayoutDrain = 5.0f;
		public const float DiscCatchRadiusLayoutScaler = 1.15f;

		public bool CanBurst { get { return Stamina > 0.0f ; } }
		public bool CanJump { get { return Stamina > StaminaJumpDrain; } }
		public bool CanLayout { get { return Stamina > StaminaLayoutDrain; } }

		public void Update(GameObject obj)
		{
			if (obj.GetComponent<AimController>().IsObserver)
				return;

			if (BurstMode > Mathf.Epsilon)
			{
				Stamina = Mathf.Max(0.0f, Stamina - BurstMode * StaminaBurstDrainSpeed * Time.deltaTime);
				BurstMode = 0.0f;
			}
			else
			{
				Stamina = Mathf.Min(MaxStamina, Stamina + StaminaRecoverySpeed * Time.deltaTime);
			}
		}

		public void Jump(GameObject obj)
		{
			if (obj.GetComponent<AimController>().IsObserver)
				return;

			Stamina = Mathf.Max(0.0f, Stamina - StaminaJumpDrain);
		}

		public void Layout(GameObject obj)
		{
			if (obj.GetComponent<AimController>().IsObserver)
				return;

			Stamina = Mathf.Max(0.0f, Stamina - StaminaLayoutDrain);
		}

		const short SerializationSize = 8 * 4;

		private static short SerializeStats(StreamBuffer outStream, object obj)
		{
			Stats stats = (Stats)obj;

			byte[] mem = new byte[SerializationSize];

			int index = 0;
			Protocol.Serialize(stats.Health, mem, ref index);
			Protocol.Serialize(stats.MaxHealth, mem, ref index);
			Protocol.Serialize(stats.Stamina, mem, ref index);
			Protocol.Serialize(stats.MaxStamina, mem, ref index);
			Protocol.Serialize(stats.StaminaRecoverySpeed, mem, ref index);

			Protocol.Serialize(stats.MoveSpeed, mem, ref index);

			Protocol.Serialize(stats.DiscSpace, mem, ref index);
			Protocol.Serialize(stats.MaxDiscCurve, mem, ref index);

			outStream.Write(mem, 0, mem.Length);

			return (short)mem.Length;
		}

		private static object DeserializeStats(StreamBuffer inStream, short length)
		{
			Stats stats = new Stats();
			byte[] mem = new byte[SerializationSize];

			inStream.Read(mem, 0, mem.Length);

			int index = 0;
			Protocol.Deserialize(out stats.Health, mem, ref index);
			Protocol.Deserialize(out stats.MaxHealth, mem, ref index);
			Protocol.Deserialize(out stats.Stamina, mem, ref index);
			Protocol.Deserialize(out stats.MaxStamina, mem, ref index);
			Protocol.Deserialize(out stats.StaminaRecoverySpeed, mem, ref index);

			Protocol.Deserialize(out stats.MoveSpeed, mem, ref index);

			Protocol.Deserialize(out stats.DiscSpace, mem, ref index);
			Protocol.Deserialize(out stats.MaxDiscCurve, mem, ref index);

			return stats;
		}


		public Stats()
		{
			Reset();
		}

		static Stats()
		{
			PhotonPeer.RegisterType(typeof(Stats), (byte)'W', SerializeStats, DeserializeStats);
		}
	}


	Dictionary<string, Action<Stats>> statsModificators = new Dictionary<string, Action<Stats>>();

	public void SetModificator(string name, Action<RPGStats.Stats> action)
	{
		if (!statsModificators.ContainsKey(name))
			statsModificators.Add(name, action);
		else
			statsModificators[name] = action;

		ApplyStats();
	}

	void ApplyStats()
	{
		CurrentStats.Reset();
		foreach (Action<RPGStats.Stats> modifier in statsModificators.Values)
		{
			if (modifier != null)
				modifier(CurrentStats);
		}

		GetComponent<AimController>().Spacer.Radius = CurrentStats.DiscSpace;
	}

	public Stats CurrentStats = new Stats();

	private float damagePerSec = 0.0f;
	public const float FireDamagePerSec = 20.0f;
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Fire")
		{
			damagePerSec += FireDamagePerSec;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.tag == "Fire")
		{
			damagePerSec -= FireDamagePerSec;
		}
	}

	[PunRPC]
	void RPC_UpdateStats(Stats stats)
	{
		CurrentStats = stats;
	}

	[PunRPC]
	void RPC_SetLowStamina(bool isLow)
	{
		isLowStamina = isLow;
		LowStaminaVFX.SetActive(isLow);
	}

	void UpdateStats()
	{
		PV.RPC("RPC_UpdateStats", RpcTarget.All, CurrentStats);
	}

	private void Awake()
	{
		PV = GetComponent<PhotonView>();
	}

	const string SpecializationVar = "Specialization";

	public const float HealthOutOfBoundsDrainSpeed = 50.0f;

	private void Start()
	{
		CurrentSpecialization = (Specialization)PlayerPrefs.GetInt(SpecializationVar, 0);
		ApplyStats();
	}

	bool isLowStamina = false;
	bool IsLowStamina
	{
		set
		{
			if (isLowStamina != value)
			{
				isLowStamina = value;
				PV.RPC("RPC_SetLowStamina", RpcTarget.All, value);
			}
		}
	}

	void Update()
	{
		CurrentStats.Update(gameObject);

		float damage = damagePerSec;

		if (transform.position.y < -2.0f)
		{
			damage += HealthOutOfBoundsDrainSpeed;
		}

		if (Mathf.Abs(damage) > Mathf.Epsilon)
		{
			CurrentStats.Health = Mathf.Max(0.0f, CurrentStats.Health - damage * Time.deltaTime);
		}

		if (CurrentStats.Health < Mathf.Epsilon && PV.IsMine && !GetComponent<PlayerController>().IsBot)
		{
			CurrentStats.Health = CurrentStats.MaxHealth;
			FrisbeeGame.Instance.RequestPlayerDeath();
		}

		if (PV.IsMine)
		{
			IsLowStamina = CurrentStats.Stamina < Stats.StaminaJumpDrain;
		}
	}
}
