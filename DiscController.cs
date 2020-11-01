﻿using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DiscController : MonoBehaviour
{
	PhotonView PV;

	Rigidbody RB;

	public GameObject CurrentThrower;
	public GameObject CurrentPlayer;
	public GameObject CurrentTarget;

	int TeamLock = -1;

	Vector3 Velocity = Vector3.zero;

	struct ThrowParams
	{
		public Vector3 Origin;
		public Vector3 Direction;
		public Vector3 Normal;
		public float Amplitude;
		public float Speed;

		public const float CurveSpeedSmootStep = 0.5f;
		public const float DiscVelocity = 11.0f;

		const float FlatAngle = 10.0f;
		const float CurvedAngle = 20.0f;

		public void Calc(Vector3 from, Vector3 to, Vector3 direction)
		{
			Origin = from;

			Direction = direction;
			Direction.Normalize();

			Vector3 targetDirection = to - from;

			float distance = targetDirection.magnitude;
			targetDirection.Normalize();

			float angle = Vector3.Angle(Direction, targetDirection);

			// Force flat throw below certain threshold, smooth normal
			if (angle < FlatAngle)
			{
				Normal = Vector3.up;
			}
			else
			{
				float ratio = Mathf.Clamp((angle - FlatAngle) / (CurvedAngle - FlatAngle), 0.0f, 1.0f);

				Vector3 targetNormal = Vector3.Cross(Direction, targetDirection).normalized;
				if (targetNormal.y < 0.0f)
					targetNormal = -targetNormal;

				Normal = Vector3.Slerp(Vector3.up, targetNormal, ratio);
			}

			float linearSpeed = Vector3.Dot(Direction, targetDirection);

			//ThrowSpeed = linearSpeed * DiscVelocity;
			Speed = Mathf.Lerp(1.0f, linearSpeed, CurveSpeedSmootStep) * DiscVelocity;

			Amplitude = Mathf.Sqrt(1.0f - linearSpeed * linearSpeed) / linearSpeed;
		}

		public float CalcCurveValue(float ratio)
		{
			return -Amplitude * ratio * (ratio - 1.0f);
		}

		public Vector3 CalcPoint(Vector3 target, float time)
		{
			Vector3 targetDirection = Origin - target;

			float distance = targetDirection.magnitude;
			float totalTime = distance / Speed;

			float ratio = Mathf.Clamp(time / totalTime, 0.0f, 1.0f);

			float height = CalcCurveValue(ratio);

			targetDirection.Normalize();

			Vector3 curveDirection = Vector3.Cross(targetDirection, Normal);
			if (Vector3.Dot(curveDirection, Direction) < 0.0f)
				curveDirection = -curveDirection;

			return Vector3.Lerp(Origin, target, ratio) + curveDirection * distance * height;
		}
	}

	ThrowParams Throw;

	float CurrentFlyingTime;


	public float DiscHeight = 0.75f;

	enum DiscState
	{
		IDLE,
		FLYING,
		IN_HANDS,
	}

	DiscState State = DiscState.IDLE;


	// Start is called before the first frame update
	void Awake()
	{
		PV = GetComponent<PhotonView>();
		RB = GetComponent<Rigidbody>();
	}

	public void CmdThrow(GameObject player, GameObject target, Vector3 direction)
	{
		PV.RPC("RPC_Throw", RpcTarget.All, player.GetComponent<PhotonView>().ViewID, target.GetComponent<PhotonView>().ViewID, direction);
	}

	Vector3 GetAdjustedTargetPosition(Vector3 position)
	{
		return new Vector3(position.x, Mathf.Max(position.y, DiscHeight), position.z);
	}

	[PunRPC]
	private void RPC_Throw(int playerViewID, int targetViewID, Vector3 direction)
	{
		PhotonView playerView = PhotonView.Find(playerViewID);
		PhotonView targetView = PhotonView.Find(targetViewID);

		if (playerView == null || targetView == null)
			return;

		State = DiscState.FLYING;

		CurrentThrower = playerView.gameObject;
		CurrentPlayer = playerView.gameObject;
		CurrentTarget = targetView.gameObject;

		Throw.Calc(transform.position, GetAdjustedTargetPosition(CurrentTarget.transform.position), direction);

		CurrentFlyingTime = 0.0f;

		GetComponent<PhotonTransformView>().enabled = true;
		GetComponent<PhotonRigidbodyView>().enabled = true;

		transform.SetParent(null);

		transform.rotation = Quaternion.identity;

		//transform.position = new Vector3(transform.position.x, DiscHeight, transform.position.z);

		CurrentThrower.GetComponent<AimController>().OnThrow(CurrentTarget);

		//CurrentThrower.GetComponent<PlayerController>().SetAnimationTrigger("Throw");
	}

	public List<Vector3> CalcTrajectory(Vector3 from, Vector3 to, Vector3 dir, float ratioStart, float ratioFinish, int numSegments)
	{
		ThrowParams throwParams = new ThrowParams();
		throwParams.Calc(from, to, dir);

		float totalTime = (to - from).magnitude / throwParams.Speed;

		List<Vector3> points = new List<Vector3>();

		float step = (ratioFinish - ratioStart) / numSegments;
		for (float r = ratioStart; r <= ratioFinish; r += step)
		{
			points.Add(throwParams.CalcPoint(to, r * totalTime));
		}

		return points;
	}

	public void CmdDrop(GameObject player, bool restrictTeam)
	{
		// Locking dosc on the oppoenent team
		int teamLock = restrictTeam ? (player.GetComponent<AimController>().Team + 1) % 2 : -1;
		PV.RPC("RPC_Drop", RpcTarget.All, player.GetComponent<PhotonView>().ViewID, teamLock);
	}

	[PunRPC]
	public void RPC_Drop(int playerViewID, int teamLock)
	{
		PhotonView playerView = PhotonView.Find(playerViewID);

		if (playerView == null)
			return;

		State = DiscState.IDLE;
		TeamLock = teamLock;

		CurrentThrower = playerView.gameObject;
		CurrentPlayer = playerView.gameObject;
		CurrentTarget = null;

		LockDisc(false);

		//transform.rotation = Quaternion.identity;
		//transform.position = new Vector3(transform.position.x, DiscHeight, transform.position.z);

		CurrentPlayer.GetComponent<AimController>().OnThrow(null);
	}

	void LockDisc(bool isLocked)
	{
		GetComponent<PhotonTransformView>().enabled = !isLocked;
		GetComponent<PhotonRigidbodyView>().enabled = !isLocked;

		GetComponent<MeshCollider>().isTrigger = isLocked;
		Rigidbody rb = GetComponent<Rigidbody>();
		rb.isKinematic = isLocked;
		rb.freezeRotation = isLocked;

		if (!isLocked)
			transform.SetParent(null);
	}


	Transform FindHand(Transform transform)
	{
		return transform.Find("Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 R Clavicle/Bip001 R UpperArm/Bip001 R Forearm/Bip001 R Hand/R_hand_container");
	}

	public void CmdCatch(GameObject player)
	{
		int playerViewID = player.GetComponent<PhotonView>().ViewID;
		RPC_Catch(playerViewID);
		PV.RPC("RPC_Catch", RpcTarget.Others, player.GetComponent<PhotonView>().ViewID);
	}

	[PunRPC]
	public void RPC_Catch(int playerViewID)
	{
		PhotonView playerView = PhotonView.Find(playerViewID);
		if (playerView == null)
		{
			Debug.LogError("Can't find Player: " + playerViewID);
			return;
		}

		GameObject player = playerView.gameObject;

		State = DiscState.IN_HANDS;
		TeamLock = -1;
		CurrentPlayer = player;
		CurrentTarget = null;

		Transform hand = FindHand(player.GetComponent<PlayerController>().Player.transform);
		if (hand != null)
		{
			LockDisc(true);

			transform.SetParent(hand, true);

			Reset();

			player.GetComponent<AimController>().OnCatch(gameObject);
		}

	}

	const float DiscSmoothRatio = 0.3f;
	const float DiscSmoothDistance = 4.0f;

	// Update is called once per frame
	void FixedUpdate()
	{
		if (CurrentPlayer != null && CurrentTarget != null)
		{
			Vector3 target = GetAdjustedTargetPosition(CurrentTarget.transform.position);

			CurrentFlyingTime += Time.fixedDeltaTime;

			Vector3 discPos = Throw.CalcPoint(target, CurrentFlyingTime);

			float distance = (transform.position - target).magnitude;

			float smoothRatio = distance < DiscSmoothDistance ? Mathf.Lerp(DiscSmoothRatio, 1.0f, 1.0f - distance / DiscSmoothDistance) : DiscSmoothRatio;

			transform.position = Vector3.Lerp(transform.position, discPos, smoothRatio);
			transform.rotation = Quaternion.LookRotation(Vector3.forward, Throw.Normal);
		}

		float minY = GetComponent<MeshCollider>().bounds.min.y;
		if (minY < 0.0f && State == DiscState.IDLE)
		{
			transform.position = new Vector3(transform.position.x, transform.position.y - minY, transform.position.z);
		}
	}

	void Reset()
	{
		transform.localPosition = new Vector3(0, 0, 0.28f);
		transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));

		Velocity = Vector3.zero;
		//RB.velocity = Vector3.zero;
		//RB.angularVelocity = new Vector3(0f, 0f, 0f);
	}

	void OnTriggerEnter(Collider collider)
	{
		if (PhotonNetwork.IsMasterClient)
		{
			if (State != DiscState.IN_HANDS)
			{
				if (/*CurrentPlayer != collider.gameObject &&*/ collider.tag == "Catcher")
				{
					GameObject player = collider.transform.parent.gameObject;
					int team = player.GetComponent<AimController>().Team;

					if (TeamLock == -1 || TeamLock == team)
						CmdCatch(player);
				}
			}
		}
	}

	public bool CanPickup(GameObject player)
	{
		int team = player.GetComponent<AimController>().Team;
		return State == DiscState.IDLE && (TeamLock == -1 || TeamLock == team);
	}

	public bool CanIntercept(GameObject player)
	{
		int team = player.GetComponent<AimController>().Team;
		return State == DiscState.FLYING && TeamPossession != team;
	}

	public const float MinCatchRadius = 0.75f;
	public const float MaxCatchRadius = 1.75f;

	public float CalculateCatchRadius(Vector3 pos)
	{
		if (State == DiscState.FLYING)
		{
			float distA = (pos - CurrentPlayer.transform.position).magnitude;
			float distB = (pos - CurrentTarget.transform.position).magnitude;

			// Calculating catch radius as a ratio between distance to the thrower and sum of distances to start/finish
			// So players putting force deep on the side gets a slight distadvantage to the straight force
			return Mathf.Lerp(MinCatchRadius, MaxCatchRadius, distA / (distA + distB));
		}
		return MinCatchRadius;
	}

	public int TeamPossession
	{
		get
		{
			if (TeamLock != -1)
				return TeamLock;

			if (CurrentPlayer == null)
				return -1;

			return CurrentPlayer.GetComponent<AimController>().Team;
		}
	}
}