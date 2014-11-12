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
		return Pos.GetHashCode();
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
			return Vector2.SqrMagnitude(End.Pos - pather.End.Pos) +
				   Vector2.SqrMagnitude(Start.Pos - End.Pos);
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
/// </summary>
public class NavGraph : Graph<NavNode>
{
	public Dictionary<NavNode, List<NavNode>> ConnectionsFromNode = new Dictionary<NavNode, List<NavNode>>();

	public void GetConnections(NavNode starting, List<Edge<NavNode>> outEdgeList)
	{
		if (ConnectionsFromNode.ContainsKey(starting))
			foreach (NavNode end in ConnectionsFromNode[starting])
				outEdgeList.Add(new NavEdge(starting, end));
	}
}