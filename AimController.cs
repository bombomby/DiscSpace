using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

public class AimController : MonoBehaviour
{
	PhotonView PV;
	PlayerController PC;

	public GameObject DiscPrefab;
	public GameObject TargetMark;

	public GameObject Disc;
	public float DiscInHandsTime;
	public GameObject CurrentTarget;
	public float CurrentTargetTimestamp;

	public int TeamIndex = 0;

	public bool IsBot;

	public bool IsObserver
	{
		get { return Team == FrisbeeGame.ObserverTeamIndex; }
	}

	public const float StallOutSec = 6.0f;
	public const float StallOutRadius = 4.0f;

	public GameObject StallOutCounter;
	public GameObject StallOutBar;

	public DiscSpacer Spacer = new DiscSpacer();

	public class DiscSpacer
	{
		public bool IsActive;
		public float Radius;
	}

	public GameObject AimDecal;

	PlayerAudio Audio;
	RPGStats Stats;

	// Start is called before the first frame update
	void Awake()
    {
		IsBot = GetComponent<BotController>() != null;
		PV = GetComponent<PhotonView>();
		PC = GetComponent<PlayerController>();
		Audio = GetComponent<PlayerAudio>();
		Stats = GetComponent<RPGStats>();
	}

	void Start()
	{
		if (PV.IsMine)
		{
			PV.RPC("RPC_SetTeam", RpcTarget.AllBuffered, TeamIndex);
		}

		StallOutBar.GetComponent<ResourceBar>().MaxValue = StallOutSec;
	}

	public int Team
	{
		get
		{
			return TeamIndex;
		}
		set
		{
			if (TeamIndex != value)
			{
				TeamIndex = value;
				if (PV.IsSceneView)
					RPC_SetTeam(value);
				else
					PV.RPC("RPC_SetTeam", RpcTarget.AllBuffered, value);
			}
		}
	}

	[PunRPC]
	void RPC_SetTeam(int index)
	{
		TeamIndex = index;

		SetGhost(index == FrisbeeGame.ObserverTeamIndex);

		Team team = FrisbeeGame.Instance.Teams[index];
		GetComponent<PlayerCustomization>().SetOutfit(team.TeamOutfit);

		FrisbeeGame.Instance.OnTeamChanged(gameObject, index);
	}

	void SetGhost(bool isGhost)
	{
		Catcher.enabled = !isGhost;
		GetComponent<PlayerCustomization>().IsGhost = isGhost;
		GetComponent<PlayerTag>().TagUI.SetActive(!isGhost);
		gameObject.layer = LayerMask.NameToLayer(isGhost ? "Observer" : "Default");
		//gameObject.layer = isGhost ? 11 : 0;
	}

	public enum TargetDirection
	{
		Forward,
		Backward,
	}

	private class AngleSort : IComparer<GameObject>
	{
		public Vector3 Origin;
		public Vector3 Direction;

		int IComparer<GameObject>.Compare(GameObject objA, GameObject objB)
		{
			float angleA = CalcAngle(objA);
			float angleB = CalcAngle(objB);
			return angleA.CompareTo(angleB);
		}

		float CalcAngle(GameObject obj)
		{
			return Vector3.SignedAngle(Direction, obj.transform.position - Origin, Vector3.up);
		}
	}

	public class SectorSort : IComparer<GameObject>
	{
		public Vector3 Origin;
		public Vector3 Direction;

		public float SectorAngle = 7.5f;

		int IComparer<GameObject>.Compare(GameObject objA, GameObject objB)
		{
			int sectorA = (int)(Mathf.Abs(CalcAngle(objA)) / SectorAngle);
			int sectorB = (int)(Mathf.Abs(CalcAngle(objB)) / SectorAngle);

			if (sectorA != sectorB)
				return sectorA.CompareTo(sectorB);

			float distanceA = Vector3.Distance(objA.transform.position, Origin);
			float distanceB = Vector3.Distance(objB.transform.position, Origin);

			return distanceA.CompareTo(distanceB);
		}

		float CalcAngle(GameObject obj)
		{
			return Vector3.SignedAngle(Direction, obj.transform.position - Origin, Vector3.up);
		}

	}


	private List<GameObject> GetPlayers(int teamIndex)
	{
		List<GameObject> result = new List<GameObject>();
		foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
		{
			if (obj.GetComponent<AimController>().Team == teamIndex)
				result.Add(obj);
		}
		return result;
	}

	private List<GameObject> GetPotentialTargets(int teamIndex)
	{
		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

		List<GameObject> targets = new List<GameObject>();
		for (int i = 0; i < players.Length; ++i)
		{
			if (players[i] != gameObject && players[i].GetComponent<AimController>().TeamIndex == teamIndex)
			{
				targets.Add(players[i]);
			}
		}

		return targets;
	}

