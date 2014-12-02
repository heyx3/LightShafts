using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Generates the full city from scratch.
/// </summary>
public class GeneratorComponent : MonoBehaviour
{
	public CityLayoutGenerator CityLayoutGen = new CityLayoutGenerator();
	public BlockLayoutGenerator BlockLayoutGen = new BlockLayoutGenerator();
	public BuildingGenerator BuildingGen = new BuildingGenerator();


	void Awake()
	{
		List<CityLayoutGenerator.Road> vertRoads = new List<CityLayoutGenerator.Road>(),
									   horzRoads = new List<CityLayoutGenerator.Road>();


		//Generate city.
		CityLayoutGen.Generate();
		vertRoads.AddRange(CityLayoutGen.VerticalRoads);
		horzRoads.AddRange(CityLayoutGen.HorizontalRoads);


		//Generate blocks.

		UnityEngine.Random.seed = BlockLayoutGen.Seed;
		
		List<Rect> buildings = new List<Rect>();
		foreach (Rect block in CityLayoutGen.Blocks)
		{
			BlockLayoutGenerator blockGen = new BlockLayoutGenerator(block, BlockLayoutGen.RoadWidthBase,
																	 BlockLayoutGen.RoadWidthVariance,
																	 BlockLayoutGen.BuildingSizeBase,
																	 BlockLayoutGen.BuildingSizeVariance,
																	 UnityEngine.Random.Range(0, 9999999),
																	 BlockLayoutGen.EmptySpaceChance);
			blockGen.Generate();

			buildings.AddRange(blockGen.BuildingSpaces);
			vertRoads.AddRange(blockGen.VerticalRoads);
			horzRoads.AddRange(blockGen.HorizontalRoads);
		}


		//Generate buildings.
		foreach (Rect building in buildings)
		{
			BuildingGenerator buildGen = new BuildingGenerator(building, BuildingGen);
			buildGen.GenerateBuilding();
		}


		//Generate roads.
		//TODO: Implement.
	}
}