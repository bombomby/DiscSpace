using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InOutAnimationController : MonoBehaviour
{
	public Animator AnimationController;

	public string EnableTrigger;
	public string DisableTrigger;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void OnEnable()
	{
		AnimationController.SetTrigger(EnableTrigger);
	}
}
