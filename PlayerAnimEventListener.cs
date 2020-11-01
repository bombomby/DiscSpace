using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimEventListener : MonoBehaviour
{
	public delegate void EnableMovementChanged(bool enabled);
	public event EnableMovementChanged EnableMovement;

	public void OnEnableMovement(AnimationEvent animationEvent)
	{
		int enabled = animationEvent.intParameter;
		Debug.Log("EnableMovement: " + enabled);
		EnableMovement?.Invoke(enabled > 0 ? true : false);
	}
}
