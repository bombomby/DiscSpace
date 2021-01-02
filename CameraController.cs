using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CameraController : MonoBehaviour
{
	PlayerInput Controls;

	public bool RotateCameraX;
	public bool RotateCameraY;

	public float CameraTurnSpeed = 360.0f;
	public float CameraMinTurnAngle = 10.0f;
	public float CameraMaxTurnAngle = 45.0f;

	public float TargetDistance = 10.0f;
	public float TargetOffsetY = 2.0f;

	public GameObject Target;

	public UnityEvent CameraChanged;

	Quaternion InitialRotation;

	public class FreeCamera
	{
	}

	private void Awake()
	{
		Controls = new PlayerInput();
		InitialRotation = transform.rotation;
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

	public delegate void CinenematicModeChangedHandler(bool isCinematic);
	public CinenematicModeChangedHandler CinenematicModeChangedEvent;

	public static event CinenematicModeChangedHandler CinematicModeChanged
	{
		add
		{
			if (Camera.main != null)
			{
				CameraController controller = Camera.main.GetComponent<CameraController>();
				if (controller != null)
				{
					controller.CinenematicModeChangedEvent += value;
				}
			}
		}
		remove
		{
			if (Camera.main != null)
			{
				CameraController controller = Camera.main.GetComponent<CameraController>();
				if (controller != null)
				{
					controller.CinenematicModeChangedEvent -= value;
				}
			}
		}
	}

	bool IsCinematicModeEnabled = false;

	public bool IsCinematic
	{
		get { return IsCinematicModeEnabled; }
		set
		{
			if (IsCinematicModeEnabled != value)
			{
				IsCinematicModeEnabled = value;
				CinenematicModeChangedEvent?.Invoke(value);
			}
		}
	}

	CameraMode Mode = CameraMode.GameCam;

	[Serializable]
	public class FreeCamSettings
	{
		public float TurnSpeed = 180.0f;
		public float MoveSpeed = 0.5f;
		public float FloatSpeed = 0.2f;
	}

	public FreeCamSettings FreeCam = new FreeCamSettings();

	Vector2 GetCameraInput()
	{
		if (GameSettings.UseNewInputSystem)
		{
			return Controls.Camera.Turn.ReadValue<Vector2>();
		}
		else
		{
			float y = Input.GetAxis("Pad X") + Input.GetAxis("Mouse X");
			float x = Input.GetAxis("Pad Y") - Input.GetAxis("Mouse Y");
			return new Vector2(x, y);
		}
	}

	void UpdateFreeCamera()
	{
		float turnSpeed = FreeCam.TurnSpeed * SettingsMenuUI.Instance.CameraSpeed;
		float moveSpeed = FreeCam.MoveSpeed * SettingsMenuUI.Instance.FreeCameraSpeed;
		float floatSpeed = FreeCam.FloatSpeed * SettingsMenuUI.Instance.FreeCameraSpeed;

		float h = Input.GetAxis("Horizontal") + Input.GetAxis("Horizontal Movement");
		float v = Input.GetAxis("Vertical") + Input.GetAxis("Vertical Movement");
		Vector3 movement = transform.rotation * new Vector3(h, 0.0f, v) * moveSpeed;
		Vector3 altitude = new Vector3(0.0f, (Input.GetAxis("Disc Charge Trigger") + Input.GetAxis("Disc Charge")) * floatSpeed, 0.0f);

		transform.position = transform.position + new Vector3(movement.x, 0.0f, movement.z) + altitude;

		Vector3 camInput = GetCameraInput();
		camInput *= turnSpeed * Time.deltaTime;

		transform.eulerAngles = transform.eulerAngles + new Vector3(camInput.x, camInput.y, 0.0f);
	}

	void UpdateGameCamera()
	{
		if (Target == null)
			return;

		Vector2 camInput = GetCameraInput();

		if (FrisbeeGame.Instance.CanProcessMouse)
		{
			float speed = CameraTurnSpeed * SettingsMenuUI.Instance.CameraSpeed;
			camInput *= speed * Time.deltaTime;
		}

		float angleX = RotateCameraX ? Mathf.Clamp(transform.eulerAngles.x + camInput.x, CameraMinTurnAngle, CameraMaxTurnAngle) : transform.eulerAngles.x;
		float angleY = RotateCameraY ? transform.eulerAngles.y + camInput.y : transform.eulerAngles.y;
		float angleZ = transform.eulerAngles.z;

		transform.eulerAngles = new Vector3(angleX, angleY, angleZ);
		transform.position = Target.transform.position - (transform.forward * TargetDistance) + new Vector3(0, TargetOffsetY, 0);

		CameraChanged?.Invoke();
	}

	// Update is called once per frame
	void LateUpdate()
    {
		if (GameSettings.UseNewInputSystem ? GameSettings.Controls.Camera.SwitchCameraMode.triggered : Input.GetKeyDown(KeyCode.F1))
		{
			Mode = (CameraMode)(((int)Mode + 1) % Enum.GetValues(typeof(CameraMode)).Length);
			if (Mode == CameraMode.GameCam)
			{
				transform.rotation = InitialRotation;
			}
		}

		if (GameSettings.UseNewInputSystem ? GameSettings.Controls.Camera.ToggleCinematic.triggered : Input.GetKeyDown(KeyCode.F2))
		{
			IsCinematic = !IsCinematic;
		}

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
