using System;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Generates the road tiles.
/// </summary>
[Serializable]
public class RoadGenerator
{
	public class RoadVertexList
	{
		public List<Vector3> Poses = new List<Vector3>();
		public List<Vector2> UVs = new List<Vector2>();
		public List<int> Indices = new List<int>();
	}

	public static void CreateHorzRoad(float startX, float endX, float y, float thickness, float posZ,
									  Vector2 lineTexSize, float baseRoadScale, float lineRoadScale,
									  RoadVertexList baseVerts, RoadVertexList lineVerts)
	{
		float halfThickness = thickness * 0.5f;
		float finalLineTexScale = lineTexSize.x / lineTexSize.y / thickness;
		

		//Line texture.

		int startLineI = lineVerts.Poses.Count;

		lineVerts.Poses.Add(new Vector3(startX, y - halfThickness, posZ));
		lineVerts.UVs.Add(new Vector2(startX * finalLineTexScale, 0.0f));
		lineVerts.Poses.Add(new Vector3(startX, y + halfThickness, posZ));
		lineVerts.UVs.Add(new Vector2(startX * finalLineTexScale, 1.0f));
		lineVerts.Poses.Add(new Vector3(endX, y - halfThickness, posZ));
		lineVerts.UVs.Add(new Vector2(endX * finalLineTexScale, 0.0f));
		lineVerts.Poses.Add(new Vector3(endX, y + halfThickness, posZ));
		lineVerts.UVs.Add(new Vector2(endX * finalLineTexScale, 1.0f));

		lineVerts.Indices.Add(startLineI);
		lineVerts.Indices.Add(startLineI + 1);
		lineVerts.Indices.Add(startLineI + 2);
		lineVerts.Indices.Add(startLineI + 2);
		lineVerts.Indices.Add(startLineI + 1);
		lineVerts.Indices.Add(startLineI + 3);


		//Base texture.

		int startBaseI = baseVerts.Poses.Count;
		
		baseVerts.Poses.Add(lineVerts.Poses[startLineI]);
		baseVerts.UVs.Add((Vector2)baseVerts.Poses[baseVerts.Poses.Count - 1] * baseRoadScale);
		baseVerts.Poses.Add(lineVerts.Poses[startLineI + 1]);
		baseVerts.UVs.Add((Vector2)baseVerts.Poses[baseVerts.Poses.Count - 1] * baseRoadScale);
		baseVerts.Poses.Add(lineVerts.Poses[startLineI + 2]);
		baseVerts.UVs.Add((Vector2)baseVerts.Poses[baseVerts.Poses.Count - 1] * baseRoadScale);
		baseVerts.Poses.Add(lineVerts.Poses[startLineI + 3]);
		baseVerts.UVs.Add((Vector2)baseVerts.Poses[baseVerts.Poses.Count - 1] * baseRoadScale);

		for (int i = startBaseI; i <= startBaseI + 3; ++i)
			baseVerts.Poses[i] += new Vector3(0.0f, 0.0f, 1.0f);
		
		baseVerts.Indices.Add(startBaseI);
		baseVerts.Indices.Add(startBaseI + 1);
		baseVerts.Indices.Add(startBaseI + 2);
		baseVerts.Indices.Add(startBaseI + 2);
		baseVerts.Indices.Add(startBaseI + 1);
		baseVerts.Indices.Add(startBaseI + 3);
	}
	public static void CreateVertRoad(float startY, float endY, float x, float thickness, float posZ,
									  Vector2 lineTexSize, float baseRoadScale, float lineRoadScale,
									  RoadVertexList baseVerts, RoadVertexList lineVerts)
	{
		float halfThickness = thickness * 0.5f;
		float finalLineTexScale = lineTexSize.x / lineTexSize.y / thickness;
		

		//Line texture.

		int startLineI = lineVerts.Poses.Count;

		lineVerts.Poses.Add(new Vector3(x - halfThickness, startY, posZ));
		lineVerts.UVs.Add(new Vector2(startY * finalLineTexScale, 0.0f));
		lineVerts.Poses.Add(new Vector3(x + halfThickness, startY, posZ));
		lineVerts.UVs.Add(new Vector2(startY * finalLineTexScale, 1.0f));
		lineVerts.Poses.Add(new Vector3(x - halfThickness, endY, posZ));
		lineVerts.UVs.Add(new Vector2(endY * finalLineTexScale, 0.0f));
		lineVerts.Poses.Add(new Vector3(x + halfThickness, endY, posZ));
		lineVerts.UVs.Add(new Vector2(endY * finalLineTexScale, 1.0f));

		lineVerts.Indices.Add(startLineI);
		lineVerts.Indices.Add(startLineI + 2);
		lineVerts.Indices.Add(startLineI + 1);
		lineVerts.Indices.Add(startLineI + 2);
		lineVerts.Indices.Add(startLineI + 3);
		lineVerts.Indices.Add(startLineI + 1);


		//Base texture.

		int startBaseI = baseVerts.Poses.Count;
		
		baseVerts.Poses.Add(lineVerts.Poses[startLineI]);
		baseVerts.UVs.Add((Vector2)baseVerts.Poses[baseVerts.Poses.Count - 1] * baseRoadScale);
		baseVerts.Poses.Add(lineVerts.Poses[startLineI + 1]);
		baseVerts.UVs.Add((Vector2)baseVerts.Poses[baseVerts.Poses.Count - 1] * baseRoadScale);
		baseVerts.Poses.Add(lineVerts.Poses[startLineI + 2]);
		baseVerts.UVs.Add((Vector2)baseVerts.Poses[baseVerts.Poses.Count - 1] * baseRoadScale);
		baseVerts.Poses.Add(lineVerts.Poses[startLineI + 3]);
		baseVerts.UVs.Add((Vector2)baseVerts.Poses[baseVerts.Poses.Count - 1] * baseRoadScale);

		for (int i = startBaseI; i <= startBaseI + 3; ++i)
			baseVerts.Poses[i] += new Vector3(0.0f, 0.0f, 1.0f);
		
		baseVerts.Indices.Add(startBaseI);
		baseVerts.Indices.Add(startBaseI + 2);
		baseVerts.Indices.Add(startBaseI + 1);
		baseVerts.Indices.Add(startBaseI + 2);
		baseVerts.Indices.Add(startBaseI + 3);
		baseVerts.Indices.Add(startBaseI + 1);
	}

