using UnityEngine;


/// <summary>
/// The final render camera that combines the light and game renders.
/// Should only ever be one of these in the scene.
/// </summary>
[RequireComponent(typeof(Camera))]
public class FinalCam : MonoBehaviour
{
	public static FinalCam Instance { get; private set; }
	public static Camera Cam { get { return Instance.MyCam; } }

	public Camera MyCam { get; private set; }

	void Awake()
	{
		MyCam = camera;

		if (Instance != null)
			Debug.LogError("There is more than one 'FinalCam' instance!");
		Instance = this;
	}
}