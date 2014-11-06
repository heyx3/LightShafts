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
	public Vector2 GameRenderSize
	{
		get
		{
			if (MyCam.targetTexture == null)
				return new Vector2(Screen.width, Screen.height);
			return new Vector2(MyCam.targetTexture.width, MyCam.targetTexture.height);
		}
	}
	public Vector2 WorldMousePos { get; private set; }

	void Awake()
	{
		MyCam = camera;
		MyTransform = transform;

		if (Instance != null)
			Debug.LogError("There are two 'GameCamera' instances in the scene!");
		Instance = this;
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
		MyTransform.eulerAngles = new Vector3();
	}
}