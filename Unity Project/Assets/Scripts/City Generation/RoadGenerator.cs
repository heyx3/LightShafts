using System;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Generates the road tiles.
/// </summary>
[Serializable]
public class RoadGenerator
{
	public float BaseRoadDepth = 0.0f;
	public float BaseRoadTexScale = 0.25f;


	/// <summary>
	/// Creates road mesh data for the given stretch of road.
	/// Puts mesh data into the "base" mesh and the "lines" mesh.
	/// </summary>
	public static void CreateRoad(Vector2 start, Vector2 end, float roadWidth, float posZ,
								  List<Vector3> basePoses, List<Vector2> baseUVs, List<int> baseIndices,
								  List<Vector3> linePoses, List<Vector2> lineUVs, List<int> lineIndices,
								  float lineTexHeight, float baseRoadScale)
	{
		Vector2 towardsEnd = (end - start).normalized,
				perp = new Vector2(-towardsEnd.y, towardsEnd.x);
		Vector2 dir1 = perp * (roadWidth * 0.5f),
				dir2 = perp * (roadWidth * -0.5f);

		int startI = linePoses.Count;
		linePoses.Add(new Vector3(start.x + dir1.x, start.y + dir1.y, posZ));
		lineUVs.Add(new Vector2(0.0f, 0.0f));
		linePoses.Add(new Vector3(start.x + dir2.x, start.y + dir2.y, posZ));
		lineUVs.Add(new Vector2(0.0f, 1.0f));
		linePoses.Add(new Vector3(end.x + dir1.x, end.y + dir1.y, posZ));
		lineUVs.Add(new Vector2((end - start).magnitude / lineTexHeight, 0.0f));
		linePoses.Add(new Vector3(end.x + dir2.x, end.y + dir2.y, posZ));
		lineUVs.Add(new Vector2(lineUVs[startI + 2].x, 1.0f));

		lineIndices.Add(startI);
		lineIndices.Add(startI + 2);
		lineIndices.Add(startI + 1);
		lineIndices.Add(startI + 2);
		lineIndices.Add(startI + 3);
		lineIndices.Add(startI + 1);


		startI = basePoses.Count;
		basePoses.Add(linePoses[linePoses.Count - 4]);
		baseUVs.Add((Vector2)basePoses[basePoses.Count - 1] * baseRoadScale);
		basePoses.Add(linePoses[linePoses.Count - 3]);
		baseUVs.Add((Vector2)basePoses[basePoses.Count - 1] * baseRoadScale);
		basePoses.Add(linePoses[linePoses.Count - 2]);
		baseUVs.Add((Vector2)basePoses[basePoses.Count - 1] * baseRoadScale);
		basePoses.Add(linePoses[linePoses.Count - 1]);
		baseUVs.Add((Vector2)basePoses[basePoses.Count - 1] * baseRoadScale);
		
		basePoses[startI] += new Vector3(0.0f, 0.0f, 1f);
		basePoses[startI + 1] += new Vector3(0.0f, 0.0f, 1f);
		basePoses[startI + 2] += new Vector3(0.0f, 0.0f, 1f);
		basePoses[startI + 3] += new Vector3(0.0f, 0.0f, 1f);
		
		baseIndices.Add(startI);
		baseIndices.Add(startI + 2);
		baseIndices.Add(startI + 1);
		baseIndices.Add(startI + 2);
		baseIndices.Add(startI + 3);
		baseIndices.Add(startI + 1);
	}
	/// <summary>
	/// Creates road mesh data for the given intersection of two identical roads.
	/// Puts mesh data into the "base" mesh and the "lines" mesh.
	/// </summary>
	public void CreateIntersection(Vector2 intersectionCenter, float width, float height,
								   float posZ, float baseRoadScale,
								   List<Vector3> basePoses, List<Vector2> baseUvs, List<int> baseIndices,
								   List<Vector3> linePoses, List<Vector2> lineUvs, List<int> lineIndices)
	{
		Vector2 halfSize = new Vector2(width, height) * 0.5f;
		Vector2 min = intersectionCenter - halfSize,
				max = intersectionCenter + halfSize;

		int baseStartI = basePoses.Count;
		basePoses.Add(new Vector3(min.x, min.y, posZ));
		baseUvs.Add((Vector2)basePoses[basePoses.Count - 1] * baseRoadScale);
		basePoses.Add(new Vector3(min.x, max.y, posZ));
		baseUvs.Add((Vector2)basePoses[basePoses.Count - 1] * baseRoadScale);
		basePoses.Add(new Vector3(max.x, min.y, posZ));
		baseUvs.Add((Vector2)basePoses[basePoses.Count - 1] * baseRoadScale);
		basePoses.Add(new Vector3(max.x, max.y, posZ));
		baseUvs.Add((Vector2)basePoses[basePoses.Count - 1] * baseRoadScale);
		
		baseIndices.Add(baseStartI);
		baseIndices.Add(baseStartI + 1);
		baseIndices.Add(baseStartI + 2);
		baseIndices.Add(baseStartI + 2);
		baseIndices.Add(baseStartI + 1);
		baseIndices.Add(baseStartI + 3);
	}


