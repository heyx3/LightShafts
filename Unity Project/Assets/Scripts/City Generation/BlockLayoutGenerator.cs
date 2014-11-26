using System.Collections.Generic;
using UnityEngine;
using Road = CityLayoutGenerator.Road;


/// <summary>
/// Generates the layout for a block by splitting it into buildings separated by small roads/alleys.
/// Works very similarly to CityLayoutGenerator, but instead o
/// </summary>
[System.Serializable]
public class BlockLayoutGenerator
{
	public List<Road> HorizontalRoads { get; private set; }
	public List<Road> VerticalRoads { get; private set; }

	/// <summary>
	/// The individual building areas.
	/// </summary>
	public List<Rect> BuildingSpaces { get; private set; }
	/// <summary>e
	/// The individual open spaces in the block.
	/// </summary>
	public List<Rect> OpenSpaces { get; private set; }


	public Rect BlockArea = new Rect(-100.0f, -100.0f, 200.0f, 200.0f);

	public float RoadWidthBase = 50.0f,
				 RoadWidthVariance = 10.0f;

	public Vector2 BuildingSizeBase = new Vector2(200.0f, 200.0f),
				   BuildingSizeVariance = new Vector2(75.0f, 75.0f);

	public int Seed = 42;

	public float EmptySpaceChance = 0.25f;


	public BlockLayoutGenerator()
	{
		HorizontalRoads = new List<Road>();
		VerticalRoads = new List<Road>();
		BuildingSpaces = new List<Rect>();
		OpenSpaces = new List<Rect>();
	}
	public BlockLayoutGenerator(Rect blockArea, float roadWidthBase, float roadWidthVariance,
								Vector2 buildingSizeBase, Vector2 buildingSizeVariance,
								int seed, float emptySpaceChance) : this()
	{
		BlockArea = blockArea;
		RoadWidthBase = roadWidthBase;
		RoadWidthVariance = roadWidthVariance;
		BuildingSizeBase = buildingSizeBase;
		BuildingSizeVariance = buildingSizeVariance;
		Seed = seed;
		EmptySpaceChance = emptySpaceChance;
	}


	/// <summary>
	/// Generates buidings in the block and stores the resulting data in
	/// "HorizontalRoads", "VerticalRoads", "BuildingSpaces", and "OpenSpaces".
	/// </summary>
	public void Generate()
	{
		HorizontalRoads.Clear();
		VerticalRoads.Clear();
		BuildingSpaces.Clear();
		OpenSpaces.Clear();

		Random.seed = Seed;


		//Generate the width/XPos of each building and width/pos of each vertical road.
		List<float> buildingWidths = new List<float>(),
					buildingXPoses = new List<float>();
		float counterX = BlockArea.xMin,
			  spaceLeft = BlockArea.width;
		while (spaceLeft >= BuildingSizeBase.x - BuildingSizeVariance.x)
		{
			//Start with a building.
			buildingXPoses.Add(counterX);
			buildingWidths.Add(Mathf.Lerp(BuildingSizeBase.x - BuildingSizeVariance.x,
										  Mathf.Min(BuildingSizeBase.x + BuildingSizeVariance.x,
													spaceLeft),
										  Random.value));
			spaceLeft -= buildingWidths[buildingWidths.Count - 1];
			counterX += buildingWidths[buildingWidths.Count - 1];

			//Next, create a road if there's enough space for a building after it.
			float roadWidth = Mathf.Lerp(RoadWidthBase - RoadWidthVariance,
										 RoadWidthBase + RoadWidthVariance,
										 Random.value);
			spaceLeft -= roadWidth;
			if (spaceLeft < BuildingSizeBase.x - BuildingSizeVariance.x)
			{
				//Push out the last building space to hit the edge.
				buildingWidths[buildingWidths.Count - 1] += spaceLeft + roadWidth;
				break;
			}
			counterX += roadWidth * 0.5f;
			VerticalRoads.Add(new Road(counterX, roadWidth));
			counterX += roadWidth * 0.5f;
		}

		//Now do the same thing with building height/YPos and width/pos of horizontal roads.
		List<float> buildingHeights = new List<float>(),
					buildingYPoses = new List<float>();
		float counterY = BlockArea.yMin;
		spaceLeft = BlockArea.height;
		while (spaceLeft >= BuildingSizeBase.y - BuildingSizeVariance.y)
		{
			buildingYPoses.Add(counterY);
			buildingHeights.Add(Mathf.Lerp(BuildingSizeBase.y - BuildingSizeVariance.y,
										   Mathf.Min(BuildingSizeBase.y + BuildingSizeVariance.y,
													 spaceLeft),
										   Random.value));
			spaceLeft -= buildingHeights[buildingHeights.Count - 1];
			counterY += buildingHeights[buildingHeights.Count - 1];

			float roadWidth = Mathf.Lerp(RoadWidthBase - RoadWidthVariance,
										 RoadWidthBase + RoadWidthVariance,
										 Random.value);
			spaceLeft -= roadWidth;
			if (spaceLeft < BuildingSizeBase.x - BuildingSizeVariance.x)
			{
				//Push out the last building space to hit the edge.
				buildingHeights[buildingHeights.Count - 1] += spaceLeft + roadWidth;
				break;
			}
			counterY += roadWidth * 0.5f;
			HorizontalRoads.Add(new Road(counterY, roadWidth));
			counterY += roadWidth * 0.5f;
		}

		//Now create the buildings.
		for (int x = 0; x < buildingWidths.Count; ++x)
			for (int y = 0; y < buildingHeights.Count; ++y)
				BuildingSpaces.Add(new Rect(buildingXPoses[x], buildingYPoses[y],
											buildingWidths[x], buildingHeights[y]));

		//Finally, convert some of the buildings into empty spaces.
		for (int i = BuildingSpaces.Count - 1; i >= 0; --i)
		{
			if (Random.value < EmptySpaceChance)
			{
				OpenSpaces.Add(BuildingSpaces[i]);
				BuildingSpaces.RemoveAt(i);
			}
		}
	}
}