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
	void Update()
	{
		//Sanity check.
		if (MyTransform.parent != null)
			Debug.LogError("The player's flashlight can't be parented to anything!");

		//Move to player's position.
		MyTransform.position = Player.MyTransform.position;
	}
	void LateUpdate()
	{
		//Get delta rotation and incorporate that into the light source angle.

		float rotZ = MyTransform.localEulerAngles.z;
		MyTransform.localEulerAngles = new Vector3();

		rotZ = AngleCalculations.TransformEulerAngleToRadian(rotZ);
		MyLightSource.LightAngle += rotZ;
	}
}