using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Generates the layout for a city by splitting it into smaller "blocks" separated by roads.
/// </summary>
[System.Serializable]
public class CityLayoutGenerator
{
	/// <summary>
	/// Represents a vertical or horizontal road.
	/// Has a position and a width.
	/// </summary>
	public struct Road
	{
		public float Pos, Width;
		public Road(float pos, float width) { Pos = pos; Width = width; }
		public void SetPos(float pos) { Pos = pos; }
		public void SetWidth(float width) { Width = width; }
	}


	/// <summary>
	/// The horizontal roads separating each block row, in ascending order by position.
	/// </summary>
	public List<Road> HorizontalRoads { get; private set; }
	/// <summary>
	/// The vertical roads separating each block column, in ascending order by position.
	/// </summary>
	public List<Road> VerticalRoads { get; private set; }

	/// <summary>
	/// The individual city blocks.
	/// </summary>
	public List<Rect> Blocks { get; private set; }
	/// <summary>e
	/// The individual open spaces in the city with no buildings.
	/// </summary>
	public List<Rect> OpenSpaces { get; private set; }

	/// <summary>
	/// The dimensions of the city.
	/// </summary>
	public Vector2 CitySize { get; private set; }


	public float RoadWidthBase = 150.0f,
				 RoadWidthVariance = 50.0f;

	public int NBlocksX = 5,
		       NBlocksY = 5;
	public Vector2 BlockBaseSize = new Vector2(1000.0f, 1000.0f);
	public Vector2 BlockSizeVariation = new Vector2(200.0f, 200.0f);

	public int Seed = 42;

	public int MinOpenSpaces = 8,
			   MaxOpenSpaces = 12;


	public CityLayoutGenerator()
	{
		HorizontalRoads = new List<Road>();
		VerticalRoads = new List<Road>();

		Blocks = new List<Rect>();
		OpenSpaces = new List<Rect>();
		CitySize = Vector2.zero;
	}
	public CityLayoutGenerator(float roadWidthBase, float roadWidthVariance, int nBlocksX, int nBlocksY,
							   Vector2 blockBaseSize, Vector2 blockSizeVariation, int seed,
							   int minOpenSpaces, int maxOpenSpaces) : this()
	{
		RoadWidthBase = roadWidthBase;
		RoadWidthVariance = roadWidthVariance;
		NBlocksX = nBlocksX;
		NBlocksY = nBlocksY;

		BlockBaseSize = blockBaseSize;
		BlockSizeVariation = blockSizeVariation;
		Seed = seed;
		MinOpenSpaces = minOpenSpaces;
		MaxOpenSpaces = maxOpenSpaces;
	}


	/// <summary>
	/// Generates the grid layout and stores the results into "RoadConnections", "Blocks", and "OpenSpaces".
	/// </summary>
	public void Generate()
	{
		HorizontalRoads.Clear();
		VerticalRoads.Clear();
		Blocks.Clear();
		OpenSpaces.Clear();

		Random.seed = Seed;


		//Get the size of each block.
		List<float> blockWidths = new List<float>(),
					blockHeights = new List<float>();
		for (int x = 0; x < NBlocksX; ++x)
			blockWidths.Add(Mathf.Lerp(BlockBaseSize.x - BlockSizeVariation.x,
									   BlockBaseSize.x + BlockSizeVariation.x,
									   Random.Range(0.0f, 1.0f)));
		for (int y = 0; y < NBlocksY; ++y)
			blockHeights.Add(Mathf.Lerp(BlockBaseSize.y - BlockSizeVariation.y,
										BlockBaseSize.y + BlockSizeVariation.y,
										Random.Range(0.0f, 1.0f)));

		//Get the size of each road.
		for (int x = 0; x <= NBlocksX; ++x)
		{
			Road rd = new Road();
			rd.Width = Mathf.Lerp(RoadWidthBase - RoadWidthVariance,
								  RoadWidthBase + RoadWidthVariance,
								  Random.Range(0.0f, 1.0f));
			VerticalRoads.Add(rd);
		}
		for (int y = 0; y <= NBlocksY; ++y)
		{
			Road rd = new Road();
			rd.Width = Mathf.Lerp(RoadWidthBase - RoadWidthVariance,
								  RoadWidthBase + RoadWidthVariance,
								  Random.Range(0.0f, 1.0f));
			HorizontalRoads.Add(rd);
		}


		//Generate the road poisitions and block spaces.

		float posX = VerticalRoads[0].Width * 0.5f;
		float posY;

		for (int x = 0; x <= NBlocksX; ++x)
		{
			VerticalRoads[x] = new Road(posX, VerticalRoads[x].Width);

			float roadWidthX = float.NaN;
			if (x < NBlocksX)
			{
				roadWidthX = (0.5f * VerticalRoads[x].Width) +
							 (0.5f * VerticalRoads[x + 1].Width);
			}
			
			posY = HorizontalRoads[0].Width * 0.5f;

			for (int y = 0; y <= NBlocksY; ++y)
			{
				HorizontalRoads[y] = new Road(posY, HorizontalRoads[y].Width);

				float roadWidthY = float.NaN;
				if (y < NBlocksY)
				{
					roadWidthY = (0.5f * HorizontalRoads[y].Width) +
								 (0.5f * HorizontalRoads[y + 1].Width);
				}

				//Generate a block.
				if (x < NBlocksX && y < NBlocksY)
				{
					Blocks.Add(new Rect(posX + (0.5f * VerticalRoads[x].Width),
										posY + (0.5f * HorizontalRoads[y].Width),
										blockWidths[x], blockHeights[y]));
				}

				if (y < NBlocksY)
				{
					posY += blockHeights[y] + roadWidthY;
				}
				else
				{
					CitySize = new Vector2(CitySize.x, posY + (0.5f * HorizontalRoads[y].Width));
				}
			}

			if (x < NBlocksX)
			{
				posX += blockWidths[x] + roadWidthX;
			}
			else
			{
				CitySize = new Vector2(posX + (VerticalRoads[x].Width * 0.5f), CitySize.y);
			}
		}


		//Take a randomized number of blocks and turn them into open spaces.

		int nEmptyBlocks = Random.Range(MinOpenSpaces, MaxOpenSpaces);
		nEmptyBlocks = Mathf.Min(nEmptyBlocks, Blocks.Count);

		for (int i = 0; i < nEmptyBlocks; ++i)
		{
			int index = Random.Range(0, Blocks.Count - 1);
			OpenSpaces.Add(Blocks[index]);
			Blocks.RemoveAt(index);
		}
	}
}