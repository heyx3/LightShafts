using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Generates Worley Noise/Voroni Cells for the city generator.
/// The algorithm works by generating a grid of points, randomizing each point's position a bit,
///     then generating float values on a smaller grid of "pixels"
///     based on each pixel's distance to the closest three generated points.
/// </summary>
[System.Serializable]
public class WorleyGenerator
{
	/// <summary>
	/// Ways of calculating distance between points.
	/// </summary>
	public enum DistanceCalcTypes
	{
		Euclidean,
		EuclideanSquared,
		Manhattan,
		MaxComponent,
		MinComponent,
	}
	

	/// <summary>
	/// Ways of calculating the final value at a pixel.
	/// </summary>
	public enum ValueCalculators
	{
		Closest1,
		Closest2,
		Closest3,

		Closest1_Plus_Closest2,
		Closest1_Plus_Closest3,
		Closest1_Minus_Closest2,
		Closest1_Minus_Closest3,

		Closest1_Plus_Closest2_Plus_Closest3,
		Closest1_Plus_Closest2_Minus_Closest3,
		Closest1_Minus_Closest2_Plus_Closest3,
		Closest1_Minus_Closest2_Minus_Closest3,

		Closest1_Times_Closest2,
		Closest1_Times_Closest3,
		Closest2_Times_Closest3,
		Closest1_Times_Closest2_Times_Closest3,
		
		Closest1_DividedBy_Closest2,
		Closest1_DividedBy_Closest3,
		Closest2_DividedBy_Closest1,
		Closest2_DividedBy_Closest3,
		Closest3_DividedBy_Closest1,
		Closest3_DividedBy_Closest2,
	}


	#region Convenient way to access each enum's corresponding function


	private delegate float DistanceCalc(Vector2 p1, Vector2 p2);
	private static Dictionary<DistanceCalcTypes, DistanceCalc> DistanceCalcLookup =
		new Dictionary<DistanceCalcTypes,DistanceCalc>()
	{
		{ DistanceCalcTypes.Euclidean, (p1, p2) => Vector2.Distance(p1, p2) },
		{ DistanceCalcTypes.EuclideanSquared, (p1, p2) => Vector2.SqrMagnitude(p1 - p2) },
		{ DistanceCalcTypes.Manhattan, (p1, p2) => (Mathf.Abs(p1.x - p2.x) + Mathf.Abs(p1.y - p2.y)) },
		{ DistanceCalcTypes.MaxComponent, (p1, p2) => Mathf.Max(Mathf.Abs(p1.x - p2.x), Mathf.Abs(p1.y - p2.y)) },
		{ DistanceCalcTypes.MinComponent, (p1, p2) => Mathf.Min(Mathf.Abs(p1.x - p2.x), Mathf.Abs(p1.y - p2.y)) },
	};


	private delegate float ValueCalc(float[] closestDistances);
	private static Dictionary<ValueCalculators, ValueCalc> ValueCalcLookup =
		new Dictionary<ValueCalculators, ValueCalc>()
	{
		{ ValueCalculators.Closest1, (ds) => (ds[0]) },
		{ ValueCalculators.Closest2, (ds) => (ds[1]) },
		{ ValueCalculators.Closest3, (ds) => (ds[2]) },
		{ ValueCalculators.Closest1_Plus_Closest2, (ds) => (ds[0] + ds[1]) },
		{ ValueCalculators.Closest1_Plus_Closest3, (ds) => (ds[0] + ds[2]) },
		{ ValueCalculators.Closest1_Minus_Closest2, (ds) => (ds[0] - ds[1]) },
		{ ValueCalculators.Closest1_Minus_Closest3, (ds) => (ds[0] - ds[2]) },
		{ ValueCalculators.Closest1_Plus_Closest2_Plus_Closest3, (ds) => (ds[0] + ds[1] + ds[2]) },
		{ ValueCalculators.Closest1_Plus_Closest2_Minus_Closest3, (ds) => (ds[0] + ds[1] - ds[2]) },
		{ ValueCalculators.Closest1_Minus_Closest2_Plus_Closest3, (ds) => (ds[0] - ds[1] + ds[2]) },
		{ ValueCalculators.Closest1_Minus_Closest2_Minus_Closest3, (ds) => (ds[0] - ds[1] - ds[2]) },
		{ ValueCalculators.Closest1_Times_Closest2, (ds) => (ds[0] * ds[1]) },
		{ ValueCalculators.Closest1_Times_Closest3, (ds) => (ds[0] * ds[2]) },
		{ ValueCalculators.Closest2_Times_Closest3, (ds) => (ds[1] * ds[2]) },
		{ ValueCalculators.Closest1_Times_Closest2_Times_Closest3, (ds) => (ds[0] * ds[1] * ds[2]) },
		{ ValueCalculators.Closest1_DividedBy_Closest2, (ds) => (ds[0] / ds[1]) },
		{ ValueCalculators.Closest1_DividedBy_Closest3, (ds) => (ds[0] / ds[2]) },
		{ ValueCalculators.Closest2_DividedBy_Closest1, (ds) => (ds[1] / ds[0]) },
		{ ValueCalculators.Closest2_DividedBy_Closest3, (ds) => (ds[1] / ds[2]) },
		{ ValueCalculators.Closest3_DividedBy_Closest1, (ds) => (ds[2] / ds[0]) },
		{ ValueCalculators.Closest3_DividedBy_Closest2, (ds) => (ds[2] / ds[1]) },
	};