	/// <summary>
	/// Creates road mesh data for the given intersection of two identical roads.
	/// Puts mesh data into the "base" mesh and the "lines" mesh.
	/// </summary>
	public static void CreateIntersection(Vector2 intersectionCenter, float width, float height,
										  float posZ, float baseRoadScale,
										  RoadVertexList baseVerts, RoadVertexList lineVerts)
	{
		Vector2 halfSize = new Vector2(width, height) * 0.5f;
		Vector2 min = intersectionCenter - halfSize,
				max = intersectionCenter + halfSize;

		int baseStartI = baseVerts.Poses.Count;
		baseVerts.Poses.Add(new Vector3(min.x, min.y, posZ));
		baseVerts.UVs.Add((Vector2)baseVerts.Poses[baseVerts.Poses.Count - 1] * baseRoadScale);
		baseVerts.Poses.Add(new Vector3(min.x, max.y, posZ));
		baseVerts.UVs.Add((Vector2)baseVerts.Poses[baseVerts.Poses.Count - 1] * baseRoadScale);
		baseVerts.Poses.Add(new Vector3(max.x, min.y, posZ));
		baseVerts.UVs.Add((Vector2)baseVerts.Poses[baseVerts.Poses.Count - 1] * baseRoadScale);
		baseVerts.Poses.Add(new Vector3(max.x, max.y, posZ));
		baseVerts.UVs.Add((Vector2)baseVerts.Poses[baseVerts.Poses.Count - 1] * baseRoadScale);
		
		baseVerts.Indices.Add(baseStartI);
		baseVerts.Indices.Add(baseStartI + 1);
		baseVerts.Indices.Add(baseStartI + 2);
		baseVerts.Indices.Add(baseStartI + 2);
		baseVerts.Indices.Add(baseStartI + 1);
		baseVerts.Indices.Add(baseStartI + 3);
	}


	public float BaseRoadDepth = 0.0f;
	public float BaseRoadTexScale = 0.25f,
				 LineRoadTexScale = 0.25f;

	public float MinorRoadMinWidth = 50.0f;


	private float GetRandDepth()
	{
		return UnityEngine.Random.Range(BaseRoadDepth - 0.03f, BaseRoadDepth + 0.03f);
	}

