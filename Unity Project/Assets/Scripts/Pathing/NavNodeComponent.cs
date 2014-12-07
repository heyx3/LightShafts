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


	public float GizmoSphereRadius = 10.0f;
	public Color GizmoColor = Color.white;


	/// <summary>
	/// Checks for any nav graph connections to the given nodes.
	/// It is not a problem if this node is in the given list; it will just be ignored.
	/// If "null" is passed for the list to check, this node will check ALL navigation nodes.
	/// </summary>
	public void FindConnections(List<NavNodeComponent> toCheck = null, float searchRadius = 9999999.0f)
	{
		if (toCheck == null) toCheck = Components;


		//Find all nav nodes that are reachable.
		Vector2 myPos = (Vector2)MyTransform.position;
		float playerRadius = PlayerInput.Instance.GetComponent<CircleCollider2D>().bounds.extents.x;
		float searchRadiusSqr = searchRadius * searchRadius;

		foreach (NavNodeComponent navNode in toCheck)
		{
			//Don't check the node if it's already known to connect or if it's actually this node.
			if (navNode == this || Connections.Contains(navNode))
				continue;

			//Don't check the node if it's too far away.
			Vector2 rayDir = navNode.node.Pos - myPos;
			float rayLenSqr = rayDir.sqrMagnitude;
			if (rayLenSqr > searchRadiusSqr)
				continue;

			//Check the node.
			float rayLen = Mathf.Sqrt(rayLenSqr);
			RaycastHit2D hit = Physics2D.CircleCast(myPos, playerRadius, rayDir / rayLen, rayLen,
													MovementHandler.NavBlockerOnlyLayerMask);
			if (hit.collider == null)
			{
				Connections.Add(navNode);
				navNode.Connections.Add(this);
			}
		}
	}


	void Awake()
	{
		MyTransform = transform;
		Components.Add(this);

		node.Pos = (Vector2)MyTransform.position;
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
		Gizmos.DrawSphere(transform.position, GizmoSphereRadius);
	}
	void OnDrawGizmosSelected()
	{
		if (!Application.isEditor) return;

		Gizmos.color = GizmoColor;

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

			Gizmos.DrawLine(endP, endP + (GizmoSphereRadius * 2.0f * arrowDir1));
			Gizmos.DrawLine(endP, endP + (GizmoSphereRadius * 2.0f * arrowDir2));
		}
	}
}