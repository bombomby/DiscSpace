using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardSelector : MonoBehaviour
{
	public GameObject LockedObject;
	public GameObject UnlockedObject;

	public GameReward Reward;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		bool isUnlocked = Reward.IsUnlocked;
		if (UnlockedObject != null)
			UnlockedObject.SetActive(isUnlocked);

		if (LockedObject != null)
			LockedObject.SetActive(!isUnlocked);
    }
}
