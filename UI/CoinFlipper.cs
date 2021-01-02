using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinFlipper : MonoBehaviour
{
	public float Cooldown = 5.0f;

	float lastFlip = 0.0f;

	public void OnFlip()
	{
		if (Mathf.Abs(Time.time - lastFlip) > Cooldown)
		{
			int value = Random.Range(0, 100);
			string msg = string.Format("{0} flips a coin: {1}", NetworkLobby.CurrentPlayerName, value % 2 == 0 ? "Heads (Win)" : "Tails (Loss)");
			FrisbeeGame.Instance.CmdAnnouncement(msg, 5);
			lastFlip = Time.time;
		}
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
