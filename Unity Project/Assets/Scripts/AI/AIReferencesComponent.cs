using UnityEngine;


/// <summary>
/// Stores references to all AI components that may belong to this object.
/// Simplifies getting references to AI components.
/// </summary>
public class AIReferencesComponent : MonoBehaviour
{
	public CheckTargetVisibility TargetVisibility { get; private set; }
	public MovementHandler MyMovement { get; private set; }

	public Transform MyTransform { get; private set; }
	public Rigidbody2D MyRigidbody { get; private set; }


	void Awake()
	{
		TargetVisibility = GetComponent<CheckTargetVisibility>();
		MyMovement = GetComponent<MovementHandler>();

		MyTransform = transform;
		MyRigidbody = rigidbody2D;
	}
}