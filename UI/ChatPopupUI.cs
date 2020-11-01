using DuloGames.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChatPopupUI : MonoBehaviour
{
	UIWindow Window;
	public TMP_Text TextArea;

    // Start is called before the first frame update
    void Awake()
    {
		Window = GetComponent<UIWindow>();
    }

	float HighlightTimeOut = 0.0f;

	public void Highlight(string message, float timeout)
	{
		HighlightTimeOut = timeout;
		TextArea.text = message;

		if (!Window.IsOpen)
		{
			Window.Show();
		}

	}

	void Update()
	{
		if (Window.IsOpen && HighlightTimeOut > 0.0f)
		{
			HighlightTimeOut -= Time.deltaTime;
			if (HighlightTimeOut < 0.0f)
			{
				Window.Hide();
			}
		}
	}
}
