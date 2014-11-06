using UnityEngine;


/// <summary>
/// Provides a static reference to the quad that displays the rendered game texture.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GameRenderQuad : MonoBehaviour
{
	public static GameRenderQuad Instance { get; private set; }
	public static Transform Trans { get { return Instance.MyTransform; } }


	public Transform MyTransform { get; private set; }


	void Awake()
	{
		MyTransform = transform;

		if (Instance != null)
			Debug.LogError("There is more than one 'GameRenderQuad' instance");
		Instance = this;
	}
}