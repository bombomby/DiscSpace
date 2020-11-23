using DuloGames.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsMenuUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void OnCloseButtonClicked()
	{
		GetComponent<UIWindow>().Hide();
	}

	public void OnEmailButtonClicked()
	{
		Utils.OpenURL("mailto:discspacegame@gmail.com");
	}

	public void OnFacebookButtonClicked()
	{
		Utils.OpenURL("https://www.facebook.com/groups/1105909683199769");
	}
}
