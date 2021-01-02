using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBarsUI : MonoBehaviour
{
	public GameObject HealthBar;
	public GameObject StaminaBar;
	public GameObject ExperienceBar;

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		GameObject player = FrisbeeGame.Instance.MainPlayer;
		if (player != null)
		{
			RPGStats stats = player.GetComponent<RPGStats>();

			ResourceBar health = HealthBar.GetComponent<ResourceBar>();
			health.MaxValue = stats.CurrentStats.MaxHealth;
			health.CurValue = stats.CurrentStats.Health;

			ResourceBar stamina = StaminaBar.GetComponent<ResourceBar>();
			stamina.MaxValue = stats.CurrentStats.MaxStamina;
			stamina.CurValue = stats.CurrentStats.Stamina;

			ResourceBar experience = ExperienceBar.GetComponent<ResourceBar>();
			DBStats dbStats = player.GetComponent<DBStats>();
			experience.MaxValue = dbStats.NextLevelExperience - dbStats.CurrLevelExperience;
			experience.CurValue = dbStats.Experience - dbStats.CurrLevelExperience;
		}
	}
}
