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


	private Transform roadContainer = null;


	void Awake()
	{
		roadContainer = new GameObject("Road Tiles").transform;


		//Generate city.
		CityLayoutGen.Generate();


		//Generate blocks.

		UnityEngine.Random.seed = BlockLayoutGen.Seed;
		
		List<Rect> buildings = new List<Rect>();
		foreach (Rect block in CityLayoutGen.Blocks)
		{
			BlockLayoutGenerator blockGen = new BlockLayoutGenerator(block, BlockLayoutGen.RoadWidthBase,
																	 BlockLayoutGen.RoadWidthVariance,
																	 BlockLayoutGen.BuildingSizeBase,
																	 BlockLayoutGen.BuildingSizeVariance,
																	 UnityEngine.Random.Range(0, 99999999),
																	 BlockLayoutGen.EmptySpaceChance);
			blockGen.Generate();

			buildings.AddRange(blockGen.BuildingSpaces);
			
			//Generate minor roads.
			TileVertexList baseVertsMinor = new TileVertexList(),
						   lineVertsMinor = new TileVertexList(),
						   baseVertsAlley = new TileVertexList(),
						   lineVertsAlley = new TileVertexList();
			RoadGen.GenerateMinorRoads(blockGen.VerticalRoads, blockGen.HorizontalRoads,
									   baseVertsMinor, lineVertsMinor,
									   baseVertsAlley, lineVertsAlley,
									   new Vector2(AlleyLines.width, AlleyLines.height),
									   new Vector2(MinorRoadLines.width, MinorRoadLines.height));
			CreateTileObject(baseVertsMinor, OpaqueTileMat, MinorRoadBase, "Minor Road Base Mesh");
			CreateTileObject(lineVertsMinor, TransparentTileMat, MinorRoadLines, "Minor Road Lines Mesh");
			CreateTileObject(baseVertsAlley, OpaqueTileMat, AlleyBase, "Alley Base Mesh");
			CreateTileObject(lineVertsAlley, TransparentTileMat, AlleyLines, "Alley Lines Mesh");
		}


		//Generate buildings.
		foreach (Rect building in buildings)
		{
			BuildingGenerator buildGen = new BuildingGenerator(building, BuildingGen);
			buildGen.GenerateBuilding();
		}


		//Generate major roads.
		TileVertexList baseVertsMajor = new TileVertexList(),
					   lineVertsMajor = new TileVertexList();
		RoadGen.GenerateMajorRoads(CityLayoutGen.VerticalRoads, CityLayoutGen.HorizontalRoads,
								   baseVertsMajor, lineVertsMajor,
								   new Vector2(MajorRoadLines.width, MajorRoadLines.height));
		CreateTileObject(baseVertsMajor, OpaqueTileMat, MajorRoadBase, "Major Road Base Mesh");
		CreateTileObject(lineVertsMajor, TransparentTileMat, MajorRoadLines, "Major Road Line Mesh");


		//Generate path nodes for roads.
		//TODO: Implement.
	}

	private GameObject CreateTileObject(TileVertexList verts, Material mat, Texture2D tex, string objectName)
	{
		Mesh msh = new Mesh();

		msh.vertices = verts.Poses.ToArray();
		msh.uv = verts.UVs.ToArray();
		msh.triangles = verts.Indices.ToArray();

		GameObject obj = new GameObject(objectName);
		obj.transform.position = Vector3.zero;
		obj.transform.parent = roadContainer;
		MeshFilter mf = obj.AddComponent<MeshFilter>();
		mf.mesh = msh;
		MeshRenderer mr = obj.AddComponent<MeshRenderer>();
		mr.material = mat;
		mr.material.mainTexture = tex;

		return obj;
	}
}