	public GameObject SelectClosestTarget(Vector3 forward)
	{
		List<GameObject> targets = GetPotentialTargets(TeamIndex);

		if (targets.Count == 0)
			return null;

		SectorSort sorter = new SectorSort() { Origin = transform.position, Direction = forward };
		targets.Sort(sorter);

		Debug.DrawRay(transform.position, forward, Color.yellow); 

		return targets[0];
	}


	public GameObject SelectNextTarget(GameObject target, TargetDirection direction = TargetDirection.Forward)
	{
		List<GameObject> targets = GetPotentialTargets(TeamIndex);

		if (targets.Count == 0)
			return null;

		// Return first elements if previous target was null
		if (target == null)
			return targets.Count > 0 ? targets[0] : null;

		AngleSort sorter = new AngleSort() { Origin = transform.position, Direction = target.transform.position - transform.position };
		targets.Sort(sorter);

		int index = targets.FindIndex(t => t == target);

		if (index != -1)
		{
			index = (index + (direction == TargetDirection.Forward ? 1 : targets.Count - 1)) % targets.Count;
			return targets[index];
		}
		else
		{
			return targets[0];
		}
	}

	const float OBSTRUCTION_CHECK_RADIUS = 5.0f;
	const float OBSTRUCTION_CHECK_ANGLE = 15.0f;

	bool IsObstructed(GameObject target)
	{
		List<GameObject> opponents = FrisbeeGame.Instance.GetPlayers((Team + 1) % 2);
		foreach (GameObject opponent in opponents)
		{
			if ((opponent.transform.position - transform.position).magnitude < OBSTRUCTION_CHECK_RADIUS)
			{
				float angle = Vector3.Angle(target.transform.position - transform.position, opponent.transform.position - transform.position);
				if (angle < OBSTRUCTION_CHECK_ANGLE)
					return true;
			}
		}
		return false;
	}

	public GameObject SelectBestTarget(Vector3 forward)
	{
		List<GameObject> targets = GetPotentialTargets(TeamIndex);

		if (targets.Count == 0)
			return null;

		List<GameObject> forwardTargets = targets.FindAll(obj => Vector3.Dot(obj.transform.position - transform.position, forward) > 0.0f);
		List<GameObject> availableTargets = new List<GameObject>();

		foreach (GameObject target in forwardTargets)
		{
			if (!IsObstructed(target))
				availableTargets.Add(target);
		}

		if (availableTargets.Count == 0)
		{
			foreach (GameObject target in targets)
				if (!IsObstructed(target))
					availableTargets.Add(target);
		}

		if (availableTargets.Count > 0)
		{
			return availableTargets[Random.Range(0, availableTargets.Count - 1)];
		}

		return targets[Random.Range(0, targets.Count - 1)];
	}

	public List<GameObject> FindOpponentInRange(float range)
	{
		List<GameObject> result = new List<GameObject>();
		List<GameObject> opponents = GetPlayers((Team + 1) % 2);
		foreach (GameObject obj in opponents)
		{
			if (Vector3.Distance(transform.position, obj.transform.position) < range)
				result.Add(obj);
		}
		return result;
	}

	public static Quaternion MinThrowingArc = Quaternion.Euler(5.0f, 0.0f, 0.0f);
	public static Quaternion MaxThrowingArc = Quaternion.Euler(25.0f, 55.0f, 0.0f);

	public static Quaternion MinHammerArc = Quaternion.Euler(50.0f, 30.0f, 0.0f);
	public static Quaternion MaxHammerArc = Quaternion.Euler(50.0f, 55.0f, 0.0f);

	public const int DoubleTeamLimit = 2;
	public const int TrippleTeamLimit = DoubleTeamLimit + 1;

	Vector3 CalculateThrowDirection(float chargeTime, Quaternion minArc, Quaternion maxArc)
	{
		return CalculateThrowDirectionFromRatio(chargeTime / DiscChargeTime, minArc, maxArc);
	}

	public Vector3 CalculateThrowDirectionFromRatio(float ratio)
	{
		return CalculateThrowDirectionFromRatio(ratio, CurrentMinArc, CurrentMaxArc);
	}

	public Vector3 CalculateThrowDirectionFromRatio(float ratio, Quaternion minArc, Quaternion maxArc)
	{
		float maxDiscCurveRatio = Stats.CurrentStats.MaxDiscCurve;
		ratio = Mathf.Clamp(ratio, -maxDiscCurveRatio, maxDiscCurveRatio);

		Vector3 dir = CurrentTarget.transform.position - gameObject.transform.position;
		dir.Normalize();

		Quaternion rotationToTarget = Quaternion.LookRotation(dir, Vector3.up);

		float lerp = Mathf.Abs(ratio);
		Quaternion throwDirection = Quaternion.Euler(-Mathf.Lerp(minArc.eulerAngles.x, maxArc.eulerAngles.x, lerp), Mathf.Sign(ratio) * Mathf.Lerp(minArc.eulerAngles.y, maxArc.eulerAngles.y, lerp), 0.0f);

		return rotationToTarget * throwDirection * Vector3.forward;
	}

