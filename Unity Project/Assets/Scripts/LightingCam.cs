using UnityEngine;


/// <summary>
/// The camera that renders the light objects into a Render Texture.
/// Should only ever be one of these in the scene.
/// </summary>
[RequireComponent(typeof(Camera))]
public class LightingCam : MonoBehaviour
{
	public static LightingCam Instance { get; private set; }
	public static Camera Cam { get { return Instance.MyCam; } }


	public Transform MyTransform { get; private set; }
	public Camera MyCam { get; private set; }


	public Color AmbientLight = new Color(0.1f, 0.1f, 0.1f, 0.01f);


	void Awake()
	{
		MyCam = camera;
		MyTransform = transform;

		if (Instance != null)
			Debug.LogError("There is more than one 'LightingCam' instance!");
		Instance = this;
	}
	void LateUpdate()
	{
		//Update ambient lighting.
		MyCam.backgroundColor = AmbientLight;

		//Exactly copy the game camera's info.
		MyTransform.position = GameCamera.Trans.position;
		MyCam.orthographicSize = GameCamera.Cam.orthographicSize;
		MyCam.nearClipPlane = GameCamera.Cam.nearClipPlane;
		MyCam.farClipPlane = GameCamera.Cam.farClipPlane;
	}
}