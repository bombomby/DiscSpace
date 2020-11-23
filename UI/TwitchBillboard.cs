using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TwitchBillboard : MonoBehaviour
{
	public RawImage ScreenshotSlot;
	public RawImage IconSlot;
	public TMP_Text StatusSlot;
	public TMP_Text NameSlot;
	public TMP_Text ViewersSlot;
	private string ChannelURL;

	public GameObject Overlay;

	WaitForSeconds RefreshInterval = new WaitForSeconds(60.0f);

	//const string TwitchURL = @"https://api.twitch.tv/kraken/streams/?game=Overwatch&limit=1";
	const string TwitchURL = @"https://api.twitch.tv/kraken/streams/?game=Disc%20Space&limit=1";

	//WWW ScreeshotRequest;
	//WWW IconRequest;
	//WWW JSONRequest;

	[Serializable]
	public class TwitchPreview
	{
		public string small;
		public string medium;
		public string large;
	}

	[Serializable]
	public class TwitchChannel
	{
		public string status;
		public string broadcaster_language;
		public string display_name;
		public string logo;
		public string url;
	}


	[Serializable]
	public class TwitchStream
	{
		public int viewers;
		public TwitchPreview preview;
		public TwitchChannel channel;
	}

	[Serializable]
	public class TwitchData
	{
		public TwitchStream[] streams;
	}

	void ApplyTexture(RawImage slot, UnityWebRequest request)
	{
		if (request.result == UnityWebRequest.Result.Success)
		{
			Texture2D downloadedTexture = DownloadHandlerTexture.GetContent(request);

			Texture2D texture = new Texture2D(downloadedTexture.width, downloadedTexture.height, downloadedTexture.format, true);
			texture.LoadImage(request.downloadHandler.data);

			slot.texture = texture;
			slot.gameObject.SetActive(true);
		}
		else
		{
			slot.gameObject.SetActive(false);
			slot.texture = null;
		}
	}

	IEnumerator Refresh()
	{
		using (UnityWebRequest JSONRequest = UnityWebRequest.Get(TwitchURL))
		{
			JSONRequest.SetRequestHeader("Client-ID", "z1bqimfahp7m38719875bnoi6ez4er");
			JSONRequest.SetRequestHeader("Accept", "application/vnd.twitchtv.v5+json");

			yield return JSONRequest.SendWebRequest();

			if (JSONRequest.result == UnityWebRequest.Result.Success)
			{
				TwitchData data = JsonUtility.FromJson<TwitchData>(JSONRequest.downloadHandler.text);
				if (data != null && data.streams.Length >= 1)
				{
					TwitchStream stream = data.streams[0];

					StatusSlot.text = stream.channel.status;
					NameSlot.text = string.Format("@{0}", stream.channel.display_name);
					ViewersSlot.text = stream.viewers >= 1000 ? string.Format("{0:F1}k", stream.viewers / 1000.0f) : stream.viewers.ToString();

					ChannelURL = stream.channel.url;

					using (UnityWebRequest iconRequest = UnityWebRequestTexture.GetTexture(stream.channel.logo))
					{
						yield return iconRequest.SendWebRequest();
						ApplyTexture(IconSlot, iconRequest);
					}

					using (UnityWebRequest screenshotRequest = UnityWebRequestTexture.GetTexture(stream.preview.large))
					{
						yield return screenshotRequest.SendWebRequest();
						ApplyTexture(ScreenshotSlot, screenshotRequest);
					}

					Overlay.SetActive(true);
				}
			}
		}
	}

	// Start is called before the first frame update
	IEnumerator Start()
    {

		while (true)
		{
			yield return Refresh();
			yield return RefreshInterval;
		}
	}

	public void CmdJoinChannel()
	{
		Utils.OpenURL(ChannelURL);
	}
}
