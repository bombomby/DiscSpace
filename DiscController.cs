using Photon.Pun;
using Photon.Realtime;
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

	public GameObject Trail;

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

		public void Calc(Vector3 from, Vector3 to, Vector3 direction, float networkDelay)
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

			float linearSpeed = Mathf.Clamp(Vector3.Dot(Direction, targetDirection), 0.001f, 0.999f);

			//ThrowSpeed = linearSpeed * DiscVelocity;
			Speed = Mathf.Lerp(1.0f, linearSpeed, CurveSpeedSmootStep) * DiscVelocity;

			// Correction for lat - increasing speed to compensate the lag
			if ((networkDelay > 0.001f) && (distance > 2.0f * networkDelay * Speed))
			{
				Speed *= distance / (distance - networkDelay * Speed);
			}

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
			if (Vector3.Dot(curveDirection, Direction) < -0.001f)
				curveDirection = -curveDirection;

			return Vector3.Lerp(Origin, target, ratio) + curveDirection * distance * height;
		}
	}

	ThrowParams Throw;

	float CurrentFlyingTime;


	const float DiscHeight = 0.75f;

	public enum DiscState
	{
		IDLE,
		FLYING,
		IN_HANDS_PENDING,
		IN_HANDS,
	}

	DiscState State = DiscState.IDLE;
	int StateChangedTimestamp = -1;
	public DiscState CurrentState { get { return State; } }
	void SetState(DiscState state, int serverTimestamp)
	{
		State = state;
		StateChangedTimestamp = serverTimestamp;
	}

	// Start is called before the first frame update
	void Awake()
	{
		PV = GetComponent<PhotonView>();
		RB = GetComponent<Rigidbody>();
	}

	public void CmdThrow(GameObject player, GameObject target, Vector3 direction, bool enableTrail)
	{
		PV.RPC("RPC_Throw", RpcTarget.All, player.GetComponent<PhotonView>().ViewID, target.GetComponent<PhotonView>().ViewID, direction, enableTrail);
	}

	Vector3 GetAdjustedTargetPosition(Vector3 position)
	{
		return new Vector3(position.x, Mathf.Max(position.y, DiscHeight), position.z);
	}

	[PunRPC]
	private void RPC_Throw(int playerViewID, int targetViewID, Vector3 direction, bool enableTrail, PhotonMessageInfo info)
	{
		PhotonView playerView = PhotonView.Find(playerViewID);
		PhotonView targetView = PhotonView.Find(targetViewID);

		if (playerView == null || targetView == null)
			return;

		SetState(DiscState.FLYING, info.SentServerTimestamp);

		CurrentThrower = playerView.gameObject;
		CurrentPlayer = playerView.gameObject;
		CurrentTarget = targetView.gameObject;

		Vector3 throwOrigin = GetAdjustedTargetPosition(CurrentPlayer.transform.position);
		Vector3 targetOrigin = GetAdjustedTargetPosition(CurrentTarget.transform.position);

		// Calc lag
		float networkDelay = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));

		Throw.Calc(throwOrigin, targetOrigin, direction, networkDelay);

		//Debug.DrawLine(throwOrigin, throwOrigin + direction * 3.0f, Color.blue, 3.0f);

		CurrentFlyingTime = 0.0f;

		GetComponent<PhotonTransformView>().enabled = true;
		GetComponent<PhotonRigidbodyView>().enabled = true;

		transform.SetParent(null);

		transform.rotation = Quaternion.identity;

		//transform.position = new Vector3(transform.position.x, DiscHeight, transform.position.z);

		CurrentThrower.GetComponent<AimController>().OnThrow(CurrentTarget);

		if (enableTrail)
			Trail.SetActive(true);

		//CurrentThrower.GetComponent<PlayerController>().SetAnimationTrigger("Throw");
	}

	public List<Vector3> CalcTrajectory(Vector3 from, Vector3 to, Vector3 dir, float ratioStart, float ratioFinish, int numSegments)
	{
		ThrowParams throwParams = new ThrowParams();
		throwParams.Calc(from, to, dir, 0.0f);

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
	public void RPC_Drop(int playerViewID, int teamLock, PhotonMessageInfo info)
	{
		PhotonView playerView = PhotonView.Find(playerViewID);

		if (playerView == null)
			return;

		SetState(DiscState.IDLE, info.SentServerTimestamp);
		TeamLock = teamLock;
		Trail.SetActive(false);

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

	public void CmdTryCatch(GameObject player)
	{
		PhotonView targetPV = player.GetComponent<PhotonView>();

		// Visual catch straight away, will be corrected by server if needed
		SetState(DiscState.IN_HANDS_PENDING, PhotonNetwork.ServerTimestamp);
		OnVisualCatch(player);

		// Send a bid for catch
		PV.RPC("RPC_TryCatch", RpcTarget.MasterClient, targetPV.ViewID);
	}

	public void CmdCatch(GameObject player)
	{
		PhotonView targetPV = player.GetComponent<PhotonView>();

		// Send a bid for catch
		PV.RPC("RPC_ConfirmCatch", RpcTarget.All, targetPV.ViewID);
	}

	public bool OnVisualCatch(GameObject player)
	{
		TeamLock = -1;
		CurrentPlayer = player;
		CurrentTarget = null;
		Trail.SetActive(false);

		Transform hand = FindHand(player.GetComponent<PlayerController>().Player.transform);
		if (hand != null)
		{
			LockDisc(true);

			transform.SetParent(hand, true);

			Reset();

			return true;
		}

		return false;
	}

	Dictionary<int, HashSet<Player>> CurrentBids = new Dictionary<int, HashSet<Player>>();
	double FirstBidTime = 0.0;

	const float MaxBidDelaySec = 0.5f;

	void TryResolveDiscPossesion()
	{
		if (CurrentBids.Count == 0)
			return;

		int maxBidID = -1;
		int maxBidCount = -1;
		int totalBidCount = 0;

		foreach (var pair in CurrentBids)
		{
			if (pair.Value.Count > maxBidCount)
			{
				maxBidCount = pair.Value.Count;
				maxBidID = pair.Key;
			}
			totalBidCount += pair.Value.Count;
		}

		if ((maxBidCount > PhotonNetwork.PlayerList.Length / 2) || 
			(totalBidCount == PhotonNetwork.PlayerList.Length) || 
			(Mathf.Abs((float)(FirstBidTime - PhotonNetwork.Time)) > MaxBidDelaySec))
		{
			PV.RPC("RPC_ConfirmCatch", RpcTarget.All, maxBidID);
			CurrentBids.Clear();
			FirstBidTime = 0.0;
		}
	}

	[PunRPC]
	public void RPC_TryCatch(int playerViewID, PhotonMessageInfo info)
	{
		if (CurrentState != DiscState.IN_HANDS)
		{
			if (CurrentBids.Count == -1)
				FirstBidTime = info.SentServerTime;

			HashSet<Player> players = null;
			if (!CurrentBids.TryGetValue(playerViewID, out players))
			{
				players = new HashSet<Player>();
				CurrentBids.Add(playerViewID, players);
			}
			players.Add(info.Sender);

			TryResolveDiscPossesion();
		}
	}

	[PunRPC]
	public void RPC_ConfirmCatch(int playerViewID, PhotonMessageInfo info)
	{
		PhotonView playerView = PhotonView.Find(playerViewID);
		if (playerView == null)
		{
			Debug.LogError("Can't find Player: " + playerViewID);
			return;
		}

		// Visual catch straight away, will be corrected by server if needed
		SetState(DiscState.IN_HANDS, info.SentServerTimestamp);
		OnVisualCatch(playerView.gameObject);

		playerView.gameObject.GetComponent<AimController>().OnCatch(gameObject);
	}

	const float DiscSmoothRatio = 0.3f;
	const float DiscSmoothDistance = 4.0f;

	private void Update()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			TryResolveDiscPossesion();
		}
	}

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

			Vector3 dir = new Vector3(Throw.Direction.x, 0.0f, Throw.Direction.z).normalized;
			Vector3 forw = Vector3.Cross(Vector3.Cross(dir, Throw.Normal), Throw.Normal);

			transform.rotation = Quaternion.LookRotation(forw, Throw.Normal);
		}

		float minY = GetComponent<MeshCollider>().bounds.min.y;
		if (minY < 0.0f)
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
		//if (PhotonNetwork.IsMasterClient)
		{
			if (CurrentState == DiscState.FLYING || CurrentState == DiscState.IDLE)
			{
				if (/*CurrentPlayer != collider.gameObject &&*/ collider.tag == "Catcher")
				{
					GameObject player = collider.transform.parent.gameObject;
					int team = player.GetComponent<AimController>().Team;

					if (TeamLock == -1 || TeamLock == team)
						CmdTryCatch(player);
				}
			}
		}
	}

	public bool CanPickup(GameObject player)
	{
		int team = player.GetComponent<AimController>().Team;
		return CurrentState == DiscState.IDLE && (TeamLock == -1 || TeamLock == team);
	}

	public bool CanIntercept(GameObject player)
	{
		int team = player.GetComponent<AimController>().Team;
		return CurrentState == DiscState.FLYING && TeamPossession != team;
	}

	public const float DoubleTeamCatchRadiusMultiplier = 0.5f;
	public const float MinCatchRadius = 0.75f;
	public const float MaxCatchRadius = 1.65f;

	public float CalculateCatchRadius(Vector3 pos)
	{
		if (CurrentState == DiscState.FLYING && CurrentPlayer != null && CurrentTarget != null)
		{
			float distA = (pos - CurrentPlayer.transform.position).magnitude;
			float distB = (pos - CurrentTarget.transform.position).magnitude;

			if (distA < AimController.StallOutRadius)
				return MinCatchRadius;

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

	void OnDestroy()
	{
		if (CurrentState == DiscState.IN_HANDS || CurrentState == DiscState.IN_HANDS_PENDING)
		{
			if (CurrentPlayer != null)
			{
				CurrentPlayer.GetComponent<AimController>().OnThrow(null); 
			}
		}
	}

	public void ForceUpdate(Player player)
	{
		if (CurrentState == DiscState.IN_HANDS)
		{
			PV.RPC("RPC_ConfirmCatch", player, CurrentPlayer.GetComponent<PhotonView>().ViewID);
		}
	}
}
