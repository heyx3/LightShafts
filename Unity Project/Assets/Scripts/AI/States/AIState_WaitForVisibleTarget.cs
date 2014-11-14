using UnityEngine;


/// <summary>
/// An AI that sits around until a specific target becomes visible.
/// </summary>
public class AIState_WaitForVisibleTarget : AIBaseComponent
{
	/// <summary>
	/// The object to wait for.
	/// </summary>
	public MovementHandler Target;
	/// <summary>
	/// The state to switch to when the target becomes visible.
	/// The "ReactionState" is automatically disabled when this state becomes enabled.
	/// </summary>
	public AIBaseComponent ReactionState;


	protected override void OnAwake()
	{
		if (Target == null)
		{
			Debug.LogError("AI character '" + gameObject.name + "'s 'AIState_WaitForVisibleTarget' " +
						     "component doesn't have a 'Target' to wait for!");
		}
		if (ReactionState == null)
		{
			Debug.LogError("AI character '" + gameObject.name + "'s 'AIState_WaitForVisibleTarget' " +
						     "component doesn't have a 'ReactionState' to switch to!");
		}
	}

	void Start()
	{
		ReactionState.enabled = false;
		if (MyPathFollower != null)
		{
			MyPathFollower.Target = null;
		}

		TargetVisibility.Target = Target;
	}
	void Update()
	{
		if (TargetVisibility.IsVisible)
		{
			enabled = false;
			ReactionState.enabled = true;
		}
	}
}