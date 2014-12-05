using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Visualizes road generation for testing.
/// </summary>
public class RoadVisualizer : MonoBehaviour
{
	public Material OpaqueRoadMat, TransparentRoadMat;
	public Texture2D MajorRoadLines, MinorRoadLines, AlleyLines,
				     MajorRoadBase, MinorRoadBase, AlleyBase;

	public CityLayoutGenerator CityLayoutGen = new CityLayoutGenerator();
	public BlockLayoutGenerator BlockLayoutGen = new BlockLayoutGenerator();
	public RoadGenerator RoadGen = new RoadGenerator();

	public bool ShouldGenerateNow = false;


	private Transform roadContainer = null;


	void Awake()
	{
		roadContainer = new GameObject("Test Roads").transform;
	}

	void Update()
	{
		if (ShouldGenerateNow)
		{
			ShouldGenerateNow = false;

			for (int i = roadContainer.childCount - 1; i >= 0; --i)
				Destroy(roadContainer.GetChild(i));

			CityLayoutGen.Generate();


			//Create major road mesh objects.
			TileVertexList baseVerts = new TileVertexList(),
						   lineVerts = new TileVertexList();
			RoadGen.GenerateMajorRoads(CityLayoutGen.VerticalRoads, CityLayoutGen.HorizontalRoads,
									   baseVerts, lineVerts,
									   new Vector2(MajorRoadLines.width, MajorRoadLines.height));
			CreateRoadObject(baseVerts, OpaqueRoadMat, MajorRoadBase, "Major Road Base Mesh");
			CreateRoadObject(lineVerts, TransparentRoadMat, MajorRoadLines, "Major Road Line Mesh");


			//Generate minor roads.
			UnityEngine.Random.seed = BlockLayoutGen.Seed;
			foreach (Rect block in CityLayoutGen.Blocks)
			{
				BlockLayoutGenerator blockGen = new BlockLayoutGenerator(block, BlockLayoutGen);
				blockGen.Seed = UnityEngine.Random.Range(0, 999191919);
				blockGen.Generate();

				TileVertexList baseVertsRoad = new TileVertexList(),
							   lineVertsRoad = new TileVertexList(),
							   baseVertsAlley = new TileVertexList(),
							   lineVertsAlley = new TileVertexList();
				RoadGen.GenerateMinorRoads(blockGen.VerticalRoads, blockGen.HorizontalRoads,
										   baseVertsRoad, lineVertsRoad, baseVertsAlley, lineVertsAlley,
										   new Vector2(AlleyLines.width, AlleyLines.height),
										   new Vector2(MinorRoadLines.width, MinorRoadLines.height));
				CreateRoadObject(baseVertsRoad, OpaqueRoadMat, MinorRoadBase, "Minor Road Base Mesh");
				CreateRoadObject(baseVertsAlley, OpaqueRoadMat, AlleyBase, "Alley Base Mesh");
				CreateRoadObject(lineVertsRoad, TransparentRoadMat, MinorRoadLines, "Minor Road Lines Mesh");
				CreateRoadObject(lineVertsAlley, TransparentRoadMat, AlleyLines, "Alley Lines Mesh");
			}
		}
	}

	private void CreateRoadObject(TileVertexList verts, Material mat, Texture2D tex, string objectName)
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
	}
}