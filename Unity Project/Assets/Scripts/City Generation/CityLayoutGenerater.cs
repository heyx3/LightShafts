using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Generates the layout for a city by splitting it into smaller "blocks" separated by roads.
/// </summary>
[System.Serializable]
public class CityLayoutGenerator
{
	/// <summary>
	/// The major roads, represented as a dictionary that indexes each intersection position
	/// into a collection of intersections that are directly connected to it.
	/// </summary>
	public Dictionary<Vector2, List<Vector2>> RoadConnections { get; private set; }

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

	public int NBlocksX = 3,
		       NBlocksY = 3;
	public Vector2 BlockBaseSize = new Vector2(1000.0f, 1000.0f);
	public Vector2 BlockSizeVariation = new Vector2(200.0f, 200.0f);

	public int Seed = 42;

	public int MinOpenSpaces = 1,
			   MaxOpenSpaces = 3;


	public CityLayoutGenerator()
	{
		RoadConnections = new Dictionary<Vector2, List<Vector2>>();
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
		RoadConnections.Clear();
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
		List<float> roadWidths = new List<float>(),
					roadHeights = new List<float>();
		for (int x = 0; x <= NBlocksX; ++x)
			roadWidths.Add(Mathf.Lerp(RoadWidthBase - RoadWidthVariance,
									  RoadWidthBase + RoadWidthVariance,
									  Random.Range(0.0f, 1.0f)));
		for (int x = 0; x <= NBlocksX; ++x)
			roadHeights.Add(Mathf.Lerp(RoadWidthBase - RoadWidthVariance,
						 			   RoadWidthBase + RoadWidthVariance,
									   Random.Range(0.0f, 1.0f)));


		//Generate the road points and spaces.

		float posX = roadWidths[0] * 0.5f;
		float posY;
		float prevPosX = float.NaN,
			  prevPosY = float.NaN;

		for (int x = 0; x <= NBlocksX; ++x)
		{
			float roadWidthX = float.NaN;
			if (x < NBlocksX)
			{
				roadWidthX = (0.5f * roadWidths[x]) + (0.5f * roadWidths[x + 1]);
			}
			
			posY = roadHeights[0] * 0.5f;

			for (int y = 0; y <= NBlocksY; ++y)
			{
				float roadWidthY = float.NaN;
				if (y < NBlocksY)
				{
					roadWidthY = (0.5f * roadHeights[y]) + (0.5f * roadHeights[y + 1]);
				}


				//Generate roads connected to this point.

				Vector2 pos = new Vector2(posX, posY);
				RoadConnections.Add(pos, new List<Vector2>());

				if (x > 0)
				{
					RoadConnections[pos].Add(new Vector2(prevPosX, pos.y));
				}
				if (x < NBlocksX)
				{
					RoadConnections[pos].Add(new Vector2(pos.x + (blockWidths[x] + roadWidthX), pos.y));
				}
				if (y > 0)
				{
					RoadConnections[pos].Add(new Vector2(pos.x, prevPosY));
				}
				if (y < NBlocksY)
				{
					RoadConnections[pos].Add(new Vector2(pos.x, pos.y + (blockHeights[y] + roadWidthY)));
				}

				//Generate a block.
				if (x < NBlocksX && y < NBlocksY)
				{
					Blocks.Add(new Rect(posX + (0.5f * roadWidths[x]),
										posY + (0.5f * roadHeights[y]),
										blockWidths[x], blockHeights[y]));
				}

				prevPosY = posY;

				if (y < NBlocksY)
				{
					posY += blockHeights[y] + roadWidthY;
				}
				else
				{
					CitySize = new Vector2(CitySize.x, posY + (0.5f * roadHeights[y]));
				}
			}

			prevPosX = posX;
			if (x < NBlocksX)
			{
				posX += blockWidths[x] + roadWidthX;
			}
			else
			{
				CitySize = new Vector2(posX + (roadWidths[x] * 0.5f), CitySize.y);
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