	void CmdThrowDiscToCurrentTarget(float chargeTime, Quaternion minArc, Quaternion maxArc)
	{
		if (CurrentTarget != null)
		{
			if (Mathf.Abs(chargeTime) < DiscStraightThrowTime)
				chargeTime = 0.0f;
			Vector3 dir = CalculateThrowDirection(chargeTime, minArc, maxArc);
			CmdThrowDisc(Disc, gameObject, CurrentTarget, dir);
			DiscCharge = 0.0f;
		}
	}

	public float DiscCharge = 0.0f;
	const float DiscChargeTime = 0.2f;
	const float DiscStraightThrowTime = DiscChargeTime * 0.5f;
	


	float CalcCurrentDiscCharge()
	{
		return Input.GetAxis("Disc Charge") + Input.GetAxis("Disc Charge Trigger");
	}

	bool IsReleasingDisc()
	{ 
		return (Input.GetButtonUp("Disc Charge") && !Input.GetButton("Disc Charge")) || Input.GetButtonDown("Disc Release");
	}

	const float ArrowAngle = 15.0f;
	const float ArrowSize = 3.0f;
	const float ArrowRatio = 0.2f;

	void UpdateAim()
	{
		if (AimDecal.activeSelf && CurrentTarget != null)
		{
			Vector3 throwDir = CalculateThrowDirection(DiscCharge, MinThrowingArc, MaxThrowingArc);

			throwDir.y = 0.0f;
			throwDir.Normalize();

			AimDecal.transform.rotation = Quaternion.LookRotation(throwDir, Vector3.up);
		}
	}

	const float TARGET_CHANGE_DELAY_SEC = 0.3f;

	public bool HasDoubleTeam => (OpponentsInRange.Count >= DoubleTeamLimit);
	public bool HasTrippleTeam => (OpponentsInRange.Count >= TrippleTeamLimit);

	Quaternion CurrentMaxArc
	{
		get
		{
			return HasDoubleTeam ? MaxHammerArc : MaxThrowingArc;
		}
	}

	Quaternion CurrentMinArc
	{
		get
		{
			return HasDoubleTeam ? MinHammerArc : MinThrowingArc;
		}
	}


	// Update is called once per frame
	void Update()
    {
		if (PV.IsMine && !IsBot)
		{
#if UNITY_EDITOR
			if (Input.GetKeyDown(KeyCode.C) && !HasDiscInHands)
			{
				CmdCreateDisc(gameObject.transform.position);
			}
#endif

			if (HasDiscInHands)
			{
				GameObject target = SelectClosestTarget(transform.forward);

				if ((Time.time - CurrentTargetTimestamp) > TARGET_CHANGE_DELAY_SEC || (target == null))
					SetTarget(target);
			}
			else
			{
				SetTarget(null);
			}

			if (HasDiscInHands)
			{
				if (Input.GetKeyDown(KeyCode.X))
				{
					CmdDropDisc(Disc, true);
				}

				if (IsReleasingDisc())
				{
					CmdThrowDiscToCurrentTarget(DiscCharge, CurrentMinArc, CurrentMaxArc);
					//CmdThrowHammerToCurrentTarget(DiscCharge);
				}
				else
				{
					float currentDiscCharge = CalcCurrentDiscCharge();
					DiscCharge = Mathf.Clamp(DiscCharge + currentDiscCharge * Time.deltaTime, -DiscChargeTime, DiscChargeTime);

					UpdateAim();
				}
			}
		}

		if (HasDiscInHands)
		{
			if (OpponentsInRange.Count > 0)
			{
				DiscInHandsTime += Time.deltaTime;
			}
			else
			{
				DiscInHandsTime = 0.0f;
			}
			float ratio = 1.0f - DiscInHandsTime / StallOutSec;
			ResourceBar bar = StallOutBar.GetComponent<ResourceBar>();
			bar.CurValue = StallOutSec - DiscInHandsTime;
			StallOutCounter.SetActive(DiscInHandsTime > Mathf.Epsilon);
		}
		else
		{
			StallOutCounter.SetActive(false);
		}
		

		if (PV.IsMine && HasDiscInHands && DiscInHandsTime >= StallOutSec)
		{
			CmdDropDisc(Disc, true);
		}

		UpdateCatcherTransform();
	}

