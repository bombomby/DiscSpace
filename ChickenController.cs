using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChickenController : MonoBehaviour
{
	Animator Anim;

    // Start is called before the first frame update
    void Start()
    {
		Anim = GetComponent<Animator>();
    }

	[System.Serializable]
	public class State
	{
		public string Name;
		public float DurationMin = 1.0f;
		public float DurationMax = 2.0f;
	}

	public State[] StateMachine;

	State CurrentState;
	float CurrentStateTimeLeft;

    // Update is called once per frame
    void Update()
    {
		if (CurrentStateTimeLeft < Mathf.Epsilon)
		{
			if (CurrentState != null && !string.IsNullOrEmpty(CurrentState.Name))
				Anim.SetBool(CurrentState.Name, false);

			CurrentState = StateMachine[Random.Range(0, StateMachine.Length)];
			CurrentStateTimeLeft = Random.Range(CurrentState.DurationMin, CurrentState.DurationMax);

			if (!string.IsNullOrEmpty(CurrentState.Name))
				Anim.SetBool(CurrentState.Name, true);
		}
		else
		{
			CurrentStateTimeLeft -= Time.deltaTime;
		}
    }
}
