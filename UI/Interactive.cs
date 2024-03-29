﻿using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactive : MonoBehaviour
{
	public enum Type
	{
		ActionStart,
		ActionStop,
	}

	public Type Axis = Type.ActionStart;
	public UnityEvent Action;
	public String Text;
	public FrisbeeGame.GameState States = FrisbeeGame.GameState.Any;

	public bool MasterClientOnly = false;

	private bool IsTriggered = false;

	bool IsActivated
	{
		get
		{
			return Input.GetButtonDown(Axis.ToString()) && FrisbeeGame.IsInState(States);
		}
	}

	public void Cancel()
	{
		if (IsTriggered)
		{
			IsTriggered = false;
			InteractionMenu.Instance.RemoveInteraction(this);
		}
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

	bool IsPlayer(GameObject obj)
	{
		if (obj.tag == "Player")
		{
			PlayerController player = obj.GetComponent<PlayerController>();
			return player.PV.IsMine && !player.IsBot;
		}
		return false;
	}

	bool CanTrigger(GameObject obj)
	{
		return IsPlayer(obj) && (!MasterClientOnly || PhotonNetwork.IsMasterClient);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (CanTrigger(other.gameObject))
		{
			IsTriggered = true;
			InteractionMenu.Instance.AddInteraction(this);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (CanTrigger(other.gameObject))
		{
			InteractionMenu.Instance.RemoveInteraction(this);
			IsTriggered = false;
		}
	}

	// Update is called once per frame
	void Update()
    {
		if (IsTriggered && IsActivated)
		{
			Action.Invoke();
		}
    }

	private void OnDestroy()
	{
		if (IsTriggered)
		{
			InteractionMenu.Instance.RemoveInteraction(this);
			IsTriggered = false;
		}
	}
}
