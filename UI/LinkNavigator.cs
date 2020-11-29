using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkNavigator : MonoBehaviour
{
	public string URL;

	public void OnClick()
	{
		if (!string.IsNullOrEmpty(URL))
			Utils.OpenURL(URL);
	}
}