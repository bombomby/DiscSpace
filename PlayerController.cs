using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : MonoBehaviour, IPunInstantiateMagicCallback
{
	public PhotonView PV;
	public AimController AC;
	public RPGStats Stats;
	public PlayerAudio Audio;
	public PlayerStats GameStats;

	public GameObject GoalVFX;

	//public class NetworkTransform
	//{
	//	public Vector3 Position;
	//	public Quaternion Rotation;
	//	public Vector3 Velocity;
	//}
	//public NetworkTransform NetTransform;

	//public class NetworkSettings
	//{
	//	public float LerpRatio = 0.3f;
	//}
	//public NetworkSettings NetSettings;


	public float RotationSpeed = 90.0f;
	public Transform Player;
	
	private Animator PlayerAnimator;

	private Rigidbody Body;
	private bool isGrounded;

	private bool isJumping;
	private float lastJumpStart;

	const float JumpDoubleTapTime = 0.3f;

	private bool isLayingOut;
	public bool IsLayingOut => isLayingOut;

	private float knockDownTimer;

	public float JumpForce;
	public float LayoutForce;

	public float LayoutStandUpTime;

	public bool IsBot = false;

	public Vector3 Jump;
	public Vector3 Layout;

	// Start is called before the first frame update
	private void Awake()
	{
		PV = GetComponent<PhotonView>();
		AC = GetComponent<AimController>();
		Body = GetComponent<Rigidbody>();
		IsBot = GetComponent<BotController>() != null;
		Stats = GetComponent<RPGStats>();
		Audio = GetComponent<PlayerAudio>();
		GameStats = GetComponent<PlayerStats>();
	}

	void Start()
    {
		if (PV.IsMine && !IsBot)
		{
			Camera.main.GetComponent<CameraController>().Target = gameObject;
			FrisbeeGame.Instance.MainPlayer = gameObject;
		}

		InitPlayer(Player);
	}

	public void InitPlayer(Transform player)
	{
		Player = player;
		PlayerAnimator = player != null ? player.GetComponent<Animator>() : null;
	}

	public bool IsGrounded { get { return isGrounded; } }

	void OnCollisionEnter(Collision collision)
	{
		if (!isGrounded && collision.gameObject.tag == "Ground")
		{
			isGrounded = true;

			if (isLayingOut)
			{
				knockDownTimer += LayoutStandUpTime;
			}

			isJumping = false;
			isLayingOut = false;


			if (Body.velocity.magnitude > Mathf.Epsilon)
			{
				Audio.OnAudioEvent(PlayerAudio.EventType.Grounded);
			}

			if (PhotonNetwork.IsMasterClient && AC.HasDiscInHands && FrisbeeGame.Instance.CanScore)
			{
				BoxCollider zone = FrisbeeGame.Instance.GetGoalZone(AC.Team);
				if (zone.bounds.Contains(transform.position))
				{
					DiscController disc = AC.Disc.GetComponent<DiscController>();
					FrisbeeGame.Instance.Score(disc.CurrentThrower, gameObject, AC.Team);
					CmdScore();
				}
			}

		}
	}

	public void CmdScore()
	{
		PV.RPC("RPC_PlayerScore", RpcTarget.All, PV.ViewID);
	}

	[PunRPC]
	void RPC_PlayerScore(int viewID)
	{
		PhotonView pv = PhotonNetwork.GetPhotonView(viewID);
		if (pv != null)
		{
			Instantiate(GoalVFX, pv.transform.position + new Vector3(0, 0.75f, 0), Quaternion.identity);
		}
	}

	bool CanMove
	{
		get { return (!GetComponent<AimController>().HasDiscInHands || FrisbeeGame.Instance.CurrentState == FrisbeeGame.GameState.Lobby) && FrisbeeGame.Instance.CanMove && !IsKnockedDown; }
	}

	bool CanJump
	{
		get
		{
			if (!Stats.CurrentStats.CanJump)
				return false;

			return FrisbeeGame.Instance.CanJump && ((FrisbeeGame.Instance.CurrentState != FrisbeeGame.GameState.Game_Playing) || !GetComponent<AimController>().HasDiscInHands);
		}
	}

	bool CanLayout
	{
		get
		{
			return Stats.CurrentStats.CanLayout && isJumping && (Time.time - lastJumpStart) < JumpDoubleTapTime;
		}
	}


	public void MoveTo(Vector3 target)
	{
		Debug.DrawLine(transform.position, target, Color.red);

		//if (CanMove)
		{
			Vector3 direction = target - transform.position;
			float dist = direction.magnitude;

			direction.Normalize();
			Vector3 velocity = direction * Mathf.Min(Stats.CurrentStats.MoveSpeed * Stats.CurrentStats.MoveSpeedBurstMultiplier, dist / Time.deltaTime);
			Body.velocity = velocity;
		}
	}

	//void UpdateNetwork()
	//{
	//	if (!PV.IsMine && NetTransform != null)
	//	{
	//		transform.position = Vector3.Lerp(transform.position, NetTransform.Position, NetSettings.LerpRatio);
	//		transform.rotation = Quaternion.Lerp(transform.rotation, NetTransform.Rotation, NetSettings.LerpRatio);
	//		Body.velocity = NetTransform.Velocity;
	//	}
	//}

	public const float ObserverSpeedMultiplier = 2.0f;

	public bool IsKnockedDown { get { return knockDownTimer > Mathf.Epsilon; } }


	void StartJump()
	{
		Stats.CurrentStats.Jump(gameObject);

		Body.AddForce(Jump * JumpForce, ForceMode.Impulse);

		isJumping = true;
		lastJumpStart = Time.time;

		SetAnimationTrigger("Jump");
		Audio.OnAudioEvent(PlayerAudio.EventType.Jump);
	}

	[PunRPC]
	public void RPC_Layout()
	{
		Body.AddForce(transform.rotation * Layout * LayoutForce, ForceMode.Impulse);

		isLayingOut = true;

		SetAnimationTrigger("Layout");
		Audio.OnAudioEvent(PlayerAudio.EventType.Jump);
	}

	public void SetAnimationTrigger(string trigger)
	{
		PlayerAnimator.SetTrigger(trigger);
	}


	void StartLayout()
	{
		Stats.CurrentStats.Layout(gameObject);
		PV.RPC("RPC_Layout", RpcTarget.All);
	}

	void Update()
    {
		//UpdateNetwork();

		Vector3 inputVelocity = new Vector3(0, 0, 0);

		if (PV.IsMine && GetComponent<BotController>() == null && FrisbeeGame.Instance.CanProcessKeyboard && !CameraController.IsFreeCamEnabled)
		{
			float h = Input.GetAxis("Horizontal") + Input.GetAxis("Horizontal Movement");
			float v = Input.GetAxis("Vertical") + Input.GetAxis("Vertical Movement");

			Vector3 movement = new Vector3(h, 0.0f, v);

			Vector3 cameraForward = Camera.main.transform.forward;
			cameraForward.y = 0.0f;
			cameraForward.Normalize();

			Quaternion cameraRotation = Quaternion.LookRotation(cameraForward, Vector3.up);
			//movement = Quaternion.Inverse(cameraRotation) * movement;
			movement = cameraRotation * movement;

			if (movement.magnitude > 1.0f)
				movement.Normalize();

			inputVelocity = movement * Stats.CurrentStats.MoveSpeed;

			float burstMode = Input.GetAxis("Sprint");

			if (burstMode > Mathf.Epsilon && Stats.CurrentStats.CanBurst)
				inputVelocity *= Mathf.Lerp(1.0f, Stats.CurrentStats.MoveSpeedBurstMultiplier, burstMode);

			if (isLayingOut)
				inputVelocity *= Stats.CurrentStats.LayoutSpeedMultiplier;

			if (AC.IsObserver)
				inputVelocity = inputVelocity * ObserverSpeedMultiplier;

			if (knockDownTimer > 0.0f)
				knockDownTimer -= Time.deltaTime;

			if (CanMove)
			{
				if (burstMode > Mathf.Epsilon && inputVelocity.magnitude > Mathf.Epsilon)
					GetComponent<RPGStats>().CurrentStats.BurstMode = burstMode;

				Body.velocity = new Vector3(inputVelocity.x, Body.velocity.y, inputVelocity.z);
			}

			if (CanLayout)
			{
				if (Input.GetButtonDown("Jump"))
				{
					StartLayout();
				}
			}

			if (CanJump)
			{
				if (Input.GetButtonDown("Jump") && isGrounded)
				{
					StartJump();
					isGrounded = false;
				}
			}
		}

		PlayerAnimator.SetFloat("Speed", Body.velocity.magnitude);

		if (PV.IsMine && AC.HasDiscInHands && !IsBot && inputVelocity.magnitude < Mathf.Epsilon)
		{
			inputVelocity = Camera.main.transform.forward;
		}

		Vector3 direction = Body.velocity.magnitude > 1.0f ? Body.velocity : inputVelocity;
		Vector3 direction2d = new Vector2(direction.x, direction.z);

		if (direction2d.magnitude > Mathf.Epsilon && !IsKnockedDown)
		{
			direction.y = 0.0f;
			direction.Normalize();

			Quaternion desiredRotation = Quaternion.LookRotation(direction, Vector3.up);

			float angle = Vector3.Angle(transform.forward, desiredRotation * Vector3.forward);

			if (angle > Mathf.Epsilon)
			{
				transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, Mathf.Min(1.0f, RotationSpeed * Time.deltaTime / angle));
			}
		}

		if (!CanMove && !CanJump && IsGrounded)
		{
			Body.constraints = RigidbodyConstraints.FreezeAll;
		}
		else
		{
			Body.constraints = RigidbodyConstraints.FreezeRotation;
		}
	}

	const float GameHeightLimit = 1.1f;
	const float PushOutSpeed = 3.0f;
	const float PushOutSlideDampening = 0.61803f;
	
	void FixedUpdate()
	{
		if (FrisbeeGame.IsInState(FrisbeeGame.GameState.Game_Playing) && PV.IsMine)
		{
			// Chack Max Height
			if (transform.position.y > GameHeightLimit)
			{
				Body.velocity = new Vector3(Body.velocity.x, Mathf.Min(Body.velocity.y, 0.0f), Body.velocity.z);
			}

			Vector3 currPos = transform.position;
			Vector3 nextPos = currPos + Body.velocity * Time.fixedDeltaTime;

			// Check Out-of-Bounds
			{
				BoxCollider gameCollider = FrisbeeGame.Instance.GetTeamStatus(AC.Team) == FrisbeeGame.TeamStatus.Offence ? FrisbeeGame.Instance.ActiveBoundsOffence : FrisbeeGame.Instance.ActiveBoundsDefence;
				Bounds area = new Bounds(gameCollider.center, gameCollider.size);

				if (Utils.Distance(area, nextPos) > Utils.Distance(area, transform.position))
				{
					float maxVelocity = Body.velocity.magnitude;

					Vector3 reflection = 
						new Vector3(
							(nextPos.x < area.min.x || nextPos.x > area.max.x) ? -1.0f : 0f,
							(nextPos.y < area.min.y || nextPos.y > area.max.y) ? -1.0f : 0f,
							(nextPos.z < area.min.z || nextPos.z > area.max.z) ? -1.0f : 0f
						);
					
					Body.velocity = (Body.velocity + Vector3.Scale(reflection, Body.velocity)).normalized * maxVelocity;
				}
			}

			// Check Disc Space
			List<GameObject> players = FrisbeeGame.Instance.GetPlayers();
			foreach (GameObject player in players)
			{
				if (player != gameObject)
				{
					AimController ac = player.GetComponent<AimController>();
					Vector3 currPushOut = ac.GetDiscSpacePushOut(currPos);
					Vector3 nextPushOut = ac.GetDiscSpacePushOut(nextPos);


					if (currPushOut.magnitude > 0.001f)
					{
						float currVelocity = Body.velocity.magnitude;
						Body.velocity = currPushOut.normalized * PushOutSpeed;
					}
					else if (nextPushOut.magnitude > 0.001f)
					{
						Vector3 slideDir = Vector3.Cross((player.transform.position - transform.position).normalized, Vector3.up);
						slideDir.y = 0.0f;
						slideDir.Normalize();

						Vector3 flatVelocity = new Vector3(Body.velocity.x, 0f, Body.velocity.z);

						if (flatVelocity.magnitude > Mathf.Epsilon)
						{
							if (Vector3.Dot(flatVelocity, slideDir) < 0.0f)
							{
								slideDir = -slideDir;
							}

							Body.velocity = Body.velocity - flatVelocity + slideDir * flatVelocity.magnitude * PushOutSlideDampening;
						}
					}
				}
			}
		}
	}

	public void Teleport(Vector3 pos, Quaternion rot)
	{
		PV.RPC("RPC_Teleport", RpcTarget.All, pos, rot);
	}

	public void Teleport(Vector3 pos)
	{
		Teleport(pos, transform.rotation);
	}


	[PunRPC]
	public void RPC_Teleport(Vector3 pos, Quaternion rot)
	{
		//if (isLocalPlayer)
		{
			transform.position = pos;
			transform.rotation = rot;
		}
	}

	public void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		if (!IsBot)
		{
			info.Sender.TagObject = gameObject;

			if (PhotonNetwork.IsMasterClient)
			{
				// VS TODO: Rebalance Teams
				AC.Team = 0;// FrisbeeGame.Instance.SelectTeamForNewPlayer();
				FrisbeeGame.Instance.OnPlayerCreated(gameObject);
			}
		}
	}

	public bool IsDestroyed { get; set; } = false;

	public void OnDestroy()
	{
		IsDestroyed = true;

		if (PhotonNetwork.IsMasterClient)
		{
			FrisbeeGame.Instance.OnPlayerDestroyed(gameObject);
		}
	}


	//public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	//{
	//	if (stream.IsWriting)
	//	{
	//		stream.SendNext(transform.position);
	//		stream.SendNext(transform.rotation);
	//		stream.SendNext(Body.velocity);
	//	}
	//	else
	//	{
	//		if (NetTransform == null)
	//			NetTransform = new NetworkTransform();

	//		NetTransform.Position = (Vector3)stream.ReceiveNext();
	//		NetTransform.Rotation = (Quaternion)stream.ReceiveNext();
	//		NetTransform.Velocity = (Vector3)stream.ReceiveNext();
	//	}
	//}
}
