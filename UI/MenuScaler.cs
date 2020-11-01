using DuloGames.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuScaler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		float scale = MenuManager.Instance.MenuScale;
		RectTransform rectTransform = transform as RectTransform;
		rectTransform.localScale = rectTransform.localScale * scale;

		GetComponent<UIWindow>().onTransitionBegin.AddListener(OnTransition);
	}

	private void OnDestroy()
	{
		GetComponent<UIWindow>().onTransitionBegin.RemoveListener(OnTransition);
	}

	private void OnTransition(UIWindow window, UIWindow.VisualState state, bool instant)
	{
		if (!window.IsOpen)
		{
			Selectable selectable = FindFirstSelectable(transform);
			selectable?.Select();
		}
	}

	Selectable FindFirstSelectable(Transform parent)
	{
		foreach (Transform child in parent)
		{
			if (child.gameObject.activeSelf)
			{
				Selectable selectable = child.GetComponent<Selectable>();
				if (selectable == null)
					selectable = FindFirstSelectable(child);

				if (selectable != null)
					return selectable;
			}
		}
		return null;
	}
}
