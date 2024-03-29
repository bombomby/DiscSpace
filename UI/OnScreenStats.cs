﻿using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class OnScreenStats : MonoBehaviour
{
	const float fpsMeasurePeriod = 0.5f;
	private int m_FpsAccumulator = 0;
	private float m_FpsNextPeriod = 0;
	private int m_CurrentFps;
	const string display = "v{0}[{1}] | Ping: {2}ms | FPS: {3}\n{4}";
	private Text m_Text;

	private void Start()
	{
		m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
		m_Text = GetComponent<Text>();
	}


	private void Update()
	{
		// measure average frames per second
		m_FpsAccumulator++;
		if (Time.realtimeSinceStartup > m_FpsNextPeriod)
		{
			m_CurrentFps = (int)(m_FpsAccumulator / fpsMeasurePeriod);
			m_FpsAccumulator = 0;
			m_FpsNextPeriod += fpsMeasurePeriod;

			string debugString = "";
#if UNITY_EDITOR
			debugString = string.Format("Interpolation: {0}", FrisbeeGame.Instance.NetworkInterpolation.ToString());
#endif

			m_Text.text = string.Format(display, NetworkLobby.GameVersion, NetworkLobby.NetworkVersion, PhotonNetwork.NetworkingClient.LoadBalancingPeer.RoundTripTime, m_CurrentFps, debugString);
		}
	}
}
