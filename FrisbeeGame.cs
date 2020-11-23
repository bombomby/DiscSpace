using DuloGames.UI;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FrisbeeGame : MonoBehaviourPunCallbacks
{
	private PhotonView PV;
	private NetworkLobby Lobby;

	private string DiscPrefab = "DiscPrefab";
	private string BotPrefab = "BotPlayer";
	private string PlayerPrefab = "MainPlayer";

	private GameObject CurrentPlayer = null;

	public AnnouncementUI Announcement;

	public BoxCollider ActiveBoundsOffence;
	public BoxCollider ActiveBoundsDefence;

	private GameObject mainPlayer = null;
	public GameObject MainPlayer
	{
		get
		{
			return mainPlayer;
		}
		set
		{
			mainPlayer = value;
		}
	}

	public Team[] Teams;

	int MaxScore { get; set; } = 11;
	bool AutoRebalanceTeams { get; set; } = true;

	bool AutoAddBots { get; set; } = true;

	public static FrisbeeGame Instance;

	public DateTime GameStartedTimestamp { get; set; }

	public static bool IsInState(GameState state)
	{
		return (Instance != null) && (Instance.CurrentState & state) != 0 && Instance.CurrentStateElapsed > Time.deltaTime;
	}

	public int SelectTeamForNewPlayer()
	{
		if (IsInState(GameState.Game))
		{
			return Teams[0].Players.Count > Teams[1].Players.Count ? 1 : 0;
		}
		return 0;
	}

	private void Awake()
	{
		Instance = this;

		PV = GetComponent<PhotonView>();
		Lobby = GetComponent<NetworkLobby>();
	}

	public delegate void GameStartedDelegate();
	public event GameStartedDelegate GameStarted;

	public BoxCollider GetGoalZone(int teamIndex)
	{
		return Teams[(teamIndex + TotalScore + 1) % 2].GetComponent<BoxCollider>();
	}


	public enum TeamStatus
	{
		Offence,
		Defence,
	}
	public TeamStatus GetTeamStatus(int teamIndex)
	{
		GameObject[] discs = GameObject.FindGameObjectsWithTag("Disc");

		foreach (GameObject disc in discs)
		{
			if (disc.GetComponent<DiscController>().TeamPossession == teamIndex)
				return TeamStatus.Offence;
		}

		return TeamStatus.Defence;
	}

	Vector3 FindAvailableSpawnPoint()
	{
		List<GameObject> spawns = new List<GameObject>();
		spawns.AddRange(GameObject.FindGameObjectsWithTag("Respawn0"));
		spawns.AddRange(GameObject.FindGameObjectsWithTag("Respawn1"));

		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

		List<GameObject> availableSpawns = new List<GameObject>();

		foreach (GameObject obj in spawns)
		{
			bool intersectPlayer = false;

			for (int i = 0; i < players.Length; ++i)
			{
				if (Vector3.Distance(players[i].transform.position, obj.transform.position) < 2.0f)
					intersectPlayer = true;
			}

			if (!intersectPlayer)
				availableSpawns.Add(obj);
		}

		if (availableSpawns.Count > 0)
		{
			return availableSpawns[UnityEngine.Random.Range(0, availableSpawns.Count)].transform.position;
		}

		return FrisbeeGame.Instance.Teams[0].transform.Find("Brick").position;
	}

	public override void OnJoinedRoom()
	{
		Vector3 pos = FindAvailableSpawnPoint();
		Quaternion rot = Quaternion.LookRotation(-Vector3.forward, Vector3.up);
		SpawnPlayer(pos, rot);
	}

	[PunRPC]
	void RPC_SwitchPossession(int offensiveTeam)
	{
		SetAnnouncement(null, 0, MainPlayer.GetComponent<AimController>().Team == offensiveTeam ? AnnouncementUI.SoundFX.Offence : AnnouncementUI.SoundFX.Defence);
	}

	public void CmdSwitchPossession(int offensiveTeam)
	{
		PV.RPC("RPC_SwitchPossession", RpcTarget.All, offensiveTeam);
	}

	void ForceUpdateGameState(Player player)
	{
		PV.RPC("RPC_GameStateChanged", player, CurrentState);
		foreach (Team team in Teams)
			team.ForceUpdate(player);

		GameObject[] discs = GameObject.FindGameObjectsWithTag("Disc");
		for (int i = 0; i < discs.Length; ++i)
		{
			discs[i].GetComponent<DiscController>().ForceUpdate(player);
		}
	}

	public void Score(GameObject from, GameObject to, int teamIndex)
	{
		Teams[teamIndex].Score = Teams[teamIndex].Score + 1;

		CurrentState = GameState.Game_Finishing;
		CmdAnnouncement(String.Format("Goal! {0} scores a point!", Teams[teamIndex].TeamName), FinishingPointDelaySec, AnnouncementUI.SoundFX.Goal);

		if (to != null)
		{
			to.GetComponent<PlayerStats>().AddScore();
		}

		if (from != null && to != null && from.GetComponent<AimController>().Team == to.GetComponent<AimController>().Team)
		{
			from.GetComponent<PlayerStats>().AddAssist();
		}
	}

	private int StartingPointDelaySec = 3;
	private int FinishingPointDelaySec = 3;

	public void CmdCreateDisc()
	{
		if (MainPlayer != null)
		{
			MainPlayer.GetComponent<AimController>().CmdCreateDisc(MainPlayer.transform.position);
		}
	}

	public enum GameState
	{
		None = 0x0,
		Lobby = 0x1,
		Game_Starting = 0x2,
		Game_Playing = 0x4,
		Game_Finishing = 0x8,

		Game = Game_Starting | Game_Playing | Game_Finishing,
		Any = 0x7FFFFFFF,
	}

	GameState currentState = GameState.None;
	public GameState CurrentState
	{
		get { return currentState; }
		set
		{
			if (PhotonNetwork.IsMasterClient)
			{
				PV.RPC("RPC_GameStateChanged", RpcTarget.All, value);
			}
			else
			{
				RPC_GameStateChanged(value);
			}
		}
	}
	float CurrentStateElapsed = 0.0f;

	void OnGameFinished()
	{
		UIWindow.GetWindow(UIWindowID.GameFinished).Show();
	}

	[PunRPC]
	private void RPC_GameStateChanged(GameState state)
	{
		if (CurrentState == GameState.Lobby && state == GameState.Game_Starting)
		{
			GameStartedTimestamp = DateTime.Now;
			GameStarted.Invoke();
		}

		if (CurrentState == GameState.Game_Finishing && state == GameState.Lobby)
		{
			OnGameFinished();
		}

		currentState = state;
		CurrentStateElapsed = 0.0f;
	}

	[PunRPC]
	private void RPC_Announcement(String message, int durationSec, String sfx = null)
	{
		SetAnnouncement(message, durationSec, sfx);
	}

	public void CmdAnnouncement(String message, int durationSec, AnnouncementUI.SoundFX sfx = AnnouncementUI.SoundFX.None)
	{
		AnnouncementUI.SoundFXEntry entry = Announcement.GetSFX(sfx);
		PV.RPC("RPC_Announcement", RpcTarget.All, message, durationSec, entry != null ? entry.ClipFX.name : null);
	}

	public void SetAnnouncement(String message, int durationSec, AnnouncementUI.SoundFX sfx = AnnouncementUI.SoundFX.None)
	{
		AnnouncementUI.SoundFXEntry entry = Announcement.GetSFX(sfx);
		Announcement.Show(message, durationSec, entry != null ? entry.ClipFX.name : null);
	}

	private void SetAnnouncement(String message, int durationSec, String sfx)
	{
		Announcement.Show(message, durationSec, sfx);
	}

	public bool CanMove
	{
		get { return CanProcessKeyboard && (CurrentState == GameState.Lobby || CurrentState == GameState.Game_Playing); }
	}

	public bool CanJump
	{
		get { return CanProcessKeyboard && ((CurrentState & (GameState.Lobby | GameState.Game_Playing | GameState.Game_Finishing)) != 0); }
	}

	public bool CanScore
	{
		get { return CurrentState == GameState.Game_Playing; }
	}

	public bool CanProcessKeyboard
	{
		get { return !MenuManager.Instance.HasOpenWindow(UIWindow.Interaction.Keyboard); }
	}

	public bool CanProcessMouse
	{
		get { return !MenuManager.Instance.HasOpenWindow(UIWindow.Interaction.Mouse); }
	}



	public int TotalScore
	{
		get { return Teams[0].Score + Teams[1].Score; }
	}

	void VerifyPlayersInbound()
	{
		bool allPlayersInbound = true;

		BoxCollider gameCollider = ActiveBoundsOffence;
		Bounds area = new Bounds(gameCollider.center, gameCollider.size);

		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
		foreach (GameObject player in players)
		{
			if (!area.Contains(player.transform.position))
				allPlayersInbound = false;
		}

		if (!allPlayersInbound)
			ResetPlayers();
	}

	void VerifyDiscCount()
	{
		GameObject[] discs = GameObject.FindGameObjectsWithTag("Disc");
		if (discs.Length != 1)
			ResetDisc(true);
	}

	void UpdateState()
	{
		switch (CurrentState)
		{
			case GameState.Game_Starting:
				if (CurrentStateElapsed > StartingPointDelaySec)
				{
					VerifyPlayersInbound();
					VerifyDiscCount();
					CmdAnnouncement(null, 0, AnnouncementUI.SoundFX.GameOn);
					CurrentState = GameState.Game_Playing;
				}
				break;

			case GameState.Game_Finishing:
				if (CurrentStateElapsed > FinishingPointDelaySec)
				{
					if (Teams[0].Score >= MaxScore || Teams[1].Score >= MaxScore)
						CmdGameOver();
					else
						CmdStartPoint();
				}
				break;

			default:
				break;
		}

		CurrentStateElapsed += Time.deltaTime;
	}

	public void RequestNewGame()
	{
		UIWindow.GetWindow(UIWindowID.StartMatch).Show();
	}

	public void RequestNewGame(int teamSize, int pointCap, bool autoAddBots, bool autoRebalanceTeams)
	{
		PV.RPC("RPC_MasterStartNewGame", RpcTarget.MasterClient, teamSize, pointCap, autoAddBots, autoRebalanceTeams);
	}

	[PunRPC]
	private void RPC_MasterStartNewGame(int teamSize, int pointCap, bool autoAddBots, bool autoRebalanceTeams)
	{
		if (CurrentState == GameState.Lobby)
		{
			CmdStartGame(teamSize, pointCap, autoAddBots, autoRebalanceTeams);
			CmdStartPoint();
		}
	}

	public void RequestFinishGame()
	{
		PV.RPC("RPC_MasterFinishGame", RpcTarget.MasterClient);
	}

	[PunRPC]
	private void RPC_MasterFinishGame()
	{
		if ((CurrentState & GameState.Game) != 0)
			CmdGameOver();
	}

	// Update is called once per frame
	void Update()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			UpdateState();

#if UNITY_EDITOR
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				CmdStartGame(3, 2, true, true);
				CmdStartPoint();
			}

			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				CmdStartPoint();
			}
