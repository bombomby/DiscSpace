using System;
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


	public class FreeCamera
	{


	}




	// Start is called before the first frame update
	void Start()
	{
	}

	enum CameraMode
	{
		GameCam,
		FreeCam,
	}

	public static bool IsFreeCamEnabled
	{
		get { return Camera.main.GetComponent<CameraController>().Mode == CameraMode.FreeCam; }
	}

	public static bool IsCinematicModeEnabled
	{
		get { return Camera.main.GetComponent<CameraController>().IsCinematic; }
	}

	public bool IsCinematic;
	CameraMode Mode = CameraMode.GameCam;

	[Serializable]
	public class FreeCamSettings
	{
		public float TurnSpeed = 180.0f;
		public float MoveSpeed = 0.5f;
		public float FloatSpeed = 0.2f;
	}

	public FreeCamSettings FreeCam = new FreeCamSettings();

	void UpdateFreeCamera()
	{
		float h = Input.GetAxis("Horizontal") + Input.GetAxis("Horizontal Movement");
		float v = Input.GetAxis("Vertical") + Input.GetAxis("Vertical Movement");
		Vector3 movement = transform.rotation * new Vector3(h, 0.0f, v) * FreeCam.MoveSpeed;
		Vector3 altitude = new Vector3(0.0f, Input.GetAxis("Disc Charge Trigger") * FreeCam.FloatSpeed, 0.0f);
		transform.position = transform.position + new Vector3(movement.x, 0.0f, movement.z) + altitude;

		float y = Input.GetAxis("Mouse X") * FreeCam.TurnSpeed * Time.deltaTime;
		float x = Input.GetAxis("Mouse Y") * FreeCam.TurnSpeed * Time.deltaTime;
		transform.eulerAngles = transform.eulerAngles + new Vector3(x, y, 0.0f);
	}

	void UpdateGameCamera()
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

	// Update is called once per frame
	void LateUpdate()
    {
#if UNITY_EDITOR
		if (Input.GetKeyDown(KeyCode.F1))
		{
			Mode = (CameraMode)(((int)Mode + 1) % Enum.GetValues(typeof(CameraMode)).Length);
		}

		if (Input.GetKeyDown(KeyCode.F2))
		{
			IsCinematic = !IsCinematic;
		}
#endif

		switch (Mode)
		{
			case CameraMode.FreeCam:
				UpdateFreeCamera();
				break;

			case CameraMode.GameCam:
				UpdateGameCamera();
				break;
		}
	}
}
