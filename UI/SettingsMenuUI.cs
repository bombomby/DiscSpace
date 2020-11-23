using DuloGames.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenuUI : MonoBehaviour
{
	public static SettingsMenuUI Instance;

	public Slider CameraSpeedSlider;
	public Text CameraSpeedText;

	public Slider GraphicsQualitySlider;
	public Text GraphicsQualityText;

	public Slider MusicVolumeSlider;
	public Text MusicVolumeText;

	public Slider EffectsVolumeSlider;
	public Text EffectsVolumeText;

	public Toggle TextChatToggle;
	public Toggle ShowServerPinToggle;

	public AudioMixer Mixer;

	public float CameraSpeed
	{
		get
		{
			return CameraSpeedSlider.value;
		}

		set
		{
			CameraSpeedSlider.value = value;
			OnCameraSpeedChanged(value);
		}
	}

	public bool EnableTextChat
	{
		get
		{
			return TextChatToggle.isOn;
		}
		set
		{
			OnTextChatChanged(value);
		}
	}

	public bool ShowServerPin
	{
		get
		{
			return ShowServerPinToggle.isOn;
		}
		set
		{
			OnShowServerPinChanged(value);
		}
	}

	bool SaveSettings = false;

	public float MusicVolume
	{
		get
		{
			return MusicVolumeSlider.value;
		}
		set
		{
			Mixer.SetFloat("MusicVolume", Mathf.Lerp(-80.0f, 0.0f, value));
			MusicVolumeSlider.value = value;
			OnMusicVolumeChanged(value);
		}
	}
	public float EffectsVolume
	{
		get { return EffectsVolumeSlider.value; }
		set
		{
			Mixer.SetFloat("EffectsVolume", Mathf.Lerp(-80.0f, 0.0f, value));
			EffectsVolumeSlider.value = value;
			OnEffectsVolumeChanged(value);
		}
	}

	public void OnCameraSpeedChanged(float value)
	{
		CameraSpeedText.text = value.ToString("F1");

		if (SaveSettings)
		{
			PlayerPrefs.SetFloat(CameraSpeedVar, value);
			PlayerPrefs.Save();
		}
	}

	public void OnMusicVolumeChanged(float value)
	{
		MusicVolumeText.text = value.ToString("F1");

		if (SaveSettings)
		{
			PlayerPrefs.SetFloat(MusicVolumeVar, value);
			PlayerPrefs.Save();
		}
	}

	public void OnEffectsVolumeChanged(float value)
	{
		EffectsVolumeText.text = value.ToString("F1");

		if (SaveSettings)
		{
			PlayerPrefs.SetFloat(EffectsVolumeVar, value);
			PlayerPrefs.Save();
		}
	}

	public void OnTextChatChanged(bool isEnabled)
	{
		if (SaveSettings)
		{
			PlayerPrefs.SetInt(TextChatVar, isEnabled ? 1 : 0);
			PlayerPrefs.Save();
		}
	}

	public void OnShowServerPinChanged(bool isEnabled)
	{
		if (SaveSettings)
		{
			PlayerPrefs.SetInt(ShowServerPinVar, isEnabled ? 1 : 0);
			PlayerPrefs.Save();
			ShowServerPinChanged?.Invoke(isEnabled);
		}
	}

	public delegate void ShowServerPinChangedEvent(bool isEnabled);
	public event ShowServerPinChangedEvent ShowServerPinChanged;

	public void OnQualitySettingsChanged(float value)
	{
		int maxQuality = QualitySettings.names.Length - 1;
		int index = Mathf.Clamp(Mathf.RoundToInt(value), 0, maxQuality);

		GraphicsQualityText.text = QualitySettings.names[index];
		QualitySettings.SetQualityLevel(index, true);

		if (SaveSettings)
		{
			PlayerPrefs.SetInt(GraphicsQuality, index);
			PlayerPrefs.Save();
		}
	}

	private void Awake()
	{
		Instance = this;
	}


	const string CameraSpeedVar = "CameraSpeed";
	const string MusicVolumeVar = "MusicVolume";
	const string EffectsVolumeVar = "EffectsVolume";
	const string GraphicsQuality = "GraphicsQuality";
	const string TextChatVar = "TextChat";
	const string ShowServerPinVar = "ShowServerPin";

	// Start is called before the first frame update
	void Start()
    {
		SaveSettings = false;

		CameraSpeed = PlayerPrefs.GetFloat(CameraSpeedVar, 1.0f);
		MusicVolume = PlayerPrefs.GetFloat(MusicVolumeVar, 0.5f);
		EffectsVolume = PlayerPrefs.GetFloat(EffectsVolumeVar, 0.6f);

		int maxQuality = QualitySettings.names.Length - 1;
		int quality = PlayerPrefs.GetInt(GraphicsQuality, maxQuality);
		GraphicsQualitySlider.maxValue = maxQuality;
		OnQualitySettingsChanged(quality);

		//QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 60;

		ShowServerPin = PlayerPrefs.GetInt(ShowServerPinVar, 1) == 1;
		EnableTextChat = PlayerPrefs.GetInt(TextChatVar, 1) == 1;

		SaveSettings = true;
	}

    // Update is called once per frame
    void Update()
    {
        
    }

	public void OnCloseButtonClick()
	{
		GetComponent<UIWindow>().Hide();
	}
}