#endif

			UpdateCatchRadius();
		}
		else
		{
			CurrentStateElapsed += Time.deltaTime;
		}

		UpdatePlayerProximity();
	}

	void ResetTeams()
	{
		foreach (Team team in Teams)
			team.Reset();
	}

	GameObject CreateBot(String name, int teamIndex, Vector3 position, Quaternion rotation)
	{
		GameObject bot = PhotonNetwork.Instantiate(BotPrefab, position, rotation);
		bot.GetComponent<AimController>().Team = teamIndex;
		bot.GetComponent<PlayerTag>().PlayerName = name;
		return bot;
	}

	GameObject CreateDisc(Vector3 position)
	{
		return PhotonNetwork.Instantiate(DiscPrefab, position, Quaternion.identity);
	}

	public void CmdStartGame(int teamSize, int pointCap, bool autoAddBots, bool autoRebalanceTeams)
	{
		// Eject the player to get him up to speed
		Spaceship.Instance.CmdEject();

		foreach (Team team in Teams)
			team.MaxTeamSize = teamSize;

		MaxScore = pointCap;
		AutoAddBots = autoAddBots;
		AutoRebalanceTeams = autoRebalanceTeams;

		ResetTeams();
		UpdateTeams();
		EqualizeTeams();
		UpdateBots();
	}

	List<GameObject> GetRespawnPoints(int teamIndex)
	{
		List<GameObject> spawnPoints = new List<GameObject>();
		Transform respawnRoot = Teams[(teamIndex + TotalScore) % 2].transform.Find("Respawn");
		foreach (Transform item in respawnRoot)
		{
			spawnPoints.Add(item.gameObject);
		}
		return spawnPoints;
	}

	public void UpdateTeams()
	{
		foreach (Team team in Teams)
			team.Players.Clear();

		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

		for (int i = 0; i < players.Length; ++i)
		{
			PlayerController controller = players[i].GetComponent<PlayerController>();

			if (!controller.IsBot && !controller.IsDestroyed)
			{
				int teamIndex = players[i].GetComponent<AimController>().Team;
				if (0 <= teamIndex && teamIndex <= 2)
				{
					Teams[teamIndex].Players.Add(players[i]);
				}
			}
		}

	}

	private void EqualizeTeams()
	{
		if (AutoRebalanceTeams)
		{
			List<GameObject> swapTeam = new List<GameObject>();

			int avgPlayers = ((Teams[0].Players.Count + Teams[1].Players.Count) + 1) / 2;

			for (int i = 0; i < NumPlayingTeams; ++i)
			{
				Team team = Teams[i];
				for (int playerIndex = avgPlayers; playerIndex < team.Players.Count; ++playerIndex)
					swapTeam.Add(team.Players[playerIndex]);
			}

			foreach (GameObject player in swapTeam)
			{
				AimController ac = player.GetComponent<AimController>();

				Teams[ac.Team].Players.Remove(player);
				Teams[(ac.Team + 1) % 2].Players.Add(player);

				ac.Team = (ac.Team + 1) % 2;
			}
		}
	}

	public const int NumPlayingTeams = 2;
	public const int ObserverTeamIndex = 2;

	private void UpdateBots()
	{
		if (PhotonNetwork.IsMasterClient && AutoAddBots)
		{
			int maxTeamSize = 0;

			for (int teamIndex = 0; teamIndex < NumPlayingTeams; ++teamIndex)
			{
				int currTeamMaxSize = Math.Max(Teams[teamIndex].MaxTeamSize, Teams[teamIndex].Players.Count);
				maxTeamSize = Math.Max(currTeamMaxSize, maxTeamSize);
			}

			for (int teamIndex = 0; teamIndex < NumPlayingTeams; ++teamIndex)
			{
				Team team = Teams[teamIndex];

				// Add Bots to fill MaxTeamSize
				while (team.PlayerCount < maxTeamSize)
				{
					Vector3 pos = FindAvailableSpawnPoint();
					team.Bots.Add(CreateBot("Bot", teamIndex, pos, Quaternion.identity));
				}

				// Remove Bots if we exceed the limit
				while ((team.PlayerCount > maxTeamSize) && (team.Bots.Count > 0))
				{
					PhotonNetwork.Destroy(team.Bots[team.Bots.Count - 1]);
					team.Bots.RemoveAt(team.Bots.Count - 1);
				}
			}
		}
	}

	void RemoveAllDiscs()
	{
		GameObject[] discs = GameObject.FindGameObjectsWithTag("Disc");
		for (int i = 0; i < discs.Length; ++i)
		{
			if (discs[i].GetComponent<PhotonView>().IsMine)
			{
				PhotonNetwork.Destroy(discs[i]);
			}
		}
	}

	void ResetDisc(bool players)
	{
		RemoveAllDiscs();

		int offensiveTeam = Teams[0].ScoreTimestamp > Teams[1].ScoreTimestamp ? 1 : 0;

		if (players)
		{
			Team team = Teams[offensiveTeam];

			// Select a random player at first
			if (team.Players.Count > 0)
			{
				CreateDisc(team.Players[UnityEngine.Random.Range(0, team.Players.Count)].transform.position);
			}
			else if (team.Bots.Count > 0)  // Then from robots
			{
				CreateDisc(team.Bots[UnityEngine.Random.Range(0, team.Bots.Count)].transform.position);
			}
		}
		else
		{
			List<GameObject> spawnPoints = GetRespawnPoints(offensiveTeam);
			CreateDisc(spawnPoints[0].transform.position);
		}

	}

	void ResetPlayers()
	{
		for (int teamIndex = 0; teamIndex < NumPlayingTeams; ++teamIndex)
		{
			Team team = Teams[teamIndex];

			List<GameObject> spawnPoints = GetRespawnPoints(teamIndex);

			for (int playerIndex = 0; playerIndex < team.PlayerCount; ++playerIndex)
			{
				Transform spawn = spawnPoints[playerIndex].transform;
				GameObject player = null;

				if (playerIndex < team.Players.Count)
				{
					player = team.Players[playerIndex];
				}
				else if (playerIndex - team.Players.Count < team.Bots.Count)
				{
					player = team.Bots[playerIndex - team.Players.Count];
				}

				if (player != null)
					player.GetComponent<PlayerController>().PV.RPC("RPC_Teleport", RpcTarget.All, spawn.position, spawn.rotation);
			}
		}
	}

	public void CmdStartPoint()
	{
		ResetDisc(false);
		ResetPlayers();

		CurrentState = GameState.Game_Starting;
		CmdAnnouncement("Get Ready!", StartingPointDelaySec);
	}

	public void CmdGameOver()
	{
		List<GameObject> bots = GetBots();
		bots.ForEach(bot => { if (bot.GetComponent<PhotonView>().IsMine) PhotonNetwork.Destroy(bot); });
		RemoveAllDiscs();
		CurrentState = GameState.Lobby;
	}

	public void RequestPlayerDeath()
	{
		SetAnnouncement("You died... but how?", 5, AnnouncementUI.SoundFX.Death);
		Vector3 pos = FindAvailableSpawnPoint();
		CurrentPlayer.GetComponent<PlayerController>().PV.RPC("RPC_Teleport", RpcTarget.All, pos, CurrentPlayer.transform.rotation);
	}

	public void ReplenishStamina()
	{
		if (MainPlayer != null)
		{
			MainPlayer.GetComponent<RPGStats>().CurrentStats.Stamina = MainPlayer.GetComponent<RPGStats>().CurrentStats.MaxStamina;
		}
	}

	List<GameObject> GetBots()
	{
		List<GameObject> bots = new List<GameObject>();

		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
		for (int i = 0; i < players.Length; ++i)
			if (players[i].GetComponent<PlayerController>().IsBot)
				bots.Add(players[i]);

		return bots;
	}

	public List<GameObject> GetPlayers(int teamIndex = -1)
	{
		List<GameObject> result = new List<GameObject>();

		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
		for (int i = 0; i < players.Length; ++i)
			if (teamIndex == -1 || players[i].GetComponent<AimController>().Team == teamIndex)
				result.Add(players[i]);

		return result;
	}

	HashSet<string> SignatureMismachNames = new HashSet<string>();

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		string newPlayerName = Utils.StripSignature(newPlayer.NickName);

		// Verify integrity
		if (PhotonNetwork.IsMasterClient)
		{
			if (!NetworkLobby.Instance.IsValidPlayer(newPlayer))
			{
				SignatureMismachNames.Add(newPlayerName);
				SetAnnouncement(String.Format("{0} has been kicked out by anti-cheat system", newPlayerName), 3);
				PhotonNetwork.CloseConnection(newPlayer);
				return;
			}
			ForceUpdateGameState(newPlayer);
		}

		base.OnPlayerEnteredRoom(newPlayer);
		if (!newPlayer.IsLocal)
		{
			SetAnnouncement(String.Format("{0} has connected", newPlayerName), 3);
		}
	}

	void SpawnPlayer(Vector3 pos, Quaternion rot)
	{
		CurrentPlayer = PhotonNetwork.Instantiate(PlayerPrefab, pos, rot);
		CurrentPlayer.GetComponent<PlayerTag>().Name = NetworkLobby.CurrentPlayerName;
		CurrentState = GameState.Lobby;
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		base.OnPlayerLeftRoom(otherPlayer);
		if (!otherPlayer.IsLocal && PhotonNetwork.InRoom)
		{
			string otherPlayerName = Utils.StripSignature(otherPlayer.NickName);
			if (!SignatureMismachNames.Contains(otherPlayerName))
			{
				SetAnnouncement(String.Format("{0} has disconnected", otherPlayerName), 3);
			}
		}
	}

	public void OnTeamChanged(GameObject obj, int teamIndex)
	{
		if (PhotonNetwork.IsMasterClient && IsInState(GameState.Game) && !obj.GetComponent<PlayerController>().IsBot)
		{
			UpdateTeams();
			UpdateBots();
		}
	}


	public void OnPlayerCreated(GameObject obj)
	{
		if (PhotonNetwork.IsMasterClient && IsInState(GameState.Game) && !obj.GetComponent<PlayerController>().IsBot)
		{
			UpdateTeams();
			UpdateBots();
		}
	}

	public void OnPlayerDestroyed(GameObject obj)
	{
		if (PhotonNetwork.IsMasterClient && IsInState(GameState.Game) && !obj.GetComponent<PlayerController>().IsBot && obj.GetComponent<PlayerController>().IsRemote)
		{
			UpdateTeams();
			EqualizeTeams();
			UpdateBots();
			ResetDisc(true);
		}
	}

	void UpdateCatchRadius()
	{
		List<AimController> aimControllers = new List<AimController>();
		foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
		{
			aimControllers.Add(obj.GetComponent<AimController>());
		}

		List<DiscController> discs = new List<DiscController>();
		foreach (GameObject disc in GameObject.FindGameObjectsWithTag("Disc"))
		{
			discs.Add(disc.GetComponent<DiscController>());
		}

		foreach (AimController ac in aimControllers)
		{
			float radius = DiscController.MaxCatchRadius;

			foreach (DiscController disc in discs)
			{
				float curRadius = disc.CalculateCatchRadius(ac.transform.position);
				radius = Mathf.Min(radius, curRadius);
			}

			if (ac.gameObject.GetComponent<PlayerController>().IsLayingOut)
			{
				radius *= RPGStats.Stats.DiscCatchRadiusLayoutScaler;
			}

			ac.CatchRadius = radius;
		}
	}

	void UpdatePlayerProximity()
	{
		List<AimController> aimControllers = new List<AimController>();
		foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
		{
			obj.GetComponent<AimController>().UpdateOpponentsInRange();
		}
	}
}
