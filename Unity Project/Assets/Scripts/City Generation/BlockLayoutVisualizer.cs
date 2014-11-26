using System;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CityLayoutVisualizer))]
public class BlockLayoutVisualizer : MonoBehaviour
{
	public BlockLayoutGenerator GeneratorTemplate = new BlockLayoutGenerator();

	public bool ShouldUpdateBlocksNow = false;

	public Color BuildingColor = Color.gray,
				 SpaceColor = Color.green,
				 RoadColor = Color.black;


	private CityLayoutVisualizer cityVisualizer;
	private List<BlockLayoutGenerator> blockGenerators = new List<BlockLayoutGenerator>();


	void Awake()
	{
		cityVisualizer = GetComponent<CityLayoutVisualizer>();
	}
	void Update()
	{
		if (ShouldUpdateBlocksNow)
		{
			ShouldUpdateBlocksNow = false;

			blockGenerators.Clear();
			foreach (Rect block in cityVisualizer.Generator.Blocks)
			{
				blockGenerators.Add(new BlockLayoutGenerator(block, GeneratorTemplate.RoadWidthBase,
															 GeneratorTemplate.RoadWidthVariance,
															 GeneratorTemplate.BuildingSizeBase,
															 GeneratorTemplate.BuildingSizeVariance,
															 UnityEngine.Random.Range(1, 3056231),
															 GeneratorTemplate.EmptySpaceChance));
				blockGenerators[blockGenerators.Count - 1].Generate();
			}
		}
	}
	void OnDrawGizmos()
	{
		foreach (BlockLayoutGenerator genBlock in blockGenerators)
		{
			Gizmos.color = RoadColor;
			foreach (CityLayoutGenerator.Road vertRoad in genBlock.VerticalRoads)
				Gizmos.DrawLine(new Vector3(vertRoad.Pos, genBlock.BlockArea.yMin, -10.0f),
								new Vector3(vertRoad.Pos, genBlock.BlockArea.yMax, -10.0f));
			foreach (CityLayoutGenerator.Road horzRoad in genBlock.HorizontalRoads)
				Gizmos.DrawLine(new Vector3(genBlock.BlockArea.xMin, horzRoad.Pos, -10.0f),
								new Vector3(genBlock.BlockArea.xMax, horzRoad.Pos, -10.0f));

			Gizmos.color = BuildingColor;
			foreach (Rect building in genBlock.BuildingSpaces)
				Gizmos.DrawCube(new Vector3(building.center.x, building.center.y, -10.0f),
								new Vector3(building.width, building.height, -10.0f));
			
			Gizmos.color = SpaceColor;
			foreach (Rect space in genBlock.OpenSpaces)
				Gizmos.DrawCube(new Vector3(space.center.x, space.center.y, -10.0f),
								new Vector3(space.width, space.height, -10.0f));
		}
	}
}