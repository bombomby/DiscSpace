using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LinkOpener : Button
{
	public void OnClick()
	{
		TextMeshProUGUI text = GetComponent<TextMeshProUGUI>();

		int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);
		if (linkIndex != -1)
		{ // was a link clicked?
			TMP_LinkInfo linkInfo = text.textInfo.linkInfo[linkIndex];

			// open the link id as a url, which is the metadata we added in the text field
			Application.OpenURL(linkInfo.GetLinkID());
		}
	}

	protected override void Start()
	{
		onClick.AddListener(OnClick);
	}

	protected override void OnDestroy()
	{
		onClick.RemoveListener(OnClick);
	}
}