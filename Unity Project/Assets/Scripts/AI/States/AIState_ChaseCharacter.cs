using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// An AI that chases the player around whenever he's in view.
/// </summary>
[RequireComponent(typeof(AIPathFollower))]
public class AIState_ChaseCharacter : AIBaseComponent
{
	/// <summary>
	/// The path node currently closest to this AI character's target.
	/// </summary>
	public NavNode TargetNode { get { return Target.ClosestNode.MyNode; } }


	/// <summary>
	/// The target to chase.
	/// </summary>
	public MovementHandler Target;
	/// <summary>
	/// How long the target has to be out of this entity's view before this entity switches back to idle.
	/// </summary>
	public float TimeUntilForget = 10.0f;
	/// <summary>
	/// The "idle" state to reactivate after this state is done.
	/// The "idle" state is automatically disabled when this instance is enabled.
	/// </summary>
	public AIBaseComponent IdleState;


	protected override void OnAwake()
	{
		//Make sure properties are set up correctly.

		if (Target == null)
		{
			Debug.LogError("AI character '" + gameObject.name + "'s 'AIState_ChaseCharacter' component " +
						     "doesn't have a 'Target' to chase!");
		}
		if (IdleState == null)
		{
			Debug.LogError("AI character '" + gameObject.name + "'s 'AIState_ChaseCharacter' component " +
						     "doesn't have a 'IdleStateComponent' to fall back on!");
		}
	}
	void Start()
	{
		IdleState.enabled = false;
		TargetVisibility.Target = Target;
		MyPathFollower.Target = TargetVisibility.LastSeenClosestNode;
	}

	void Update()
	{
		Vector2 targetPos = (Vector2)Target.MyTransform.position;

		//If the target is in sight, just head towards it.
		if (TargetVisibility.IsVisible)
		{
			MyPathFollower.Target = null;
			MyMovement.MovementInput = targetPos - (Vector2)MyTransform.position;
		}
		//Otherwise, if the target has been gone for too long, go back to idle.
		else if (TargetVisibility.TimeSinceVisible >= TimeUntilForget)
		{
			enabled = false;
			IdleState.enabled = true;
		}
		//Otherwise, path towards the target's last-known position.
		else
		{
			MyPathFollower.Target = TargetVisibility.LastSeenClosestNode;
		}
	}
}