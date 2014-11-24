using System;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Represents a single cell in a Voroni diagram.
/// This cell must be convex.
/// </summary>
public class VoroniCell
{
	/// <summary>
	/// The vertices defining this cell's shape.
	/// </summary>
	public List<Vector2> Vertices;

	public VoroniCell(List<Vector2> vertices)
	{
		if (vertices.Count < 3)
			Debug.LogError("All voroni cell shapes must have at least three vertices! " +
						   "This one was given " + vertices.Count);
	}
}



/// <summary>
/// Generates a Voroni diagram using a Worley noise generator.
/// The noise is assumed to be composed entirely of convex polygons.
/// The generated diagram is defined by a collection of VoroniCells.
/// </summary>
[System.Serializable]
public class VoroniGenerator
{
	public WorleyGenerator Generator = new WorleyGenerator(WorleyGenerator.DistanceCalcTypes.EuclideanSquared,
														   WorleyGenerator.ValueCalculators.Closest1, 128, 128,
														   new Vector2(0.6f, 0.6f), 42);

	public int NoiseSizeX = 512,
			   NoiseSizeY = 512;


	public List<VoroniCell> Generate()
	{
		List<VoroniCell> cells = new List<VoroniCell>();

		//Generate the noise.
		float[,] noise = new float[NoiseSizeX, NoiseSizeY];
		Generator.Generate(noise);

		//Each Worley grid cell can be mapped to a Voroni cell.
		int nWorleyCellsX = NoiseSizeX / Generator.GridSizeX,
			nWorleyCellsY = NoiseSizeY / Generator.GridSizeY;
		cells.Capacity = nWorleyCellsX * nWorleyCellsY;


		//Generate each Voroni cell.

		for (int x = 0; x < nWorleyCellsX; ++x)
		{
			for (int y = 0; y < nWorleyCellsY; ++y)
			{
				//TODO: Finish.
			}
		}

		return cells;
	}
}