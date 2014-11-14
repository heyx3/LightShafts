using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Represents a unique NavNode instance.
/// </summary>
public class NavNodeComponent : MonoBehaviour
{
	public static List<NavNodeComponent> Components { get { return components; } }
	private static List<NavNodeComponent> components = new List<NavNodeComponent>();


	public Transform MyTransform { get; private set; }

	public NavNode MyNode { get { return node; } }
	private NavNode node = new NavNode();

	private NavGraph Graph { get { return NavGraphComponent.Graph; } }


	public List<NavNodeComponent> Connections = new List<NavNodeComponent>();
	public bool FindConnectionsAtStartup = true;

	public float ConnectionSearchRadius = 1000.0f;

	public Color GizmoColor = Color.white;
	public float NodeRadius = 10.0f;



	void Awake()
	{
		MyTransform = transform;
		Components.Add(this);

		node.Pos = (Vector2)MyTransform.position;
	}

	void Start()
	{
		Vector2 myPos = (Vector2)MyTransform.position;
		float playerRadius = PlayerInput.Instance.GetComponent<CircleCollider2D>().bounds.extents.x;

		if (FindConnectionsAtStartup)
		{
			//Get any connections that haven't been found yet.
			for (int i = 0; i < Components.Count; ++i)
			{
				if (Components[i] == this || Connections.Contains(Components[i]))
					continue;

				Vector2 rayDir = Components[i].node.Pos - myPos;
				float rayLenSqr = rayDir.sqrMagnitude;

				if (rayLenSqr > (ConnectionSearchRadius * ConnectionSearchRadius))
					continue;

				float rayLen = Mathf.Sqrt(rayLenSqr);
				RaycastHit2D hit = Physics2D.CircleCast(myPos, playerRadius, rayDir / rayLen, rayLen,
														MovementHandler.NavBlockerOnlyLayerMask);

				if (hit.collider == null)
				{
					Connections.Add(Components[i]);
					if (Components[i].FindConnectionsAtStartup && !Components[i].Connections.Contains(this))
					{
						Components[i].Connections.Add(this);
					}
				}
			}
		}

		if (!Graph.ConnectionsFromNode.ContainsKey(node))
		{
			Graph.ConnectionsFromNode.Add(node, Connections);
		}
	}

	void OnDestroy()
	{
		Graph.ConnectionsFromNode.Remove(node);
		Components.Remove(this);
	}

	void LateUpdate()
	{
		//Remove any nodes that have been destroyed.
		for (int i = Connections.Count - 1; i > 0; --i)
			if (Connections[i] == null)
				Connections.RemoveAt(i);

		node.Pos = (Vector2)MyTransform.position;
	}


	void OnDrawGizmos()
	{
		Gizmos.color = GizmoColor;
		Gizmos.DrawSphere(transform.position, NodeRadius);
	}
	void OnDrawGizmosSelected()
	{
		if (!Application.isEditor) return;

		Gizmos.color = GizmoColor;

		if (FindConnectionsAtStartup && !Application.isPlaying)
		{
			Gizmos.DrawWireSphere(transform.position, ConnectionSearchRadius);
		}

		MyTransform = transform;
		Vector3 startP = MyTransform.position;

		foreach (NavNodeComponent end in Connections)
		{
			if (end == null) continue;

			Vector3 endP = end.transform.position;

			Gizmos.DrawLine(startP, endP);

			Vector3 endToStartNorm = (startP - endP).normalized,
					endToStartNormPerp = new Vector3(-endToStartNorm.y, endToStartNorm.x, 0.0f);

			Vector3 arrowDir1 = (endToStartNorm + endToStartNormPerp) * 0.5f,
				    arrowDir2 = (endToStartNorm - endToStartNormPerp) * 0.5f;

			Gizmos.DrawLine(endP, endP + (NodeRadius * 2.0f * arrowDir1));
			Gizmos.DrawLine(endP, endP + (NodeRadius * 2.0f * arrowDir2));
		}
	}
}