using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveMode : MonoBehaviour
{
	public FrisbeeGame.GameState State;
	public List<GameObject> Activators;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		foreach (GameObject obj in Activators)
		{
			obj.SetActive(FrisbeeGame.IsInState(State));
		}
    }
}
