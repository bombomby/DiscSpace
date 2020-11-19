using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class InteractionMenu : MonoBehaviour
{
	public GameObject InteractionObject;
	public Text InteractionText;
	private static InteractionMenu instance = null;

	public List<Interactive> ActiveInteractions = new List<Interactive>();

	public static InteractionMenu Instance
	{
		get
		{
			return instance;
		}
	}

	private void Awake()
	{
		if (instance == null)
			instance = this;
	}

	// Start is called before the first frame update
	void Start()
    {
	}

    // Update is called once per frame
    void Update()
    {
        if (ActiveInteractions.Count > 0)
		{
			StringBuilder text = new StringBuilder();
			foreach (Interactive interactive in ActiveInteractions)
			{
				if ((interactive.States & FrisbeeGame.Instance.CurrentState) != 0)
				{
					if (text.Length > 0)
						text.Append("\n");

					string key = string.Empty;

					switch (interactive.Axis)
					{
						case Interactive.Type.ActionStart:
							key = "'F'(keyboard)/'Y'(gamepad)";
							break;

						case Interactive.Type.ActionStop:
							key = "'G'(keyboard)/'B'(gamepad)";
							break;
					}

					text.Append(interactive.Text.Replace("{key}", key));
				}
			}
			InteractionText.text = text.ToString();
			InteractionObject.SetActive(true);
		}
		else
		{
			InteractionObject.SetActive(false);
		}
    }

	public void AddInteraction(Interactive interactive)
	{
		if (!ActiveInteractions.Contains(interactive))
			ActiveInteractions.Add(interactive);
	}

	public void RemoveInteraction(Interactive interactive)
	{
		ActiveInteractions.Remove(interactive);
	}

	public void Clear()
	{
		ActiveInteractions.Clear();
	}
}
