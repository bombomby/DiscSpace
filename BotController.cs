﻿using Photon.Pun;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BotController : MonoBehaviour
{
	PhotonView PV;
	AimController AC;
	RPGStats Stats;
	Rigidbody Body;
	PlayerController PC;

	public float MaxThrowingDelay;
	public float MaxMovingDelay;

	PlayerController Mover;
	float CurrentThrowingDelay;
	float CurrentMovingDelay;

	Vector3 NextTarget;

	// Start is called before the first frame update
	void Awake()
    {
		Body = GetComponent<Rigidbody>();
		Stats = GetComponent<RPGStats>();
		PV = GetComponent<PhotonView>();
		AC = GetComponent<AimController>();
		PC = GetComponent<PlayerController>();
		Mover = GetComponent<PlayerController>();
	}

	public float EndZoneProbability;

	Vector3 GetNextRandomPoint()
	{
		FrisbeeGame.TeamStatus status = FrisbeeGame.Instance.GetTeamStatus(AC.Team);
		switch (status)
		{
			case FrisbeeGame.TeamStatus.Offence:
			case FrisbeeGame.TeamStatus.Defence:
				if (Random.Range(0.0f, 1.0f) < EndZoneProbability)
				{
					BoxCollider endZone = FrisbeeGame.Instance.GetGoalZone(status == FrisbeeGame.TeamStatus.Offence ? AC.Team : (1 - AC.Team));
					//Debug.DrawLine(transform.position, endZone.bounds.center, Color.green, 3.0f);
					return new Vector3(Random.Range(endZone.bounds.min.x, endZone.bounds.max.x), 0.0f, Random.Range(endZone.bounds.min.z, endZone.bounds.max.z));
				}
				break;
		}

		BoxCollider area = FrisbeeGame.Instance.ActiveBoundsOffence;

		return new Vector3(Random.Range(area.bounds.min.x, area.bounds.max.x), 0.0f, Random.Range(area.bounds.min.z, area.bounds.max.z));
	}

	GameObject FindDiscOnGround()
	{
		foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Disc"))
		{
			DiscController disc = obj.GetComponent<DiscController>();
			if (disc.CanPickup(gameObject))
				return obj;
		}
		return null;
	}

	GameObject FindDiscForInterception()
	{
		foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Disc"))
		{
			DiscController disc = obj.GetComponent<DiscController>();
			if (disc.CanIntercept(gameObject))
			{
				if (disc.CurrentTarget != null)
				{
					GameObject closestBot = FrisbeeGame.Instance.Teams[AC.Team].Bots.OrderBy(b => (b.transform.position - disc.transform.position).magnitude).FirstOrDefault();
					if (closestBot == gameObject)
						return obj;
				}
			}
		}
		return null;
	}

	float CalcBestCurve(GameObject target)
	{
		if (AC.OpponentsInRange.Count > 0)
		{
			Vector3 dir = (target.transform.position - transform.position);
			dir.y = 0.0f;

			float curve = 0.0f;

			if (dir.magnitude > 0.1f)
			{
				dir = dir.normalized;

				foreach (GameObject opponent in AC.OpponentsInRange)
				{
					Vector3 opponentDir = opponent.transform.position - transform.position;
					opponentDir.y = 0.0f;
					opponentDir = opponentDir.normalized;

					float angle = Vector3.SignedAngle(dir, opponentDir, Vector3.up);

					curve += -Mathf.Sign(angle) * 1.0f;
				}
			}

			return Mathf.Clamp(curve, -1.0f, 1.0f);
		}

		return Random.Range(-1.0f, 1.0f);
	}

	void UpdateDiscThrow()
	{
		if (CurrentState != BotState.HOLD_DISC)
		{
			PV.RPC("RPC_ChangeState", RpcTarget.All, BotState.HOLD_DISC, transform.position);
		}

		CurrentThrowingDelay += Time.deltaTime;

		if (CurrentThrowingDelay > MaxThrowingDelay)
		{
			Vector3 targetDirection = FrisbeeGame.Instance.GetGoalZone(AC.Team).bounds.center - transform.position;
			GameObject target = AC.SelectBestTarget(targetDirection);
			AC.CurrentTarget = target;
			float ratio = CalcBestCurve(target);
			Vector3 throwDirection = AC.CalculateThrowDirectionFromRatio(ratio);
			AC.CmdThrowDisc(AC.Disc, gameObject, AC.CurrentTarget, throwDirection);
			CurrentThrowingDelay = 0.0f;
		}
	}

	enum BotState
	{
		IDLE,
		HOLD_DISC,
		MOVE,
		PICKUP_DISC,
		CHASE,
	}

	BotState CurrentState = BotState.IDLE;

	bool UpdateIfDiscOnTheGround()
	{
		GameObject discOnGround = FindDiscOnGround();
		if (discOnGround != null)
		{
			Vector3 discTarget = discOnGround.transform.position;

			if (CurrentState != BotState.PICKUP_DISC)
			{
				PV.RPC("RPC_ChangeState", RpcTarget.All, BotState.PICKUP_DISC, discTarget);
			}
			else
			{
				NextTarget = discTarget;
			}

			return true;
		}
		return false;
	}

	bool UpdateIfDiscInTheAir()
	{
		GameObject discForInterception = FindDiscForInterception();
		if (discForInterception != null)
		{
			DiscController disc = discForInterception.GetComponent<DiscController>();
			Vector3 targetPos = disc.CurrentTarget.transform.position;
			Vector3 interceptPos = Vector3.Lerp(disc.transform.position, targetPos, 0.85f);

			Vector3 chaseTarget = new Vector3(interceptPos.x, transform.position.y, interceptPos.z);

			if (CurrentState != BotState.CHASE)
			{
				PV.RPC("RPC_ChangeState", RpcTarget.All, BotState.CHASE, chaseTarget);
			}
			else
			{
				NextTarget = chaseTarget;
			}
			
			return true;
		}
		return false;
	}

	public bool DisableMovement;

	[PunRPC]
	void RPC_ChangeState(BotState state, Vector3 target)
	{
		CurrentState = state;
		NextTarget = target;
	}

	void SelectNextTarget()
	{
		Vector3 target;

		// Get Random Point
		do
		{
			target = GetNextRandomPoint();
		} while (Vector3.Distance(target, NextTarget) < 5.0f);

		PV.RPC("RPC_ChangeState", RpcTarget.All, BotState.MOVE, target);
	}

	void Update()
    {
#if UNITY_EDITOR
		if (DisableMovement)
			return;
#endif

		if (PV.IsMine)
		{
			if (AC.Disc != null)
			{
				UpdateDiscThrow();
			}
			else
			{
				CurrentMovingDelay += Time.deltaTime;

				if (!UpdateIfDiscOnTheGround() && !UpdateIfDiscInTheAir())
				{
					if (CurrentMovingDelay > MaxMovingDelay)
					{
						CurrentMovingDelay = 0.0f;
						SelectNextTarget();
					}
				}
			}

			if (AC.Disc == null && NextTarget != Vector3.zero)
			{
				bool hasReachedDestination = MoveTo(NextTarget, Time.deltaTime);
				if (hasReachedDestination && IsReceivingDisc())
					SelectNextTarget();
			}
		}
    }

	const float CloseProximityDistance = 1.0f;

	void FixedUpdate()
	{
		if (!PV.IsMine)
		{
			if (CurrentState == BotState.MOVE && Vector3.Distance(transform.position, NextTarget) < CloseProximityDistance)
			{
				MoveTo(NextTarget, Time.fixedDeltaTime);
			}
			else
			{
				PC.UpdateNetwork();
			}
		}
	}

	bool IsReceivingDisc()
	{
		foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Disc"))
		{
			DiscController disc = obj.GetComponent<DiscController>();
			if (disc.CurrentState == DiscController.DiscState.FLYING)
			{
				if (disc.CurrentTarget == gameObject)
					return true;
			}
		}
		return false;
	}

	public bool MoveTo(Vector3 target, float deltaTime)
	{
		//Debug.DrawLine(transform.position, target, Color.red);

		Vector3 direction = target - transform.position;
		float dist = direction.magnitude;

		direction.Normalize();

		float maxSpeed = Stats.CurrentStats.MoveSpeed * Stats.CurrentStats.MoveSpeedBurstMultiplier;

		Vector3 velocity = direction * Mathf.Min(maxSpeed, dist / deltaTime);
		Body.velocity = velocity;

		bool hasReachedDestination = maxSpeed >= (dist / deltaTime);
		return hasReachedDestination;
	}
}
