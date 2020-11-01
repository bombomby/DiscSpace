using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BilloardText : MonoBehaviour
{
	[Serializable]
	public struct UpScaleSettings
	{
		public bool IsEnabled;
		public float MinDistance;
		public float DiminishingFactor;
	}

	public UpScaleSettings UpScale = new UpScaleSettings();

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
		transform.rotation = Camera.main.transform.rotation;

		if (UpScale.IsEnabled)
		{
			float distanceToCamera = (Camera.main.transform.position - transform.position).magnitude;
			float ratio = distanceToCamera / UpScale.MinDistance;
			float scale = 1.0f + Mathf.Max(0f, (ratio - 1.0f) * UpScale.DiminishingFactor);
			transform.localScale = Vector3.one * scale;
		}
	}
}
