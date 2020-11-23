using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spaceship : MonoBehaviour
{
	private PhotonView PV;
	private Animator ShipAnimator;
	public Interactive InteractiveObject;

	public static Spaceship Instance;

	// Start is called before the first frame update
	void Start()
    {
		ShipAnimator = GetComponent<Animator>();
		PV = GetComponent<PhotonView>();
	}

	private void Awake()
	{
		Instance = this;
	}

	bool IsFlying;
	Vector3 OriginalPosition;
	GameObject FlyingPlayer;

	public void OnFlyFinished()
	{
		if (!IsFlying)
			return;

		if (PV.IsMine && FlyingPlayer != null)
		{
			PV.RPC("RPC_FlyToMoonFinish", RpcTarget.All, FlyingPlayer.GetComponent<PhotonView>().ViewID);
		}
	}

	public void CmdEject()
	{
		if (!IsFlying)
			return;

		if (FlyingPlayer != null)
		{
			PV.RPC("RPC_FlyToMoonFinish", RpcTarget.All, FlyingPlayer.GetComponent<PhotonView>().ViewID);
		}
	}

	[PunRPC]
	void RPC_FlyToMoonStart(int playerViewID)
	{
		if (!IsFlying)
		{
			PhotonView playerView = PhotonView.Find(playerViewID);
			if (playerView != null)
			{
				GameObject player = playerView.gameObject;

				OriginalPosition = player.transform.position;
				FlyingPlayer = player;

				player.GetComponent<PlayerController>().Teleport(gameObject.transform.position);
				player.transform.SetParent(gameObject.transform, true);
				player.SetActive(false);
			}

			if (PV.IsMine)
			{
				ShipAnimator.SetTrigger("FlyToMoon");
			}

			IsFlying = true;
		}
	}

	[PunRPC]
	void RPC_FlyToMoonFinish(int playerViewID)
	{
		PhotonView playerView = PhotonView.Find(playerViewID);
		if (playerView != null)
		{
			GameObject player = playerView.gameObject;

			player.transform.SetParent(null);
			player.GetComponent<PlayerController>().Teleport(OriginalPosition);
			player.SetActive(true);
		}
		IsFlying = false;
		FlyingPlayer = null;
	}

	public void Fly()
	{
		GameObject player = FrisbeeGame.Instance.MainPlayer;
		if (player != null)
		{
			PV.RPC("RPC_FlyToMoonStart", RpcTarget.All, player.GetComponent<PhotonView>().ViewID);
			InteractiveObject.Cancel();
		}
	}
}
