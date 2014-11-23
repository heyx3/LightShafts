using System;
using UnityEngine;


/// <summary>
/// Provides a static reference to the game camera.
/// </summary>
[RequireComponent(typeof(Camera))]
public class GameCamera : MonoBehaviour
{
	public static GameCamera Instance { get; private set; }
	public static Camera Cam { get { return Instance.MyCam; } }
	public static Transform Trans { get { return Instance.MyTransform; } }


	public Camera MyCam { get; private set; }
	public Transform MyTransform { get; private set; }

	public Vector2 WorldMousePos { get; private set; }
	public Vector2 WorldViewSize { get; private set; }

	private PlayerInput Player { get { return PlayerInput.Instance; } }


	/// <summary>
	/// Gets and sets the resolution of the game render.
	/// If this game was built with a Pro license, it resizes the game/lighting render textures.
	/// If this game was built without a Pro license, it resizes the screen itself.
	/// </summary>
	public Vector2 GameRenderSize
	{
		get
		{
			if (!Application.HasProLicense())
				return new Vector2(Screen.width, Screen.height);
			return new Vector2(MyCam.targetTexture.width, MyCam.targetTexture.height);
		}
		set
		{
			if (!Application.HasProLicense())
			{
				Screen.SetResolution((int)value.x, (int)value.y, Screen.fullScreen);
			}
			else
			{
				MyCam.targetTexture.width = (int)value.x;
				MyCam.targetTexture.height = (int)value.y;

				LightingCam.Cam.targetTexture.width = (int)value.x;
				LightingCam.Cam.targetTexture.height = (int)value.y;
				
				GameRenderQuad.Trans.localScale = new Vector3(value.x, value.y, 1.0f);
				LightRenderQuad.Trans.localScale = new Vector3(value.x, value.y, 1.0f);
			}
		}
	}


	/// <summary>
	/// Gets and sets the orthographic size of the camera.
	/// </summary>
	public float CurrentZoom
	{
		get { return MyCam.orthographicSize; }
		set { MyCam.orthographicSize = value; }
	}

	public float ZoomMin = 80.0f,
				 ZoomMax = 130.0f;
	public float ZoomMaxChangeSpeed = 0.1f;
	public float ZoomMaxPlayerMouseDist = 1500.0f;

	public float CameraSnapDist = 10.0f;
	public Vector2 CameraBorderLerp = new Vector2(0.1f, 0.1f);
	public float Accel = 2000.0f;

	public bool IsDynamic = true;

	public bool RotateWithPlayer = false;


	[System.NonSerialized]public Vector2 Speed = Vector2.zero;


	void Awake()
	{
		MyCam = camera;
		MyTransform = transform;

		if (Instance != null)
			Debug.LogError("There are two 'GameCamera' instances in the scene!");
		Instance = this;
	}
	void Start()
	{
		//If not using Unity pro, disable the nifty lighting effects.
		if (!Application.HasProLicense())
		{
			Destroy(LightingCam.Instance.gameObject);
			Destroy(FinalCam.Instance.gameObject);

			MyCam.cullingMask |= (1 << LayerMask.NameToLayer("Lights"));
			MyCam.targetTexture = null;
		}

		CurrentZoom = (ZoomMax + ZoomMin) * 0.5f;
	}

	void Update()
	{
		Vector2 mouse = new Vector2(Input.mousePosition.x, Input.mousePosition.y),
				mouseLerp = new Vector2(mouse.x / (float)Screen.width,
										mouse.y / (float)Screen.height),
				camViewPixels = GameRenderSize,
				cameraViewSize = new Vector2(2.0f * MyCam.orthographicSize *
												camViewPixels.x / camViewPixels.y,
											 2.0f * MyCam.orthographicSize),
				camPos = new Vector2(MyTransform.position.x, MyTransform.position.y),
				worldMouse = new Vector2(Mathf.Lerp(camPos.x - (cameraViewSize.x * 0.5f),
													camPos.x + (cameraViewSize.x * 0.5f),
													mouseLerp.x),
										 Mathf.Lerp(camPos.y - (cameraViewSize.y * 0.5f),
													camPos.y + (cameraViewSize.y * 0.5f),
													mouseLerp.y));

		WorldMousePos = worldMouse;
		WorldViewSize = cameraViewSize;


		if (IsDynamic) UpdateZoom();
	}
	private void UpdateZoom()
	{
		float targetZoom = CurrentZoom;
		
		float distPlayerToMouse = Vector2.Distance(WorldMousePos, Player.MyTransform.position);

		//Get the target zoom as a function of the distance.
		float lerpDist = Mathf.Clamp(distPlayerToMouse / ZoomMaxPlayerMouseDist, 0.0f, 1.0f);
		targetZoom = Mathf.Lerp(ZoomMin, ZoomMax, lerpDist);

		//Now try to move from current zoom to target zoom.
		if (Mathf.Abs(targetZoom - CurrentZoom) < (ZoomMaxChangeSpeed * Time.deltaTime))
		{
			CurrentZoom = targetZoom;
		}
		else
		{
			CurrentZoom += ZoomMaxChangeSpeed * Time.deltaTime * Mathf.Sign((targetZoom - CurrentZoom));
		}

		CurrentZoom = Mathf.Clamp(CurrentZoom, ZoomMin, ZoomMax);
	}

