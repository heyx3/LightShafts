using UnityEngine;


/// <summary>
/// Represents an controlled Rigidbody. This component has an acceleration ("Accel"),
/// a directional input ("MovementInput"),
/// and a reference to the closest path node to this object ("ClosestNode").
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class MovementHandler : MonoBehaviour
{
	/// <summary>
	/// Defines the default layer used for raycasting -- only navigation blockers.
	/// </summary>
	public static int NavBlockerOnlyLayerMask
	{
		get
		{
			return 1 << LayerMask.NameToLayer("Navigation Blocker");
		}
	}
	/// <summary>
	/// Defines another possible layer used for raycasting -- only characters.
	/// </summary>
	public static int CharacterOnlyLayerMask
	{
		get
		{
			return 1 << LayerMask.NameToLayer("Character");
		}
	}
	/// <summary>
	/// Defines another possible layer used for raycasting -- navigation blockers AND characters.
	/// </summary>
	public static int NavBlockerAndCharacterLayerMask
	{
		get
		{
			return NavBlockerOnlyLayerMask | CharacterOnlyLayerMask;
		}
	}


	public float Accel = 1000.0f;

	
	public Transform MyTransform { get; private set; }
	public Rigidbody2D MyRigidbody { get; private set; }
	public CircleCollider2D MyCollider { get; private set; }

	/// <summary>
	/// Set this value to indicate the direction this entity should accelerate towards.
	/// Clamps the magnitude to the range {0, 1}. This magnitude represents the acceleration scale.
	/// Resets to {0, 0} after it is applied.
	/// </summary>
	public Vector2 MovementInput { get; set; }
	/// <summary>
	/// The closest navigation node to this entity.
	/// </summary>
	public NavNodeComponent ClosestNode { get; private set; }


	/// <summary>
	/// Returns the result of casting out a collision ray along the given direction,
	/// starting from this entity, over the given distance.
	/// If the given layer mask is 0, "NavBlockerOnlyLayerMask" is used.
	/// </summary>
	public RaycastHit2D CastRay(Vector2 dir, float distance, int layerMask = 0)
	{
		//Start the ray juuuuuust in front of this character so that the ray doesn't hit it.
		Vector2 rayStart = (Vector2)MyTransform.position + (dir * MyCollider.radius * 1.001f);
		return Physics2D.Raycast(rayStart, dir, distance,
								 (layerMask == 0 ? NavBlockerOnlyLayerMask : layerMask));
	}
	/// <summary>
	/// Returns the result of casting out a collision ray along the given direction,
	/// starting from this entity.
	/// If the given layer mask is 0, "NavBlockerOnlyLayerMask" is used.
	/// </summary>
	public RaycastHit2D CastRay(Vector2 dir, int layerMask = 0)
	{
		//Start the ray juuuuuust in front of this character so that the ray doesn't hit it.
		Vector2 rayStart = (Vector2)MyTransform.position + (dir * MyCollider.radius * 1.001f);
		return Physics2D.Raycast(rayStart, dir, 9999999.0f,
								 (layerMask == 0 ? NavBlockerOnlyLayerMask : layerMask));
	}


	void Awake()
	{
		MyTransform = transform;
		MyRigidbody = rigidbody2D;
		MyCollider = GetComponent<CircleCollider2D>();
	}
	void FixedUpdate()
	{
		//Set the acceleration.
		MyRigidbody.AddForce(Vector2.ClampMagnitude(MovementInput, 1.0f) * Accel);
		MovementInput = Vector2.zero;
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


		//Set the closest node.
		if (closestElement >= 0)
		{
			ClosestNode = NavNodeComponent.Components[closestElement];
		}
		else
		{
			ClosestNode = null;
		}
	}
}