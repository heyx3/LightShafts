using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Represents a unique NavNode instance.
/// </summary>
public class NavNodeComponent : MonoBehaviour
{
	public Transform MyTransform { get; private set; }
	private NavGraph Graph { get { return NavGraphComponent.Graph; } }


	public List<NavNodeComponent> Connections = new List<NavNodeComponent>();

	public Color GizmoColor = Color.white;
	public float NodeRadius = 10.0f;

	private NavNode node = new NavNode();


	void Awake()
	{
		MyTransform = transform;
	}

	void Start()
	{
		if (!Graph.ConnectionsFromNode.ContainsKey(node))
		{
			Graph.ConnectionsFromNode.Add(node, Connections.ConvertAll(n => n.node));
		}
	}
	void OnDestroy()
	{
		Graph.ConnectionsFromNode.Remove(node);
	}

	void LateUpdate()
	{
		node.Pos = (Vector2)MyTransform.position;
	}


	void OnDrawGizmos()
	{
		if (!Application.isEditor) return;

		Gizmos.color = GizmoColor;

		Vector3 startP = transform.position;

		Gizmos.DrawSphere(startP, NodeRadius);

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