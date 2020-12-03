using Photon.Pun.UtilityScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugWindow : MonoBehaviour
{
	PhotonLagSimulationGui PhotonLagGUI;

    // Start is called before the first frame update
    void Awake()
    {
		PhotonLagGUI = GetComponent<PhotonLagSimulationGui>();

	}

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
		{
			PhotonLagGUI.Visible = !PhotonLagGUI.Visible;
		}
    }
}
