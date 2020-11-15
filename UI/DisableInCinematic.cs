using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableInCinematic : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
		gameObject.SetActive(!CameraController.IsCinematicModeEnabled);
    }
}
