using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimRandomStart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		var animator = GetComponentInChildren<Animator>();
		animator.Update(Random.value);
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
