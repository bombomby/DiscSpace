using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRemover : MonoBehaviour
{
	public Bounds Area = new Bounds(Vector3.zero, Vector3.one * 500.0f);

	PhotonView PV;

    void Start()
    {
		PV = GetComponent<PhotonView>();
    }

    void Update()
    {
        if (PV.IsMine)
		{
			if (!Area.Contains(transform.position))
			{
				PhotonNetwork.Destroy(gameObject);
			}
		}
    }
}
