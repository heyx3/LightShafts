using UnityEngine;


/// <summary>
/// Represents the player's flashlight. Should NOT be parented to anything.
/// </summary>
[RequireComponent(typeof(LightSource))]
public class Flashlight : MonoBehaviour
{
	public Transform MyTransform { get; private set; }
	public LightSource MyLightSource { get; private set; }


	public PlayerInput Player = null;


	void Awake()
	{
		MyTransform = transform;
		MyLightSource = GetComponent<LightSource>();

		if (Player == null)
			Debug.LogError("The flashlight's 'Player' field hasn't been set!");
	}
	void Start()
	{
		MyTransform.position = Player.MyTransform.position;
	}
	void OnEnable()
	{
		Start();
	}
	void Update()
	{
		//Move to player's position.
		MyTransform.position = Player.MyTransform.position;
	}
}