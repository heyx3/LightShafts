using UnityEngine;


/// <summary>
/// Checks every so often whether a certain target is within line of sight.
/// </summary>
[RequireComponent(typeof(MovementHandler))]
public class CheckTargetVisibility : AIBaseComponent
{
	public float CheckInterval = 0.6f,
				 IntervalVariation = 0.2f;
	public float MaxVisibleDist = 1000.0f;

	public MovementHandler Target;

	public float DEBUG = 0.0f;


	/// <summary>
	/// Whether the target is currently visible to this entity.
	/// </summary>
	public bool IsVisible { get; private set; }
	/// <summary>
	/// The most recent position that the target was seen at.
	/// </summary>
	public Vector2 LastSeenPos { get; private set; }
	/// <summary>
	/// The path node closest to the most recent position the target was seen at.
	/// </summary>
	public NavNode LastSeenClosestNode { get; private set; }
	/// <summary>
	/// The amount of time since the target was last seen.
	/// </summary>
	public float TimeSinceVisible { get; private set; }


	void Start()
	{
		TimeSinceVisible = 999999.0f;
		IsVisible = false;

		LastSeenPos = Target.MyTransform.position;
		StartCoroutine(CheckVisibleCoroutine());
	}
	void Update()
	{
		if (IsVisible)
		{
			TimeSinceVisible = 0.0f;

			LastSeenPos = Target.MyTransform.position;
			LastSeenClosestNode = Target.ClosestNode.MyNode;
		}
		else
		{
			TimeSinceVisible += Time.deltaTime;
		}

		DEBUG = TimeSinceVisible;
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere((Vector3)LastSeenPos, 100.0f);
	}


	private System.Collections.IEnumerator CheckVisibleCoroutine()
	{
		RaycastHit2D hit = MyMovement.CastRay(((Vector2)Target.MyTransform.position -
											   (Vector2)MyTransform.position).normalized,
											  MaxVisibleDist,
											  MovementHandler.NavBlockerAndCharacterLayerMask);
		IsVisible = (hit.collider != null) &&
					((1 << hit.collider.gameObject.layer) == MovementHandler.CharacterOnlyLayerMask);
		if (IsVisible)
		{
			TimeSinceVisible = 0.0f;
		}

		yield return new WaitForSeconds(Random.Range(CheckInterval - IntervalVariation,
													 CheckInterval + IntervalVariation));
		StartCoroutine(CheckVisibleCoroutine());
	}
}