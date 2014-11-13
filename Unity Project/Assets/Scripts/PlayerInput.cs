using UnityEngine;


/// <summary>
/// Handles the player's WASD/Mouse input.
/// </summary>
[RequireComponent(typeof(MovementHandler))]
public class PlayerInput : MonoBehaviour
{
	public static PlayerInput Instance { get; private set; }


	public MovementHandler MyMovement { get; private set; }
	public Transform MyTransform { get { return MyMovement.MyTransform; } }
	public Rigidbody2D MyRigidbody { get { return MyMovement.MyRigidbody; } }

	public Vector2 WorldMousePos { get; private set; }


	public Flashlight Flashlight = null;
	private float previousEulerZ;


	void Awake()
	{
		MyMovement = GetComponent<MovementHandler>();
		previousEulerZ = transform.eulerAngles.z;

		if (Flashlight == null)
			Debug.LogError("Player's 'Flashlight' field isn't set!");

		if (Instance != null)
			Debug.LogError("More than one 'PlayerInput' component in the scene!");
		Instance = this;
	}
	void Start()
	{
		if (GameCamera.Instance == null)
			Debug.LogError("There is no 'Game Camera' component in the scene!");
	}
	void Update()
	{
		Vector2 input = Vector2.zero;
		if (Input.GetKey(KeyCode.W))
			input.y += 1.0f;
		if (Input.GetKey(KeyCode.S))
			input.y -= 1.0f;
		if (Input.GetKey(KeyCode.A))
			input.x -= 1.0f;
		if (Input.GetKey(KeyCode.D))
			input.x += 1.0f;
		MyMovement.MovementInput = input;


		Vector2 dir = GameCamera.Instance.WorldMousePos - (Vector2)MyTransform.position;
		float ang = Mathf.Atan2(dir.y, dir.x);
		float angD = ang * Mathf.Rad2Deg;

		MyTransform.eulerAngles = new Vector3(0.0f, 0.0f, angD);
		Flashlight.MyTransform.eulerAngles += new Vector3(0.0f, 0.0f, angD - previousEulerZ);
		previousEulerZ = angD;
	}
}