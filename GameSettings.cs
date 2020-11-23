using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
	static public bool UseNewInputSystem = false;

	private PlayerInput PlayerControls;

	public static PlayerInput Controls
	{
		get { return Instance.PlayerControls; }
	}

	static GameSettings Instance;

    // Start is called before the first frame update
    void Awake()
    {
		Instance = this;
		PlayerControls = new PlayerInput();
	}
}
