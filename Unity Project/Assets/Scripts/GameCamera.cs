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

	/// <summary>
	/// Gets and sets the resolution of the game.
	/// If this was built with a Pro license, it resizes the game/lighting render textures.
	/// If this was built without a Pro license, it resizes the screen itself.
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


	public bool RotateWithPlayer = false;


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
	}
	void LateUpdate()
	{
		if (!RotateWithPlayer)
			MyTransform.eulerAngles = new Vector3();
	}
}