	/// <summary>
	/// Generates mesh data for the given major roads.
	/// Assumes the vert and horz roads are given in increasing order by position.
	/// Stores mesh data for the road lines and the base road tiles into the given lists.
	/// </summary>
	public void GenerateMajorRoads(List<CityLayoutGenerator.Road> vertRoads,
								   List<CityLayoutGenerator.Road> horzRoads,
								   List<Vector3> basePoses, List<Vector2> baseUVs, List<int> baseIndices,
								   List<Vector3> linePoses, List<Vector2> lineUVs, List<int> lineIndices,
								   float lineTexHeight)
	{
		float texCoordScale = 1.0f / BaseRoadTexScale;

		for (int x = 0; x < vertRoads.Count; ++x)
		{
			for (int y = 0; y < horzRoads.Count; ++y)
			{
				//Generate the intersection.
				CreateIntersection(new Vector2(vertRoads[x].Pos, horzRoads[y].Pos),
								   vertRoads[x].Width, horzRoads[y].Width,
								   UnityEngine.Random.Range(BaseRoadDepth - 0.03f, BaseRoadDepth + 0.03f),
								   BaseRoadTexScale,
								   basePoses, baseUVs, baseIndices, linePoses, lineUVs, lineIndices);

				//Generate the road from this intersection towards the right.
				if (x < vertRoads.Count - 1)
				{
					CreateRoad(new Vector2(vertRoads[x].Pos + (0.5f * vertRoads[x].Width),
										   horzRoads[y].Pos),
							   new Vector2(vertRoads[x + 1].Pos - (0.5f * vertRoads[x + 1].Width),
										   horzRoads[y].Pos),
							   horzRoads[y].Width,
							   UnityEngine.Random.Range(BaseRoadDepth - 0.03f, BaseRoadDepth + 0.03f),
							   basePoses, baseUVs, baseIndices, linePoses, lineUVs, lineIndices,
							   lineTexHeight, BaseRoadTexScale);
				}
				//Generate the road from this intersection towards the positive Y.
				if (y < vertRoads.Count - 1)
				{
					CreateRoad(new Vector2(vertRoads[x].Pos,
										   horzRoads[y].Pos + (0.5f * horzRoads[y].Width)),
							   new Vector2(vertRoads[x].Pos,
										   horzRoads[y + 1].Pos - (0.5f * horzRoads[y + 1].Width)),
							   vertRoads[x].Width,
							   UnityEngine.Random.Range(BaseRoadDepth - 0.03f, BaseRoadDepth + 0.03f),
							   basePoses, baseUVs, baseIndices, linePoses, lineUVs, lineIndices,
							   lineTexHeight, BaseRoadTexScale);
				}
			}
		}
	}
}