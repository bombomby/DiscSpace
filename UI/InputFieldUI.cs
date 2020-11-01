using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputFieldUI : MonoBehaviour
{
	InputField TextInput;

	private void Awake()
	{
		TextInput = GetComponent<InputField>();
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

	bool wasFocused;

    // Update is called once per frame
    void Update()
    {
		//if (enabled)
		//{
		//	if (wasFocused != TextInput.isFocused)
		//	{
		//		if (TextInput.isFocused && !TouchScreenKeyboard.visible)
		//			TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, false, false, TextInput.placeholder.GetComponent<Text>().text);
		//	}

		//	if (Input.GetButtonDown("OKB"))
		//	{
		//		TouchScreenKeyboard.Open("test", TouchScreenKeyboardType.Default);
		//	}
		//}
	}
}
