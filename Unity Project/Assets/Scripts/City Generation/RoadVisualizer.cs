using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Visualizes road generation for testing.
/// </summary>
public class RoadVisualizer : MonoBehaviour
{
	public Material OpaqueRoadMat, TransparentRoadMat;
	public Texture MajorRoadLines, MinorRoadLines, AlleyLines,
				   MajorRoadBase, MinorRoadBase, AlleyBase;

	public CityLayoutGenerator CityLayoutGen = new CityLayoutGenerator();
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


			//Get mesh data for the major roads.

			Mesh majorBase = new Mesh(),
				 majorLines = new Mesh();

			List<Vector3> basePoses = new List<Vector3>(),
						  linePoses = new List<Vector3>();
			List<Vector2> baseUVs = new List<Vector2>(),
						  lineUVs = new List<Vector2>();
			List<int> baseIndices = new List<int>(),
					  lineIndices = new List<int>();

			RoadGen.GenerateMajorRoads(CityLayoutGen.VerticalRoads, CityLayoutGen.HorizontalRoads,
									   basePoses, baseUVs, baseIndices, linePoses, lineUVs, lineIndices,
									   (float)MajorRoadLines.height);

			majorBase.vertices = basePoses.ToArray();
			majorBase.uv = baseUVs.ToArray();
			majorBase.triangles = baseIndices.ToArray();
			majorBase.UploadMeshData(true);

			majorLines.vertices = linePoses.ToArray();
			majorLines.uv = lineUVs.ToArray();
			majorLines.triangles = lineIndices.ToArray();
			majorLines.UploadMeshData(true);


			//Generate meshes for the major roads.

			GameObject baseMajorRoad = new GameObject("Major Roads Base");
			baseMajorRoad.transform.position = Vector3.zero;
			baseMajorRoad.transform.parent = roadContainer;
			MeshFilter mf = baseMajorRoad.AddComponent<MeshFilter>();
			mf.mesh = majorBase;
			MeshRenderer mr = baseMajorRoad.AddComponent<MeshRenderer>();
			mr.material = OpaqueRoadMat;
			mr.material.mainTexture = MajorRoadBase;

			GameObject lineMajorRoad = new GameObject("Major Roads Lines");
			lineMajorRoad.transform.position = Vector3.zero;
			lineMajorRoad.transform.parent = roadContainer;
			mf = lineMajorRoad.AddComponent<MeshFilter>();
			mf.mesh = majorLines;
			mr = lineMajorRoad.AddComponent<MeshRenderer>();
			mr.material = TransparentRoadMat;
			mr.material.mainTexture = MajorRoadLines;
		}
	}
}