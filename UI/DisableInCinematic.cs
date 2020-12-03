using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableInCinematic : MonoBehaviour
{
	private void Start()
	{
		CameraController.CinematicModeChanged += CameraController_CinematicModeChanged;
	}

	private void CameraController_CinematicModeChanged(bool isCinematic)
	{
		gameObject.SetActive(!isCinematic);
	}

	private void OnDestroy()
	{
		CameraController.CinematicModeChanged -= CameraController_CinematicModeChanged;
	}
}
