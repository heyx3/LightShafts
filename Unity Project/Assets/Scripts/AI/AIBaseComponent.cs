using UnityEngine;


/// <summary>
/// Base class for all AI components. Provides easy references to other
/// AI components that may be attached to this object.
/// </summary>
[RequireComponent(typeof(AIReferencesComponent))]
public class AIBaseComponent : MonoBehaviour
{
	public MovementHandler MyMovement { get { return refs.MyMovement; } }
	public AIPathFollower MyPathFollower { get { return refs.MyPathFollower; } }
	public CheckTargetVisibility TargetVisibility { get { return refs.TargetVisibility; } }

	public Transform MyTransform { get { return refs.MyTransform; } }
	public PlayerInput Player { get { return PlayerInput.Instance; } }


	private AIReferencesComponent refs;


	void Awake()
	{
		refs = GetComponent<AIReferencesComponent>();
		OnAwake();
	}


	/// <summary>
	/// Does initialization for this component on Awake,
	/// after all references to other AI components are cached.
	/// </summary>
	protected virtual void OnAwake() { }
}