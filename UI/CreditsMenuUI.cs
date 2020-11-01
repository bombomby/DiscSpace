﻿using DuloGames.UI;
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
		Application.OpenURL("mailto:slyusarev@gmail.com");
	}

	public void OnFacebookButtonClicked()
	{
		Application.OpenURL("https://www.facebook.com/vslyusarev");
	}
}