	void LateUpdate()
	{
		if (!RotateWithPlayer)
		{
			MyTransform.eulerAngles = new Vector3();
		}

		if (!IsDynamic)
		{
			MyTransform.position = new Vector3(Player.MyTransform.position.x,
											   Player.MyTransform.position.y,
											   MyTransform.position.z);
			return;
		}


		//Update the position of the camera based on the player and mouse positions.

		//If the camera is close enough, just snap to the player.
		if (Vector2.SqrMagnitude((Vector2)MyTransform.position - WorldMousePos) <
			(CameraSnapDist * CameraSnapDist))
		{
			MyTransform.position = new Vector3(WorldMousePos.x, WorldMousePos.y, MyTransform.position.z);
			Speed = Vector2.zero;
		}
		//Otherwise, accelerate towards him.
		{
			//Get the player's position in the world view region from 0 to 1.
			Vector2 playerViewPosLerp = (Vector2)MyCam.WorldToScreenPoint(Player.MyTransform.position);
			playerViewPosLerp = new Vector2(playerViewPosLerp.x / GameRenderSize.x,
											playerViewPosLerp.y / GameRenderSize.y);

			Vector2 camRight = (Vector2)MyTransform.right,
					camUp = (Vector2)MyTransform.up;
			Vector2 toTarget = (WorldMousePos - (Vector2)MyTransform.position).normalized;

			


			//Accelerate towards the player.
			Vector2 accel = toTarget * Accel;
			Speed += accel * Time.deltaTime;


			//If the player is too far to the center of the camera view, move the camera closer to him.

			//Split the velocity into the individual components along the right/up vectors of the camera.
			//One or both of these components may be zeroed out
			//    if the camera is heading too far in one direction.
			Vector2 speedAlongRight = camRight * Vector2.Dot(camRight, Speed),
					speedAlongUp = Speed - speedAlongRight;

			if (playerViewPosLerp.x < CameraBorderLerp.x)
			{
				float deltaRight = (CameraBorderLerp.x - playerViewPosLerp.x);
				MyTransform.position -= (Vector3)(camRight * deltaRight * WorldViewSize.x);

				if (Vector2.Dot(speedAlongRight, camRight) > 0.0f)
					speedAlongRight = Vector2.zero;
			}
			else if (playerViewPosLerp.x > (1.0f - CameraBorderLerp.x))
			{
				float deltaLeft = WorldViewSize.x * (playerViewPosLerp.x - (1.0f - CameraBorderLerp.x));
				MyTransform.position -= (Vector3)(-camRight * deltaLeft);

				if (Vector2.Dot(speedAlongRight, camRight) < 0.0f)
					speedAlongRight = Vector2.zero;
			}

			if (playerViewPosLerp.y < CameraBorderLerp.y)
			{
				float deltaUp = WorldViewSize.y * (CameraBorderLerp.y - playerViewPosLerp.y);
				MyTransform.position -= (Vector3)(camUp * deltaUp);

				if (Vector2.Dot(speedAlongUp, camRight) > 0.0f)
					speedAlongUp = Vector2.zero;
			}
			else if (playerViewPosLerp.y > (1.0f - CameraBorderLerp.y))
			{
				float deltaDown = WorldViewSize.x * (playerViewPosLerp.y - (1.0f - CameraBorderLerp.y));
				MyTransform.position -= (Vector3)(-camUp * deltaDown);

				if (Vector2.Dot(speedAlongUp, camRight) < 0.0f)
					speedAlongUp = Vector2.zero;
			}

			//Recombine the speed components.
			Speed = speedAlongRight + speedAlongUp;


			//Limit the speed of this camera.
			float maxSpeed = 4.0f * Vector2.Distance((Vector2)MyTransform.position, WorldMousePos);
			float finalSpeed;
			if (Vector2.SqrMagnitude(Speed) > (maxSpeed * maxSpeed))
				finalSpeed = maxSpeed;
			else finalSpeed = Speed.magnitude;

			//Redirect the velocity to point towards the target.
			Speed = ((Vector2)WorldMousePos - (Vector2)MyTransform.position).normalized * finalSpeed;
		}

		MyTransform.position += (Vector3)Speed * Time.deltaTime;
	}
}