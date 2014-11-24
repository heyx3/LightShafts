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
/// The cells are found by running "tracers" from the center of each cell out towards its extents.
/// </summary>
[System.Serializable]
public class VoroniGenerator
{
	public WorleyGenerator Generator = new WorleyGenerator(WorleyGenerator.DistanceCalcTypes.EuclideanSquared,
														   WorleyGenerator.ValueCalculators.Closest1, 128, 128,
														   new Vector2(0.6f, 0.6f), 42);

	public int NoiseSizeX = 512,
			   NoiseSizeY = 512;

	public int TracersPerCell = 20;


	public List<VoroniCell> Generate()
	{
		List<VoroniCell> cells = new List<VoroniCell>();

		//Generate the noise.
		float[,] noise = new float[NoiseSizeX, NoiseSizeY];
		Generator.Generate(noise);

		//Each Worley grid cell can be mapped to a Voroni cell.
		cells.Capacity = Generator.NPointsX * Generator.NPointsY;

		//Generate each Voroni cell.
		for (int x = 0; x < Generator.NPointsX; ++x)
		{
			for (int y = 0; y < Generator.NPointsY; ++y)
			{
				List<Vector2> tracerPoses = new List<Vector2>();
				tracerPoses.Capacity = TracersPerCell;

				for (int i = 0; i < TracersPerCell; ++i)
				{
					int posX = (int)Generator.CellPoints[x, y].x,
						posY = (int)Generator.CellPoints[x, y].y;
					RunTracer(noise, ref posX, ref posY);
					tracerPoses.Add(new Vector2(posX, posY));
				}
			}
		}

		return cells;
	}
	private void RunTracer(float[,] noise, ref int posX, ref int posY)
	{
		//Keep climbing up until the highest-possible value is found.
		while (true)
		{
			float lessX = (posX > 0 ? noise[posX - 1, posY] : float.MinValue),
				  moreX = (posX < noise.GetLength(0) ? noise[posX + 1, posY] : float.MinValue),
				  lessY = (posY > 0 ? noise[posX, posY - 1] : float.MinValue),
				  moreY = (posY < noise.GetLength(1) ? noise[posX, posY + 1] : float.MinValue);
			float current = noise[posX, posY];

			if (lessX > current && lessX > moreX && lessX > lessY && lessX > moreY)
			{
				posX -= 1;
			}
			else if (moreX > current && moreX > lessX && moreX > lessY && moreX > moreY)
			{
				posX += 1;
			}
			else if (lessY > current && lessY > lessX && lessY > moreX && lessY > moreY)
			{
				posY -= 1;
			}
			else if (moreY > current && moreY > lessX && moreY > moreX && moreY > lessY)
			{
				posY += 1;
			}
			else
			{
				break;
			}
		}
	}
}