	/// <summary>
	/// Generates mesh data for the given major roads.
	/// Assumes the vert and horz roads are given in increasing order by position.
	/// Stores mesh data for the road lines and the base road tiles into the given lists.
	/// </summary>
	public void GenerateMajorRoads(List<CityLayoutGenerator.Road> vertRoads,
								   List<CityLayoutGenerator.Road> horzRoads,
								   RoadVertexList baseVerts, RoadVertexList lineVerts,
								   Vector2 lineTexSize)
	{
		for (int x = 0; x < vertRoads.Count; ++x)
		{
			for (int y = 0; y < horzRoads.Count; ++y)
			{
				//Generate the intersection.
				CreateIntersection(new Vector2(vertRoads[x].Pos, horzRoads[y].Pos),
								   vertRoads[x].Width, horzRoads[y].Width, GetRandDepth(),
								   BaseRoadTexScale, baseVerts, lineVerts);

				//Generate the road from this intersection towards the right.
				if (x < vertRoads.Count - 1)
				{
					CreateHorzRoad(vertRoads[x].Pos + (0.5f * vertRoads[x].Width),
								   vertRoads[x + 1].Pos - (0.5f * vertRoads[x + 1].Width),
								   horzRoads[y].Pos, horzRoads[y].Width, GetRandDepth(),
								   lineTexSize, BaseRoadTexScale, LineRoadTexScale, baseVerts, lineVerts);
				}
				//Generate the road from this intersection towards the positive Y.
				if (y < vertRoads.Count - 1)
				{
					CreateVertRoad(horzRoads[y].Pos + (0.5f * horzRoads[y].Width),
								   horzRoads[y + 1].Pos - (0.5f * horzRoads[y + 1].Width),
								   vertRoads[x].Pos, vertRoads[x].Width, GetRandDepth(),
								   lineTexSize, BaseRoadTexScale, LineRoadTexScale, baseVerts, lineVerts);
				}
			}
		}
	}
	/// <summary>
	/// Generates mesh data for the given minor roads/alleyways.
	/// Assumes the vert and horz roads are given in increasing order by position.
	/// Stores mesh data for the road lines and the base road tiles into the given lists.
	/// </summary>
	public void GenerateMinorRoads(List<CityLayoutGenerator.Road> vertRoads,
								   List<CityLayoutGenerator.Road> horzRoads,
								   RoadVertexList baseVertsRoad, RoadVertexList lineVertsRoad,
								   RoadVertexList baseVertsAlley, RoadVertexList lineVertsAlley,
								   Vector2 alleyLineTexSize, Vector2 minorRoadLineTexSize)
	{
		//Roads are only intercepted if a bigger or equal road intersects it.

		for (int x = 0; x < vertRoads.Count; ++x)
		{
			bool isVertAlley = (vertRoads[x].Width < MinorRoadMinWidth);

			for (int y = 0; y < horzRoads.Count; ++y)
			{
				bool isHorzAlley = (horzRoads[y].Width < MinorRoadMinWidth);


				//If the intersection is between two of the same kind of road, draw an intersection.
				//Otherwise, just continue the bigger road like normal.
				if (isVertAlley == isHorzAlley)
				{
					CreateIntersection(new Vector2(vertRoads[x].Pos, horzRoads[y].Pos),
									   vertRoads[x].Width, horzRoads[y].Width, GetRandDepth(),
									   BaseRoadTexScale,
									   (isVertAlley ? baseVertsAlley : baseVertsRoad),
									   (isVertAlley ? lineVertsAlley : lineVertsRoad));
				}
				else if (isVertAlley)
				{
					CreateHorzRoad(vertRoads[x].Pos - (0.5f * vertRoads[x].Width),
								   vertRoads[x].Pos + (0.5f * vertRoads[x].Width),
								   horzRoads[y].Pos, horzRoads[y].Width, GetRandDepth(),
								   minorRoadLineTexSize, BaseRoadTexScale, LineRoadTexScale,
								   baseVertsRoad, lineVertsRoad);
				}
				else
				{
					CreateVertRoad(horzRoads[y].Pos - (0.5f * horzRoads[y].Width),
								   horzRoads[y].Pos + (0.5f * horzRoads[y].Width),
								   vertRoads[x].Pos, vertRoads[x].Width, GetRandDepth(),
								   minorRoadLineTexSize, BaseRoadTexScale, LineRoadTexScale,
								   baseVertsRoad, lineVertsRoad);
				}

				//Add a road pointing towards the right.
				if (x < vertRoads.Count - 1)
				{
					CreateHorzRoad(vertRoads[x].Pos + (0.5f * vertRoads[x].Width),
								   vertRoads[x + 1].Pos - (0.5f * vertRoads[x + 1].Width),
								   horzRoads[y].Pos, horzRoads[y].Width, GetRandDepth(),
								   (isHorzAlley ? alleyLineTexSize : minorRoadLineTexSize),
								   BaseRoadTexScale, LineRoadTexScale,
								   (isHorzAlley ? baseVertsAlley : baseVertsRoad),
								   (isHorzAlley ? lineVertsAlley : lineVertsRoad));
				}
				//Add a road pointing towards the positive Y.
				if (y < horzRoads.Count - 1)
				{
					CreateVertRoad(horzRoads[y].Pos + (0.5f * horzRoads[y].Width),
								   horzRoads[y + 1].Pos - (0.5f * horzRoads[y + 1].Width),
								   vertRoads[x].Pos, vertRoads[x].Width, GetRandDepth(),
								   (isVertAlley ? alleyLineTexSize : minorRoadLineTexSize),
								   BaseRoadTexScale, LineRoadTexScale,
								   (isVertAlley ? baseVertsAlley : baseVertsRoad),
								   (isVertAlley ? lineVertsAlley : lineVertsRoad));
				}
			}
		}
	}
}