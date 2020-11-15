using DuloGames.UI;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour
{
	public InputField MessageText;
	public GameObject MessageList;
	public ScrollRect MessageScroll;
	public GameObject ItemPrefab;
	public GameObject InputGroup;

	public ChatPopupUI Popup;

	PhotonView PV;
	UIWindow Window;
	
	private void Awake()
	{
		PV = GetComponent<PhotonView>();
		Window = GetComponent<UIWindow>();
	}

	private void SendMessage()
	{
		if (!string.IsNullOrEmpty(MessageText.text))
		{
			PV.RPC("SendMessage", RpcTarget.All, PhotonNetwork.NickName, MessageText.text);
			MessageText.text = string.Empty;
		}
	}

	public void OnSendButtonClicked()
	{
		SendMessage();
	}

	string PostprocessMessage(string message)
	{
		// Replace url with <a>
		message = Regex.Replace(message, 
			@"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)",
			"<link=\"$1\"><color=#6666FF><u>$1</u></color></link>");

		return message;
	}

	string CombineMessage(string name, string message)
	{
		return string.Format("<b>{0}</b>: {1}", name, PostprocessMessage(message));
	}

	[PunRPC]
	void SendMessage(string name, string message)
	{
		string combinedMessage = CombineMessage(name, message);

		GameObject item = Instantiate(ItemPrefab);
		item.transform.SetParent(MessageList.transform, false);
		item.GetComponent<TMP_Text>().text = combinedMessage;
		StartCoroutine(ScrollToBottom());

		if (!Window.IsOpen)
		{
			Popup.Highlight(combinedMessage, 5.0f);
		}
	}

	IEnumerator ScrollToBottom()
	{
		yield return new WaitForEndOfFrame();
		MessageScroll.gameObject.SetActive(true);
		MessageScroll.normalizedPosition = new Vector2(0, 0);
	}

	void Update()
	{
		if (MessageText.text != string.Empty && Input.GetButtonDown("Submit"))
		{
			SendMessage();
			FocusInput();
		}
	}

	public void Show()
	{
		Window.interaction = UIWindow.Interaction.MouseAndKeyboard;
		Window.Show();
	}

	public void FocusInput()
	{
		MessageText.Select();
		MessageText.ActivateInputField();
	}

	public void OnActivateTransition()
	{
		FocusInput();
	}

	public void Close()
	{
		Window.Hide();
	}
}
