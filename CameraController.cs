using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CameraController : MonoBehaviour
{
	public bool RotateCameraX;
	public bool RotateCameraY;

	public float CameraTurnSpeed = 360.0f;
	public float CameraMinTurnAngle = 10.0f;
	public float CameraMaxTurnAngle = 45.0f;

	public float TargetDistance = 10.0f;
	public float TargetOffsetY = 2.0f;

	public GameObject Target;

	public UnityEvent CameraChanged;

	// Start is called before the first frame update
	void Start()
    {
	}

	// Update is called once per frame
	void LateUpdate()
    {
		if (Target == null)
			return;

		float y = 0.0f;
		float x = 0.0f;

		if (FrisbeeGame.Instance.CanProcessMouse)
		{
			float speed = CameraTurnSpeed * SettingsMenuUI.Instance.CameraSpeed;

			y = Input.GetAxis("Mouse X") * speed * Time.deltaTime;
			x = -Input.GetAxis("Mouse Y") * speed * Time.deltaTime;
		}

		float angleX = RotateCameraX ? Mathf.Clamp(transform.eulerAngles.x + x, CameraMinTurnAngle, CameraMaxTurnAngle) : transform.eulerAngles.x;
		float angleY = RotateCameraY ? transform.eulerAngles.y + y : transform.eulerAngles.y;
		float angleZ = transform.eulerAngles.z;

		transform.eulerAngles = new Vector3(angleX, angleY, angleZ);
		transform.position = Target.transform.position - (transform.forward * TargetDistance) + new Vector3(0, TargetOffsetY, 0);

		CameraChanged?.Invoke();
	}
}
