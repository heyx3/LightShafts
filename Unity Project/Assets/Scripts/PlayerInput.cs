using UnityEngine;


/// <summary>
/// Handles the player's WASD/Mouse input.
/// </summary>
[RequireComponent(typeof(MovementHandler))]
public class PlayerInput : MonoBehaviour
{
	public static PlayerInput Instance { get; private set; }


	public MovementHandler MyMovement { get; private set; }
	public PlayerAnimation MyAnimations { get; private set; }

	public Transform MyTransform { get { return MyMovement.MyTransform; } }
	public Rigidbody2D MyRigidbody { get { return MyMovement.MyRigidbody; } }

	public Vector2 WorldMousePos { get; private set; }


	public Flashlight Flashlight = null;
	public Transform ArrowPowerLine = null;

	public GameObject ArrowPrefab = null;

	public int AmmoAmount = 5;

	public float SidewaysWalkSpeedScale = 0.85f,
				 BackwardsWalkSpeedScale = 0.65f;

	public float MinArrowSpeed = 100.0f,
				 MaxArrowSpeed = 1000.0f;
	public float MaxArrowArmTime = 2.5f,
				 ArrowReloadTime = 2.5f;
	

	private float previousEulerZ;
	private float arrowArmedTime = 0.0f;
	private float baseArrowGuideScale;


	void Awake()
	{
		MyMovement = GetComponent<MovementHandler>();
		MyAnimations = GetComponent<PlayerAnimation>();

		previousEulerZ = transform.eulerAngles.z;

		if (ArrowPrefab == null)
			Debug.LogError("Player's 'ArrowPrefab' field isn't set!");

		if (Flashlight == null)
			Debug.LogError("Player's 'Flashlight' field isn't set!");

		if (ArrowPowerLine == null)
			Debug.LogError("Player's 'ArrowPowerLine' field isn't set!");
		baseArrowGuideScale = ArrowPowerLine.localScale.x;

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
		//Update movement input.
		Vector2 input = Vector2.zero;
		if (Input.GetKey(KeyCode.W))
			input.y += 1.0f;
		if (Input.GetKey(KeyCode.S))
			input.y -= 1.0f;
		if (Input.GetKey(KeyCode.A))
			input.x -= 1.0f;
		if (Input.GetKey(KeyCode.D))
			input.x += 1.0f;

		//Scale movement input along the sideways/backwards directions based on player settings.
		Vector2 forward = (Vector2)MyTransform.right,
				side = new Vector2(-forward.y, forward.x);
		float dotForward = Vector2.Dot(input, forward);
		Vector2 forwardPart = forward * dotForward,
				sidePart = input - forwardPart;
		input = (dotForward > 0.0f ? forwardPart : (forwardPart * BackwardsWalkSpeedScale)) +
				(sidePart * SidewaysWalkSpeedScale);

		//If the camera is rotating with the player, rotate the input as well.
		if (GameCamera.Instance.RotateWithPlayer)
		{
			Quaternion camRot = Quaternion.AngleAxis(GameCamera.Instance.MyTransform.eulerAngles.z,
													 new Vector3(0.0f, 0.0f, 1.0f));
			MyMovement.MovementInput = ((Vector2)(camRot * (Vector3)input)).normalized;
		}
		else
		{
			MyMovement.MovementInput = input;
		}


		//Update arrow arming.

		//If the player is still reloading, count the timer up to 0.
		if (arrowArmedTime < 0.0f)
		{
			arrowArmedTime += Time.deltaTime;
			if (arrowArmedTime > 0.0f)
				arrowArmedTime = 0.0f;
		}
		//Otherwise, if he's holding down the mouse button, try to prime/continue priming the next shot.
		else if (Input.GetMouseButton(0))
		{
			if (AmmoAmount > 0)
			{
				arrowArmedTime += Time.deltaTime;

				if (arrowArmedTime >= MaxArrowArmTime)
				{
					FireArrow();
				}
			}
		}
		//Otherwise, see if he just fired an arrow.
		else if (AmmoAmount > 0 && arrowArmedTime > 0.0f)
		{
			FireArrow();
		}

		//Update arrow-arming display helper.
		if (arrowArmedTime > 0.0f && AmmoAmount > 0)
		{
			ArrowPowerLine.gameObject.SetActive(true);
			float length = baseArrowGuideScale * Mathf.Lerp(MinArrowSpeed, MaxArrowSpeed,
															arrowArmedTime / MaxArrowArmTime);
			ArrowPowerLine.localScale = new Vector3(length, ArrowPowerLine.localScale.y,
													ArrowPowerLine.localScale.z);
			ArrowPowerLine.localPosition = new Vector3(length * 0.5f,
													   ArrowPowerLine.localPosition.y,
													   ArrowPowerLine.localPosition.z);

			//Update animation while we're at it.
			if (MyMovement.MovementInput.sqrMagnitude > 0.1f)
			{
				MyAnimations.SetSprite(MyAnimations.WalkBow);
			}
			else
			{
				MyAnimations.SetSprite(MyAnimations.IdleBow);
			}
		}
		else
		{
			ArrowPowerLine.gameObject.SetActive(false);

			//Update animation while we're at it.
			if (MyMovement.MovementInput.sqrMagnitude > 0.1f)
			{
				MyAnimations.SetSprite(MyAnimations.Walk);
			}
			else
			{
				MyAnimations.SetSprite(MyAnimations.Idle);
			}
		}


		//Rotate the player based on the mouse position.

		Vector2 dir = GameCamera.Instance.WorldMousePos - (Vector2)MyTransform.position;
		float ang = Mathf.Atan2(dir.y, dir.x);
		float angD = ang * Mathf.Rad2Deg;

		MyTransform.eulerAngles = new Vector3(0.0f, 0.0f, angD);
		Flashlight.MyTransform.eulerAngles += new Vector3(0.0f, 0.0f, angD - previousEulerZ);
		previousEulerZ = angD;
	}


	private void FireArrow()
	{
		AmmoAmount -= 1;

		GameObject arrow = (GameObject)Instantiate(ArrowPrefab);
		Vector3 aimDir = MyTransform.right;

		Transform arrowTr = arrow.transform;
		arrowTr.right = MyTransform.right;

		float arrowStartOffset = MyMovement.MyCollider.radius * 1.001f;
		arrowStartOffset += arrow.GetComponent<SpriteRenderer>().sprite.bounds.extents.x;
		arrowTr.position = MyTransform.position + (aimDir * arrowStartOffset);

		float arrowSpeed = Mathf.Lerp(MinArrowSpeed, MaxArrowSpeed, arrowArmedTime / MaxArrowArmTime);
		arrow.rigidbody2D.velocity = (Vector2)MyTransform.right * arrowSpeed;
			
		arrowArmedTime = -ArrowReloadTime;
	}
}