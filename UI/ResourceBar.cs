using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceBar : MonoBehaviour
{
	private RectTransform fullTransform;
	private RectTransform currTransform;

	public Color FullColor;
	public Color EmptyColor;

	public Text ValueText;

	private float ratio = 1.0f;
	private float Ratio
	{
		set
		{
			ratio = Mathf.Clamp(value, 0.0f, 1.0f);

			if (currTransform != null && fullTransform != null)
			{
				currTransform.sizeDelta = new Vector2(fullTransform.sizeDelta.x * ratio, currTransform.sizeDelta.y);

				if (EmptyColor.a > Mathf.Epsilon && FullColor.a > Mathf.Epsilon)
				{
					currTransform.gameObject.GetComponent<Image>().color = Color.Lerp(EmptyColor, FullColor, ratio);
				}
			}

			ValueText.text = string.Format("{0}/{1}", CurValue.ToString("F0"), MaxValue.ToString("F0"));
		}
	}

	float maxValue = 1.0f;
	float curValue = 1.0f;
	public float MaxValue
	{
		get
		{
			return maxValue;
		}
		set
		{
			if (maxValue != value)
			{
				maxValue = value;
				Ratio = CurValue / MaxValue;
			}
		}
	}
	public float CurValue
	{
		get
		{
			return curValue;
		}
		set
		{
			if (curValue != value)
			{
				curValue = Mathf.Clamp(value, 0.0f, MaxValue);
				Ratio = CurValue / MaxValue;
			}
		}
	}

	// Start is called before the first frame update
	void Start()
    {
		fullTransform = transform.Find("Background") as RectTransform;
		currTransform = transform.Find("Foreground") as RectTransform;

		if (FullColor.a > Mathf.Epsilon)
		{
			currTransform.gameObject.GetComponent<Image>().color = FullColor;
		}
	}

	// Update is called once per frame
	void Update()
    {
        if (GameSettings.UseNewInputSystem ? GameSettings.Controls.UI.ShowStats.triggered : (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
		{
			ValueText.enabled = true;
		}
		else
		{
			ValueText.enabled = false;
		}
    }
}
