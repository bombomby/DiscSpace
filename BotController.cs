using Photon.Pun;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BotController : MonoBehaviour
{
	PhotonView PV;
	AimController AC;

	public float MaxThrowingDelay;
	public float MaxMovingDelay;

	PlayerController Mover;
	float CurrentThrowingDelay;
	float CurrentMovingDelay;

	Vector3 NextTarget;

	// Start is called before the first frame update
	void Awake()
    {
		PV = GetComponent<PhotonView>();
		AC = GetComponent<AimController>();
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
					Debug.DrawLine(transform.position, endZone.bounds.center, Color.green, 3.0f);
					return new Vector3(Random.Range(endZone.bounds.min.x, endZone.bounds.max.x), 0.0f, Random.Range(endZone.bounds.min.z, endZone.bounds.max.z));
				}
				break;
		}

		Rect area = FrisbeeGame.Instance.ActiveArea;

		return new Vector3(Random.Range(area.x, area.xMax), 0.0f, Random.Range(area.y, area.yMax));
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


	void UpdateDiscThrow()
	{
		CurrentThrowingDelay += Time.deltaTime;

		if (CurrentThrowingDelay > MaxThrowingDelay)
		{
			Vector3 targetDirection = FrisbeeGame.Instance.GetGoalZone(AC.Team).bounds.center - transform.position;
			GameObject target = AC.SelectBestTarget(targetDirection);
			AC.CurrentTarget = target;
			float ratio = Random.Range(-1.0f, 1.0f);
			Vector3 throwDirection = AC.CalculateThrowDirectionFromRatio(ratio);
			AC.CmdThrowDisc(AC.Disc, gameObject, AC.CurrentTarget, throwDirection);
			CurrentThrowingDelay = 0.0f;
		}
	}

	bool UpdateIfDiscOnTheGround()
	{
		GameObject discOnGround = FindDiscOnGround();
		if (discOnGround != null)
		{
			NextTarget = discOnGround.transform.position;
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

			NextTarget = new Vector3(interceptPos.x, transform.position.y, interceptPos.z);
			return true;
		}
		return false;
	}

    void Update()
    {
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

						Vector3 target;

						// Get Random Point
						do
						{
							target = GetNextRandomPoint();
						} while (Vector3.Distance(target, NextTarget) < 5.0f);

						NextTarget = target;
					}
				}
			}
		}

		if (AC.Disc == null && NextTarget != Vector3.zero)
		{
			Mover.MoveTo(NextTarget);
		}
    }
}
