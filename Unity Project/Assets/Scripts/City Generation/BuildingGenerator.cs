using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Generates a building in a rectangular space.
/// </summary>
[Serializable]
public class BuildingGenerator
{
	[NonSerialized] public Rect BuildingArea = new Rect(0.0f, 0.0f, 0.0f, 0.0f);

	public List<GameObject> BuildingPrefabs = new List<GameObject>();
	public List<Vector2> BuildingSizes = new List<Vector2>();

	public float PlayerCollisionRadius = 32.0f;


	public BuildingGenerator() { }
	public BuildingGenerator(Rect buildingArea, BuildingGenerator copy)
	{
		BuildingArea = buildingArea;
		BuildingPrefabs = copy.BuildingPrefabs;
		BuildingSizes = copy.BuildingSizes;
	}


	/// <summary>
	/// A nav node and the search radius to use when finding its connections.
	/// </summary>
	public struct NodeToSearch
	{
		public NavNodeComponent Node;
		public float SearchRadius;
		public NodeToSearch(NavNodeComponent node, float searchRadius) { Node = node; SearchRadius = searchRadius; }
	}
	/// <summary>
	/// Generates a building given this generator's fields.
	/// Returns all the generated nav nodes and the search radii needed for each node when finding its connections.
	/// </summary>
	/// <param name="searchScale">The scale of the search radius for a building's nav nodes.
	/// Should be something around "1.25f * largestPossibleBuildingRadius".</param>
	public List<NodeToSearch> GenerateBuilding(float searchScale)
	{
		searchScale /= BuildingArea.size.magnitude;


		//Get the building prefab that's closest in size to this area.

		Vector2 targetSize = new Vector2(BuildingArea.width, BuildingArea.height);

		int closestIndex = 0;
		Vector2 sizeDelta = new Vector2(targetSize.x / BuildingSizes[0].x,
										targetSize.y / BuildingSizes[0].y);
		float deltaDistSqr = Vector2.SqrMagnitude(sizeDelta - new Vector2(1.0f, 1.0f));

		for (int i = 1; i < BuildingPrefabs.Count; ++i)
		{
			Vector2 sizeDeltaTemp = new Vector2(targetSize.x / BuildingSizes[i].x,
												targetSize.y / BuildingSizes[i].y);
			float deltaDistSqrTemp = Vector2.SqrMagnitude(sizeDeltaTemp - new Vector2(1.0f, 1.0f));
			
			if (deltaDistSqrTemp < deltaDistSqr)
			{
				closestIndex = i;
				sizeDelta = sizeDeltaTemp;
				deltaDistSqr = deltaDistSqrTemp;
			}
		}


		//Now create that prefab.

		Transform buildingTr = ((GameObject)GameObject.Instantiate(BuildingPrefabs[closestIndex])).transform;
		buildingTr.localScale = new Vector3(buildingTr.localScale.x * sizeDelta.x,
											buildingTr.localScale.y * sizeDelta.y,
											buildingTr.localScale.z);
		buildingTr.position = new Vector3(BuildingArea.center.x,
										  BuildingArea.center.y,
										  buildingTr.position.z);


		List<NodeToSearch> pathNodes = new List<NodeToSearch>();
		pathNodes.Capacity = 4;

		//Add path nodes on each corner of the building.
		float searchDist = targetSize.magnitude * searchScale;
		Bounds collBounds = buildingTr.collider2D.bounds;
		Transform minXY = new GameObject("Path Node MinXY Corner").transform,
				  maxXY = new GameObject("Path Node MaxXY Corner").transform,
				  minXMaxY = new GameObject("Path Node MinXMaxY Corner").transform,
				  minYMaxX = new GameObject("Path Node MinYMaxX Corner").transform;
		minXY.position = collBounds.min - new Vector3(PlayerCollisionRadius, PlayerCollisionRadius, 0.0f);
		maxXY.position = collBounds.max + new Vector3(PlayerCollisionRadius, PlayerCollisionRadius, 0.0f);
		minXMaxY.position = new Vector3(collBounds.min.x, collBounds.max.y, collBounds.center.z) +
							new Vector3(-PlayerCollisionRadius, PlayerCollisionRadius, 0.0f);
		minYMaxX.position = new Vector3(collBounds.max.x, collBounds.min.y, collBounds.center.z) +
							new Vector3(PlayerCollisionRadius, -PlayerCollisionRadius, 0.0f);
		pathNodes.Add(new NodeToSearch(minXY.gameObject.AddComponent<NavNodeComponent>(), searchDist));
		pathNodes.Add(new NodeToSearch(maxXY.gameObject.AddComponent<NavNodeComponent>(), searchDist));
		pathNodes.Add(new NodeToSearch(minXMaxY.gameObject.AddComponent<NavNodeComponent>(), searchDist));
		pathNodes.Add(new NodeToSearch(minYMaxX.gameObject.AddComponent<NavNodeComponent>(), searchDist));

		
		//Set up any path nodes in and around the building.
		NavNodeComponent[] nodesInChildren = buildingTr.GetComponentsInChildren<NavNodeComponent>();
		pathNodes.Capacity += nodesInChildren.Length;
		foreach (NavNodeComponent navNode in nodesInChildren)
			pathNodes.Add(new NodeToSearch(navNode, searchDist));


		return pathNodes;
	}
}