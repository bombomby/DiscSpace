using DuloGames.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EmojiMenuUI : MonoBehaviour
{
	public static Vector3 EmojiCameraOffset = new Vector3(0f, 1f, 0f);


	Transform currentSelection;

	Transform Selection
	{
		get { return currentSelection; }
		set
		{
			if (currentSelection != null)
			{
				(currentSelection as RectTransform).localScale = Vector3.one;
			}

			currentSelection = value;

			if (currentSelection != null)
			{
				(currentSelection as RectTransform).localScale = Vector3.one * 1.5f;
			}
		}
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

	public void UpdatePosition()
	{
		GameObject player = FrisbeeGame.Instance.MainPlayer;
		if (player != null)
		{
			Vector3 pos = Camera.main.WorldToScreenPoint(player.transform.position + EmojiCameraOffset);
			transform.position = pos;
		}
	}

	private void Update()
	{
		UIWindow window = GetComponent<UIWindow>();

		if (window.IsOpen)
		{
			List<Transform> emojiList = new List<Transform>();
			foreach (Transform child in transform.Find("RadialLayout"))
				emojiList.Add(child);

			Vector2 dir = new Vector2(Input.GetAxis("Pad X"), -Input.GetAxis("Pad Y"));
			if (dir.magnitude > Mathf.Epsilon)
			{
				var sorted = emojiList.OrderBy(obj => Vector3.Angle(obj.position - window.transform.position, dir));
				Selection = sorted.First();
			}
			else
			{
				Selection = null;
			}
		}
	}

	public void Close()
	{
		if (Selection != null)
		{
			Selection.gameObject.GetComponent<EmojiMenuItem>().OnClick();
			Selection = null;
		}

		GetComponent<UIWindow>().Hide();
	}
}
