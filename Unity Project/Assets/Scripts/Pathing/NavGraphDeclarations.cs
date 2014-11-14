using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A concrete node class for the game's navigation graph.
/// </summary>
public class NavNode : Node
{
	public Vector2 Pos;

	public override bool IsEqualTo(Node other)
	{
		return ReferenceEquals(other, this);
	}
	public override bool IsNotEqualTo(Node other)
	{
		return !ReferenceEquals(other, this);
	}

	public override int GetHashCode()
	{
		return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
	}
}

/// <summary>
/// A concrete edge class for the game's navigation graph.
/// </summary>
public class NavEdge : Edge<NavNode>
{
	public NavEdge(NavNode start, NavNode end) : base(start, end) { }

	public override float GetTraversalCost(PathFinder<NavNode> pather)
	{
		if (pather.HasAnEnd)
		{
			return Vector2.SqrMagnitude(Start.Pos - End.Pos) +
				   Vector2.SqrMagnitude(End.Pos - pather.End.Pos);
		}
		else
		{
			return Vector2.SqrMagnitude(Start.Pos - End.Pos);
		}
	}
	public override float GetSearchCost(PathFinder<NavNode> pather)
	{
		return Vector2.SqrMagnitude(Start.Pos - End.Pos);
	}
}

/// <summary>
/// Contains a collection of each node's connections in the graph.
/// The connecting nodes are stored as the Monobehaviour that represents each node.
/// This is done to simplify the interface between this nav graph and the Monobehaviour scripts.
/// </summary>
public class NavGraph : Graph<NavNode>
{
	public Dictionary<NavNode, List<NavNodeComponent>> ConnectionsFromNode =
		new Dictionary<NavNode, List<NavNodeComponent>>();

	public void GetConnections(NavNode starting, List<Edge<NavNode>> outEdgeList)
	{
		if (ConnectionsFromNode.ContainsKey(starting))
			outEdgeList.AddRange(ConnectionsFromNode[starting].ConvertAll(n => (Edge<NavNode>)new NavEdge(starting, n.MyNode)));
	}
}