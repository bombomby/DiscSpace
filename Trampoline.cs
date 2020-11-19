using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trampoline : MonoBehaviour
{
	AudioSource AS;
	public GameObject VFX;

    // Start is called before the first frame update
    void Start()
    {
		AS = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public Vector3 Force;

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag == "Player")
		{
			Rigidbody RB = collision.gameObject.GetComponent<Rigidbody>();
			RB.AddForce(Force, ForceMode.Impulse);
			AS.PlayOneShot(AS.clip);
			VFX.SetActive(true);
		}
	}
}