	#endregion


	/// <summary>
	/// How to calculate the distance between two points.
	/// </summary>
	public DistanceCalcTypes DistanceCalculator = DistanceCalcTypes.Euclidean;
	/// <summary>
	/// How to calculate the final value of each pixel.
	/// </summary>
	public ValueCalculators ValueCalculator = ValueCalculators.Closest1;

	/// <summary>
	/// The length of each grid cell in pixels.
	/// </summary>
	public int GridSizeX = 25,
			   GridSizeY = 25;

	public int Seed = 42;

	/// <summary>
	/// The size of the region around the center of each grid cell in which the points will be generated.
	/// A value of 0 means that the points will be generated in the exact center. A value of 1 means that
	/// the points can be generated anywhere inside the grid cell, including the edges of the cell.
	/// </summary>
	public Vector2 GridPositionVarianceLerp = new Vector2(0.5f, 0.5f);


	public WorleyGenerator() { }
	public WorleyGenerator(DistanceCalcTypes distanceCalculator, ValueCalculators valueCalculator,
						   int gridSizeX, int gridSizeY, Vector2 gridPositionVarianceLerp, int seed)
	{
		DistanceCalculator = distanceCalculator;
		ValueCalculator = valueCalculator;
		GridSizeX = gridSizeX;
		GridSizeY = gridSizeY;
		GridPositionVarianceLerp = gridPositionVarianceLerp;
		Seed = seed;
	}


	/// <summary>
	/// Generates the Worley values and puts them into the given array.
	/// </summary>
	public void Generate(float[,] outValues)
	{
		Random.seed = Seed;

		int nPointsX = outValues.GetLength(0) / GridSizeX,
			nPointsY = outValues.GetLength(1) / GridSizeY;

		//First create a point in each grid cell.
		Vector2 variance = GridPositionVarianceLerp * 0.5f;
		Vector2[,] points = new Vector2[nPointsX, nPointsY];
		for (int x = 0; x < nPointsX; ++x)
			for (int y = 0; y < nPointsY; ++y)
			{
				Vector2 posLerp = new Vector2(Random.Range(0.5f - variance.x, 0.5f + variance.x),
											  Random.Range(0.5f - variance.y, 0.5f + variance.y));
				points[x, y] = new Vector2(Mathf.Lerp((float)(x * GridSizeX),
													  (float)((x + 1) * GridSizeX),
													  posLerp.x),
										   Mathf.Lerp((float)(y * GridSizeY),
													  (float)((y + 1) * GridSizeY),
													  posLerp.y));
			}


		//Store the three closest distances from each pixel to a point.

		float[] distances = new float[3];
		DistanceCalc distCalc = DistanceCalcLookup[DistanceCalculator];
		ValueCalc valCalc = ValueCalcLookup[ValueCalculator];

		for (int x = 0; x < outValues.GetLength(0); ++x)
		{
			float posX = (float)x + 0.5f;

			for (int y = 0; y < outValues.GetLength(1); ++y)
			{
				float posY = (float)y + 0.5f;

				Vector2 pos = new Vector2(posX, posY);

				//Empty the "distances" array.
				distances[0] = float.NaN;
				distances[1] = float.NaN;
				distances[2] = float.NaN;


				//Go through all nearby points and see which ones are closest.
				int gridPosX = x / GridSizeX,
					gridPosY = y / GridSizeY;
				for (int deltaGridX = -1; deltaGridX <= 1; ++deltaGridX)
				{
					int gridX = gridPosX + deltaGridX;
					if (gridX >= 0 && gridX < points.GetLength(0))
					{
						for (int deltaGridY = -1; deltaGridY <= 1; ++deltaGridY)
						{
							int gridY = gridPosY + deltaGridY;
							if (gridY >= 0 && gridY < points.GetLength(0))
							{
								//The grid cell being searched is a valid cell, so see how close its point is.
								float dist = distCalc(pos, points[gridX, gridY]);

								//Insert it into the correct spot in the collection of distances.
								if (float.IsNaN(distances[0]))
								{
									distances[0] = dist;
								}
								else if (distances[0] > dist)
								{
									distances[2] = distances[1];
									distances[1] = distances[0];
									distances[0] = dist;
								}
								else if (float.IsNaN(distances[1]))
								{
									distances[1] = dist;
								}
								else if (distances[1] > dist)
								{
									distances[2] = distances[1];
									distances[1] = dist;
								}
								else if (float.IsNaN(distances[2]) || distances[2] > dist)
								{
									distances[2] = dist;
								}
							}
						}
					}
				}

				//Now that the distances have been found, calculate the final value.
				outValues[x, y] = valCalc(distances);
			}
		}
	}
}