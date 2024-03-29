﻿using DuloGames.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
	public static MenuManager Instance;

	public float MenuScale = 1.0f;

	public bool HasOpenWindow(UIWindow.Interaction interaction)
	{
		foreach (UIWindow window in UIWindow.GetWindows())
		{
			if (window.IsOpen && (interaction & window.interaction) != 0)
				return true;
		}
		return false;
	}

	private void Awake()
	{
		Instance = this;
	}

	// Start is called before the first frame update
	void Start()
	{

	}

	DateTime LastNavigationTime = new DateTime();
	const float NavigationRepeatDelay = 0.25f;

	public StandaloneInputModule InputModule;

	private IEnumerator RestartInputModule(float waitTime)
	{
		InputModule.enabled = false;
		yield return new WaitForSeconds(waitTime);
		InputModule.enabled = true;
	}

	Vector2 prevAxis = new Vector2();

	void UpdateNavigation()
	{
		if ((DateTime.Now - LastNavigationTime).TotalSeconds < NavigationRepeatDelay)
			return;

		EventSystem system = EventSystem.current;

		GameObject currSelection = system.currentSelectedGameObject;
		if (currSelection != null)
		{
			//InputField input = currSelection.GetComponent<InputField>();
			//if (input != null)
			{
				Vector2 currAxis = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

				//if (currAxis.magnitude < Mathf.Epsilon && prevAxis.magnitude > 0.5f)
				{
					Vector2 selectionDir = currAxis;

					if (selectionDir.magnitude > Mathf.Epsilon)
					{
						Selectable selectable = currSelection.GetComponent<Selectable>();
						Selectable next = null;

						if (Mathf.Abs(selectionDir.x) > Mathf.Abs(selectionDir.y))
						{
							next = selectionDir.x < 0.0f ? selectable.FindSelectableOnLeft() : selectable.FindSelectableOnRight();
						}
						else
						{
							next = selectionDir.y < 0.0f ? selectable.FindSelectableOnDown() : selectable.FindSelectableOnUp();
						}

						if (next != null)
						{
							LastNavigationTime = DateTime.Now;
							next.Select();
						}
					}
				}

				prevAxis = currAxis;
			}
		}
	}

	DateTime LastOpened;
	const float MenuReopenDelay = 0.5f;

	// Update is called once per frame

	void CheckEmojiMenu(UIWindowID windowID, string buttonName, string axisName)
	{
		// VS TODO: New Input System
		//if (/*(GameSettings.UseNewInputSystem ? GameSettings.Controls.UI.OpenEmoji.triggered :*/ Input.GetButtonDown("Emoji Menu")))
		if ((buttonName != null && Input.GetButtonDown(buttonName)) || (axisName != null && Math.Abs(Input.GetAxis(axisName)) > 0.5f))
		{
			if (!HasOpenWindow(UIWindow.Interaction.Mouse))
			{
				UIWindow.GetWindow(windowID).Show();
			}
		}

		if ((buttonName != null && Input.GetButtonUp(buttonName)) || (axisName != null && Math.Abs(Input.GetAxis(axisName)) < 0.5f))
		{
			UIWindow.GetWindow(windowID).GetComponent<EmojiMenuUI>().Close();
		}
	}

	void LateUpdate()
	{
		if (HasOpenWindow(UIWindow.Interaction.Mouse))
		{
#if !UNITY_EDITOR
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
#endif
		}
		else
		{
#if !UNITY_EDITOR
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
#endif
		}

		if (!HasOpenWindow(UIWindow.Interaction.Keyboard))
		{
			if ((DateTime.Now - LastOpened).TotalSeconds > MenuReopenDelay && (GameSettings.UseNewInputSystem ? GameSettings.Controls.UI.OpenMenu.triggered : Input.GetButtonUp("Main Menu")))
			{
				UIWindow.GetWindow(UIWindowID.PauseMenu).Show();
			}

			if ((GameSettings.UseNewInputSystem ? GameSettings.Controls.UI.OpenChat.triggered : Input.GetButtonDown("Chat Menu")) && SettingsMenuUI.Instance.EnableTextChat)
			{
				UIWindow.GetWindow(UIWindowID.TextChat).Show();
			}
		}
		else
		{
			LastOpened = DateTime.Now;
		}

		CheckEmojiMenu(UIWindowID.EmojiMenu0, "Emoji Menu 0", null);
		CheckEmojiMenu(UIWindowID.EmojiMenu1, "Emoji Menu 1", /*"DPad Y"*/ null);
		//CheckEmojiMenu(UIWindowID.EmojiMenu1, "Emoji Menu 2", "DPad X");

		// VS TODO: UpdateNavigation
		if (!GameSettings.UseNewInputSystem)
			UpdateNavigation();
	}
}
