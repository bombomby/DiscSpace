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

		Application.targetFrameRate = 60;

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