	private void UpdateCatcherTransform()
	{
		Transform trA = PC.Player.transform;
		Transform trB = PC.Player.Find("Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Neck/Bip001 Head");
		Vector3 pos = Vector3.Lerp(trA.position, trB.position, 0.5f);
		Quaternion rot = Quaternion.LookRotation(trA.forward, (trB.position - trA.position).normalized);
		Catcher.transform.SetPositionAndRotation(pos, rot);
	}

	public void SetTarget(GameObject target)
	{
		if (CurrentTarget != null && !IsBot)
		{
			CurrentTarget.GetComponent<AimController>().TargetMark.SetActive(false);
		}

		CurrentTarget = target;
		CurrentTargetTimestamp = Time.time;

		if (!IsBot)
			AimDecal.SetActive(target != null);

		if (CurrentTarget != null && !IsBot)
		{
			CurrentTarget.GetComponent<AimController>().TargetMark.SetActive(true);
		}
	}

	public void OnCatch(GameObject disc)
	{
		if (HasDiscInHands)
		{
			CmdDropDisc(Disc, true);
		}

		Disc = disc;
		DiscInHandsTime = 0.0f;

		if (PV.IsMine && !IsBot)
		{
			GameObject target = SelectNextTarget(null, TargetDirection.Forward);
			SetTarget(target);
		}

		if (PhotonNetwork.IsMasterClient && FrisbeeGame.Instance.CanScore)
		{
			bool announceTransition = FrisbeeGame.IsInState(FrisbeeGame.GameState.Game_Playing);

			if (GetComponent<PlayerController>().IsGrounded)
			{
				BoxCollider zone = FrisbeeGame.Instance.GetGoalZone(TeamIndex);
				if (zone.bounds.Contains(transform.position))
				{
					FrisbeeGame.Instance.Score(disc.GetComponent<DiscController>().CurrentThrower, gameObject, TeamIndex);
					PC.CmdScore();
					announceTransition = false;
				}
			}

			GameObject thrower = disc.GetComponent<DiscController>().CurrentThrower;

			if (thrower.GetComponent<AimController>().Team != Team)
			{
				GetComponent<PlayerStats>().AddDefence();

				if (announceTransition)
					FrisbeeGame.Instance.CmdSwitchPossession(Team);
			}
		}

		Spacer.IsActive = true;
	}

	public void OnThrow(GameObject target)
	{
		Disc = null;
		Spacer.IsActive = false;
		Audio.OnAudioEvent(PlayerAudio.EventType.Throw);
	}

	public void CmdCreateDisc(Vector3 position)
	{
		PV.RPC("CMD_CreateDisc", RpcTarget.MasterClient, position);
	}

	const int MaxDiscCount = 10;

	[PunRPC]
	public void CMD_CreateDisc(Vector3 position)
	{
		GameObject[] discs = GameObject.FindGameObjectsWithTag("Disc");
		if (discs.Length >= MaxDiscCount)
			PhotonNetwork.Destroy(discs[0]);

		PhotonNetwork.InstantiateSceneObject(DiscPrefab.name, position, Quaternion.identity);
	}

	public void CmdThrowDisc(GameObject disc, GameObject from, GameObject to)
	{
		if (to != null)
		{
			DiscController controller = disc.GetComponent<DiscController>();

			Vector3 direction = to.transform.position - from.transform.position;
			direction.Normalize();

			CmdThrowDisc(disc, from, to, direction);
		}
	}

	public void CmdThrowDisc(GameObject disc, GameObject from, GameObject to, Vector3 direction)
	{
		if (to != null)
		{
			DiscController controller = disc.GetComponent<DiscController>();

			bool enableTrail = OpponentsInRange.Count >= DoubleTeamLimit;

			controller.CmdThrow(from, to, direction, enableTrail);
		}
	}

	public void CmdDropDisc(GameObject disc, bool restrictTeam)
	{
		DiscController controller = disc.GetComponent<DiscController>();
		controller.CmdDrop(gameObject, restrictTeam);
	}


	public bool HasDiscInHands
	{
		get
		{
			return Disc != null;
		}
	}

	public CapsuleCollider Catcher;

	public float CatchRadius
	{
		get
		{
			return Catcher.radius;
		}
		set
		{
			Catcher.radius = value;
		}
	}

	public Vector3 GetDiscSpacePushOut(Vector3 pos)
	{
		if (Spacer.IsActive)
		{
			Vector3 dir = new Vector3(pos.x, 0f, pos.z) - new Vector3(transform.position.x, 0f, transform.position.z);
			float dist = dir.magnitude;
			if (dist < Spacer.Radius && dist > Mathf.Epsilon)
				return dir.normalized * (Spacer.Radius - dist);
		}

		return Vector3.zero;
	}

	public List<GameObject> OpponentsInRange = new List<GameObject>();
	public void UpdateOpponentsInRange()
	{
		OpponentsInRange = FindOpponentInRange(StallOutRadius);
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(this.transform.position, Catcher.radius);
	}
}
