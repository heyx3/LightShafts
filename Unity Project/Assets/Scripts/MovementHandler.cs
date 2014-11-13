using UnityEngine;


/// <summary>
/// Represents an controlled Rigidbody. This component has an acceleration ("Accel"),
/// a directional input ("MovementInput"),
/// and a reference to the closest path node to this object ("ClosestNode").
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class MovementHandler : MonoBehaviour
{
	/// <summary>
	/// Defines the layer used for raycasting.
	/// </summary>
	public static int NavBlockerLayerMask
	{
		get
		{
			return 1 << LayerMask.NameToLayer("Navigation Blocker");
		}
	}


	public float Accel = 1000.0f;

	
	public Transform MyTransform { get; private set; }
	public Rigidbody2D MyRigidbody { get; private set; }

	/// <summary>
	/// Set this value to indicate the direction this entity should accelerate towards.
	/// This vector's magnitude scales the acceleration.
	/// </summary>
	public Vector2 MovementInput { get; set; }
	/// <summary>
	/// The closest navigation node to this entity.
	/// </summary>
	public NavNodeComponent ClosestNode { get; private set; }


	/// <summary>
	/// Returns the result of casting out a collision ray along the given direction,
	/// starting from this entity, over the given distance.
	/// </summary>
	public RaycastHit2D CastRay(Vector2 dir, float distance)
	{
		return Physics2D.Raycast((Vector2)MyTransform.position, dir, distance, NavBlockerLayerMask);
	}
	/// <summary>
	/// Returns the result of casting out a collision ray along the given direction,
	/// starting from this entity.
	/// </summary>
	public RaycastHit2D CastRay(Vector2 dir)
	{
		return Physics2D.Raycast((Vector2)MyTransform.position, dir, 9999999.0f, NavBlockerLayerMask);
	}


	void Awake()
	{
		MyTransform = transform;
		MyRigidbody = rigidbody2D;
	}
	void FixedUpdate()
	{
		//Set the acceleration.
		MyRigidbody.AddForce(MovementInput * Accel);
	}
	void LateUpdate()
	{
		//Calculate the closest visible nav node.

		Vector2 myPos = (Vector2)MyTransform.position;
		float closestDist = System.Single.PositiveInfinity;
		int closestElement = -1;

		for (int i = 0; i < NavNodeComponent.Components.Count; ++i)
		{
			NavNodeComponent comp = NavNodeComponent.Components[i];

			float tempDist = Vector2.SqrMagnitude(comp.MyNode.Pos - myPos);
			if (closestElement < 0 || tempDist < closestDist)
			{
				//Make sure the node is visible.

				Vector2 dir = comp.MyNode.Pos - myPos;
				float dist = dir.magnitude;
				RaycastHit2D hit = CastRay(dir / dist, dist);

				if (hit.collider == null)
				{
					closestDist = tempDist;
					closestElement = i;
				}
			}
		}

		//Assert that there IS a visible node.
		if (closestElement < 0)
		{
			Debug.LogError("No node is within view of object '" + gameObject.name + "'!");
		}
		else
		{
			//Set the closest node.
			ClosestNode = NavNodeComponent.Components[closestElement];
		}
	}

	void OnDrawGizmosSelected()
	{
		if (!Application.isPlaying || ClosestNode == null) return;

		Gizmos.color = Color.white;
		Gizmos.DrawLine(transform.position, ClosestNode.transform.position);
	}
}