using System;
using System.Collections.Generic;
using UnityEngine;

using Road = CityLayoutGenerator.Road;


/// <summary>
/// Generates the full game world from scratch.
/// </summary>
public class GeneratorComponent : MonoBehaviour
{
	public CityLayoutGenerator CityLayoutGen = new CityLayoutGenerator();
	public BlockLayoutGenerator BlockLayoutGen = new BlockLayoutGenerator();
	public BuildingGenerator BuildingGen = new BuildingGenerator();
	public RoadGenerator RoadGen = new RoadGenerator();
	
	public Material OpaqueTileMat, TransparentTileMat;
	public Texture2D MajorRoadLines, MinorRoadLines, AlleyLines,
				     MajorRoadBase, MinorRoadBase, AlleyBase;
	public Texture2D LargeOpenSpace, SmallOpenSpace;

	public float LargeOpenSpaceScale = 8.0f,
				 SmallOpenSpaceScale = 8.0f;

	public float OpenSpacesZ = 2.0f;


	private Transform tileContainer = null;


	private class NavRoadway
	{
		//Each vertical street's navigation nodes in ascending order by Y position.
		//The roads themselves are ordered in ascending order by position.
		public List<List<NavNode>> VertRoads = new List<List<NavNode>>();
		//Each horizontal street's navigation nodes in ascending order by X position.
		//The roads themselves are ordered in ascending order by position.
		public List<List<NavNode>> HorzRoads = new List<List<NavNode>>();
	}
	void Awake()
	{
		tileContainer = new GameObject("Tiles").transform;


		//Generate city.
		CityLayoutGen.Generate();


		//Generate large open spaces.

		TileVertexList spaceVerts = new TileVertexList();
		List<Rect> openBlocks = new List<Rect>();
		openBlocks.AddRange(CityLayoutGen.OpenSpaces);
		foreach (Rect space in CityLayoutGen.OpenSpaces)
		{
			int startI = spaceVerts.Poses.Count;

			spaceVerts.Poses.Add(new Vector3(space.xMin, space.yMin, OpenSpacesZ));
			spaceVerts.UVs.Add((Vector2)spaceVerts.Poses[spaceVerts.Poses.Count - 1] * LargeOpenSpaceScale);
			spaceVerts.Poses.Add(new Vector3(space.xMin, space.yMax, OpenSpacesZ));
			spaceVerts.UVs.Add((Vector2)spaceVerts.Poses[spaceVerts.Poses.Count - 1] * LargeOpenSpaceScale);
			spaceVerts.Poses.Add(new Vector3(space.xMax, space.yMin, OpenSpacesZ));
			spaceVerts.UVs.Add((Vector2)spaceVerts.Poses[spaceVerts.Poses.Count - 1] * LargeOpenSpaceScale);
			spaceVerts.Poses.Add(new Vector3(space.xMax, space.yMax, OpenSpacesZ));
			spaceVerts.UVs.Add((Vector2)spaceVerts.Poses[spaceVerts.Poses.Count - 1] * LargeOpenSpaceScale);
			
			spaceVerts.Indices.Add(startI);
			spaceVerts.Indices.Add(startI + 1);
			spaceVerts.Indices.Add(startI + 2);
			
			spaceVerts.Indices.Add(startI + 2);
			spaceVerts.Indices.Add(startI + 1);
			spaceVerts.Indices.Add(startI + 3);
		}
		CreateTileObject(spaceVerts, OpaqueTileMat, LargeOpenSpace, "Large Open Spaces");


		//Generate blocks.

		UnityEngine.Random.seed = BlockLayoutGen.Seed;
		
		List<Rect> buildings = new List<Rect>();
		List<Rect> openBuildings = new List<Rect>();
		Dictionary<Rect, List<Road>> vertRoadsByBlock = new Dictionary<Rect, List<Road>>(),
									 horzRoadsByBlock = new Dictionary<Rect, List<Road>>();
		foreach (Rect block in CityLayoutGen.Blocks)
		{
			BlockLayoutGenerator blockGen = new BlockLayoutGenerator(block, BlockLayoutGen.RoadWidthBase,
																	 BlockLayoutGen.RoadWidthVariance,
																	 BlockLayoutGen.BuildingSizeBase,
																	 BlockLayoutGen.BuildingSizeVariance,
																	 UnityEngine.Random.Range(0, 99999999),
																	 BlockLayoutGen.EmptySpaceChance);
			blockGen.Generate();
			vertRoadsByBlock.Add(block, blockGen.VerticalRoads);
			horzRoadsByBlock.Add(block, blockGen.HorizontalRoads);

			buildings.AddRange(blockGen.BuildingSpaces);
			
			//Generate minor roads.
			TileVertexList baseVertsMinor = new TileVertexList(),
						   lineVertsMinor = new TileVertexList(),
						   baseVertsAlley = new TileVertexList(),
						   lineVertsAlley = new TileVertexList();
			RoadGen.GenerateMinorRoads(block, blockGen.VerticalRoads, blockGen.HorizontalRoads,
									   baseVertsMinor, lineVertsMinor,
									   baseVertsAlley, lineVertsAlley,
									   new Vector2(AlleyLines.width, AlleyLines.height),
									   new Vector2(MinorRoadLines.width, MinorRoadLines.height));
			CreateTileObject(baseVertsMinor, OpaqueTileMat, MinorRoadBase, "Minor Road Base Mesh");
			CreateTileObject(lineVertsMinor, TransparentTileMat, MinorRoadLines, "Minor Road Lines Mesh");
			CreateTileObject(baseVertsAlley, OpaqueTileMat, AlleyBase, "Alley Base Mesh");
			CreateTileObject(lineVertsAlley, TransparentTileMat, AlleyLines, "Alley Lines Mesh");


			//Generate empty building spaces.
			spaceVerts = new TileVertexList();
			openBuildings.AddRange(blockGen.OpenSpaces);
			foreach (Rect space in blockGen.OpenSpaces)
			{
				int startI = spaceVerts.Poses.Count;

				spaceVerts.Poses.Add(new Vector3(space.xMin, space.yMin, OpenSpacesZ));
				spaceVerts.UVs.Add((Vector2)spaceVerts.Poses[spaceVerts.Poses.Count - 1] * SmallOpenSpaceScale);
				spaceVerts.Poses.Add(new Vector3(space.xMin, space.yMax, OpenSpacesZ));
				spaceVerts.UVs.Add((Vector2)spaceVerts.Poses[spaceVerts.Poses.Count - 1] * SmallOpenSpaceScale);
				spaceVerts.Poses.Add(new Vector3(space.xMax, space.yMin, OpenSpacesZ));
				spaceVerts.UVs.Add((Vector2)spaceVerts.Poses[spaceVerts.Poses.Count - 1] * SmallOpenSpaceScale);
				spaceVerts.Poses.Add(new Vector3(space.xMax, space.yMax, OpenSpacesZ));
				spaceVerts.UVs.Add((Vector2)spaceVerts.Poses[spaceVerts.Poses.Count - 1] * SmallOpenSpaceScale);

				spaceVerts.Indices.Add(startI);
				spaceVerts.Indices.Add(startI + 1);
				spaceVerts.Indices.Add(startI + 2);

				spaceVerts.Indices.Add(startI + 2);
				spaceVerts.Indices.Add(startI + 1);
				spaceVerts.Indices.Add(startI + 3);
			}
			CreateTileObject(spaceVerts, OpaqueTileMat, SmallOpenSpace, "Small Open Space");
		}


		//Generate major roads.
		TileVertexList baseVertsMajor = new TileVertexList(),
					   lineVertsMajor = new TileVertexList();
		RoadGen.GenerateMajorRoads(CityLayoutGen.VerticalRoads, CityLayoutGen.HorizontalRoads,
								   baseVertsMajor, lineVertsMajor,
								   new Vector2(MajorRoadLines.width, MajorRoadLines.height));
		CreateTileObject(baseVertsMajor, OpaqueTileMat, MajorRoadBase, "Major Road Base Mesh");
		CreateTileObject(lineVertsMajor, TransparentTileMat, MajorRoadLines, "Major Road Line Mesh");


		//Generate buildings.
		List<BuildingGenerator.NodeToSearch> nodesToSearch = new List<BuildingGenerator.NodeToSearch>();
		nodesToSearch.Capacity += buildings.Count * 4;
		float largestPossibleBuildingRadius = (BlockLayoutGen.BuildingSizeBase +
											   BlockLayoutGen.BuildingSizeVariance).magnitude;
		foreach (Rect building in buildings)
		{
			BuildingGenerator buildGen = new BuildingGenerator(building, BuildingGen);
			nodesToSearch.AddRange(buildGen.GenerateBuilding(1.25f * largestPossibleBuildingRadius));
		}

		//foreach (BuildingGenerator.NodeToSearch ns in nodesToSearch)
		//	ns.Node.FindConnections(null, ns.SearchRadius);
		nodesToSearch.Clear();

		//Generate path nodes inside open spaces.

		Vector2 maxRoadSize = 2.0f *
							  new Vector2(CityLayoutGen.RoadWidthBase + CityLayoutGen.RoadWidthVariance,
										  CityLayoutGen.RoadWidthBase + CityLayoutGen.RoadWidthVariance);
		foreach (Rect space in openBlocks)
		{
			Transform spaceNode = new GameObject("Open Block Path Node").transform;
			spaceNode.position = (Vector3)space.center;
			//NavNodeComponent spaceNavNode = spaceNode.gameObject.AddComponent<NavNodeComponent>();
			//spaceNavNode.FindConnections(null, (maxRoadSize + space.size).magnitude);
		}

		maxRoadSize = 2.0f *
					  new Vector2(BlockLayoutGen.RoadWidthBase + BlockLayoutGen.RoadWidthVariance,
								  BlockLayoutGen.RoadWidthBase + BlockLayoutGen.RoadWidthVariance);
		foreach (Rect space in openBuildings)
		{
			Transform spaceNode = new GameObject("Open Building Path Node").transform;
			spaceNode.position = (Vector3)space.center;
			//NavNodeComponent spaceNavNode = spaceNode.gameObject.AddComponent<NavNodeComponent>();
			//spaceNavNode.FindConnections(null, (maxRoadSize + space.size).magnitude);
		}
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.magenta;
		
		Vector2 cityLayoutMaxSize = new Vector2((CityLayoutGen.NBlocksX * (CityLayoutGen.BlockBaseSize.x +
																		   CityLayoutGen.BlockSizeVariation.x)) +
												 ((CityLayoutGen.NBlocksX + 1) *
													(CityLayoutGen.RoadWidthBase +
													 CityLayoutGen.RoadWidthVariance)),
												(CityLayoutGen.NBlocksY * (CityLayoutGen.BlockBaseSize.y +
																		   CityLayoutGen.BlockSizeVariation.y)) +
												 ((CityLayoutGen.NBlocksY + 1) *
													(CityLayoutGen.RoadWidthBase +
													 CityLayoutGen.RoadWidthVariance)));
		Rect cityBounds = new Rect(0.0f, 0.0f, cityLayoutMaxSize.x, cityLayoutMaxSize.y);
		Gizmos.DrawCube(cityBounds.center, (Vector3)cityBounds.size + new Vector3(0, 0, 1));
	}


	private GameObject CreateTileObject(TileVertexList verts, Material mat, Texture2D tex, string objectName)
	{
		Mesh msh = new Mesh();

		msh.vertices = verts.Poses.ToArray();
		msh.uv = verts.UVs.ToArray();
		msh.triangles = verts.Indices.ToArray();

		GameObject obj = new GameObject(objectName);
		obj.transform.position = Vector3.zero;
		obj.transform.parent = tileContainer;
		MeshFilter mf = obj.AddComponent<MeshFilter>();
		mf.mesh = msh;
		MeshRenderer mr = obj.AddComponent<MeshRenderer>();
		mr.material = mat;
		mr.material.mainTexture = tex;

		return obj;
	}
}