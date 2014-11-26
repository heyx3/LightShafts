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
		Vertices = vertices;
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
	//Some simple declarations of classes for the algorithm.
	private struct Vector2i { public int x, y; public Vector2i(int _x, int _y) { x = _x; y = _y; } }
	private class CompareZOfVectors : IComparer<Vector3>
	{
		public int Compare(Vector3 a, Vector3 b)
		{
			return (a.z < b.z) ? -1 : ((a.z == b.z) ? 0 : 1);
		}
	}


	public WorleyGenerator Generator = new WorleyGenerator(WorleyGenerator.DistanceCalcTypes.EuclideanSquared,
														   WorleyGenerator.ValueCalculators.Closest1, 128, 128,
														   new Vector2(0.6f, 0.6f), 42);

	public int NoiseSizeX = 512,
			   NoiseSizeY = 512;

	public int TraceRadius = 1;


	public List<VoroniCell> Generate()
	{
		List<VoroniCell> cells = new List<VoroniCell>();

		//Generate the noise.
		float[,] noise = new float[NoiseSizeX, NoiseSizeY];
		Generator.Generate(noise);

		//Analyze the noise.
		/*
		float min = float.MaxValue,
			  max = float.MinValue;
		for (int x = 0; x < NoiseSizeX; ++x)
			for (int y = 0; y < NoiseSizeY; ++y)
			{
				min = Mathf.Min(min, noise[x, y]);
				max = Mathf.Max(max, noise[x, y]);
			}
		for (int x = 0; x < NoiseSizeX; ++x)
			for (int y = 0; y < NoiseSizeY; ++y)
				noise[x, y] = Mathf.InverseLerp(min, max, noise[x, y]);
		*/

			//Each Worley grid cell can be mapped to a Voroni cell.
			cells.Capacity = Generator.NPointsX * Generator.NPointsY;

		//Generate each Voroni cell.
		Vector2 cellHalfSize = 0.5f * new Vector2(Generator.GridSizeX, Generator.GridSizeY);
		for (int x = 0; x < Generator.NPointsX; ++x)
		{
			for (int y = 0; y < Generator.NPointsY; ++y)
			{
				List<Vector2i> tracerPoses = new List<Vector2i>();

				int pixelX = (int)Generator.CellPoints[x, y].x,
					pixelY = (int)Generator.CellPoints[x, y].y;


				//if (true)
				if (tracerPoses != null)
				{
					//Add tracers that are likely to end up at a corner.

					//First, find the maximum distance that a position can be from this grid point
					//    without any chance of exiting the Voroni cell.
					Vector2 cellCenter = new Vector2((x + 0.5f) * Generator.GridSizeX,
												     (y + 0.5f) * Generator.GridSizeY);
					float distFromPoint = Mathf.Min((cellCenter.x + cellHalfSize.x) - Generator.CellPoints[x, y].x,
												    (cellCenter.y + cellHalfSize.y) - Generator.CellPoints[x, y].y,
													Generator.CellPoints[x, y].x - (cellCenter.x - cellHalfSize.x),
													Generator.CellPoints[x, y].y - (cellCenter.y - cellHalfSize.y));
					distFromPoint *= 0.5f;
					

					//Now put tracers at that distance from the point towards certain targets.

					//If this point is on an edge, use special tracer placement.
					bool onBottomEdge = (y == 0),
						 onTopEdge = (y == Generator.NPointsY - 1),
						 onLeftEdge = (x == 0),
						 onRightEdge = (x == Generator.NPointsX - 1);
					if (onBottomEdge || onTopEdge)
					{
						int yEnd = (onBottomEdge ? 0 : NoiseSizeY);

						//Is this point on a [top/bottom] corner?
						if (onLeftEdge || onRightEdge)
						{
							int xEnd = (onLeftEdge ? 0 : NoiseSizeX);
							tracerPoses.Add(new Vector2i(xEnd, yEnd));

							Vector2i towardsCenter = new Vector2i((onLeftEdge ? 1 : -1),
																  (onBottomEdge ? 1 : -1));

							//Trace vertically inwards and horizontally outwards.
							Vector2 targetPos = (Generator.CellPoints[x, y + towardsCenter.y] +
												 Generator.CellPoints[x, y]) *
												0.5f;
							tracerPoses.Add(RunTracer(noise, new Vector2i(x, y), targetPos, distFromPoint));


							//Trace vertically outwards and horizontally inwards.
							targetPos = (Generator.CellPoints[x + towardsCenter.x, y] +
										 Generator.CellPoints[x, y]) * 0.5f;
							targetPos.y = yEnd;
							tracerPoses.Add(RunTracer(noise, new Vector2i(x, y), targetPos, distFromPoint));


							//Trace the midpoint of the inner [downward/upward] corner and the adjacent edges.

							targetPos = (Generator.CellPoints[x, y + towardsCenter.y] +
										 Generator.CellPoints[x + towardsCenter.x, y + towardsCenter.y]) *
										0.5f;
							tracerPoses.Add(RunTracer(noise, new Vector2i(x, y), targetPos, distFromPoint));

							targetPos = (Generator.CellPoints[x + towardsCenter.x, y + towardsCenter.y] +
										 Generator.CellPoints[x + towardsCenter.x, y]) *
										0.5f;
							tracerPoses.Add(RunTracer(noise, new Vector2i(x, y), targetPos, distFromPoint));
						}
						//Otherwise, it's just on the [top/bottom] edge.
						else
						{
							int towardsCenterY = (onBottomEdge ? 1 : -1);

							//For both "corners" diagonally adjacent to this point,
							//    trace towards the midpoint of that corner and both adjacent edges.
							for (int xDir = -1; xDir <= 1; xDir += 2)
							{
								//Put down a tracer moving between this corner and either edge next to it.

								Vector2 targetPos = (Generator.CellPoints[x + xDir, y] +
													 Generator.CellPoints[x + xDir, y + towardsCenterY]) *
													0.5f;
								tracerPoses.Add(RunTracer(noise, new Vector2i(x, y), targetPos, distFromPoint));

								targetPos = (Generator.CellPoints[x + xDir, y + towardsCenterY] +
											 Generator.CellPoints[x, y + towardsCenterY]) *
											0.5f;
								tracerPoses.Add(RunTracer(noise, new Vector2i(x, y), targetPos, distFromPoint));
							}
						}
					}
					//Is it on the left/right edge?
					else if (onLeftEdge || onRightEdge)
					{
						int towardsCenterX = (onLeftEdge ? 1 : -1);

						//For both "corners" diagonally adjacent to this point,
						//    trace towards the midpoint of that corner and both adjacent edges.
						for (int yDir = 1; yDir <= 1; yDir += 2)
						{
							//Put down a tracer moving between this corner and either edge next to it.

							Vector2 targetPos = (Generator.CellPoints[x + towardsCenterX, y] +
												 Generator.CellPoints[x + towardsCenterX, y + yDir]) *
												0.5f;
							tracerPoses.Add(RunTracer(noise, new Vector2i(x, y), targetPos, distFromPoint));

							targetPos = (Generator.CellPoints[x + towardsCenterX, y + yDir] +
										 Generator.CellPoints[x, y + yDir]) *
										0.5f;
							tracerPoses.Add(RunTracer(noise, new Vector2i(x, y), targetPos, distFromPoint));
						}
					}
					//Otherwise, iterate through all four "corners" diagonally adjacent to this point
					//    and trace towards the midpoint of that corner and both adjacent edges.
					else for (int yDir = -1; yDir <= 1; yDir += 2)
					{
						for (int xDir = -1; xDir <= 1; xDir += 2)
						{
							//Put down a tracer moving between this corner and either edge next to it.

							Vector2 targetPos = (Generator.CellPoints[x + xDir, y] +
												 Generator.CellPoints[x + xDir, y + yDir]) *
												0.5f;
							tracerPoses.Add(RunTracer(noise, new Vector2i(x, y), targetPos, distFromPoint));
							

							targetPos = (Generator.CellPoints[x, y + yDir] +
										 Generator.CellPoints[x + xDir, y + yDir]) *
										0.5f;
							tracerPoses.Add(RunTracer(noise, new Vector2i(x, y), targetPos, distFromPoint));
						}
					}
				}
				else
				{
					/*
					//Add tracers above/below the point.
					for (int traceX = pixelX - TraceRadius; traceX <= pixelX + TraceRadius; ++traceX)
					{
						if (traceX >= 0 && traceX < NoiseSizeX)
						{
							int traceY1 = pixelY - TraceRadius,
								traceY2 = pixelY + TraceRadius;

							if (traceY1 >= 0)
							{
								int posX = traceX,
									posY = traceY1;

								RunTracer(noise, ref posX, ref posY);
								tracerPoses.Add(new Vector2i(posX, posY));
							}
							if (traceY2 < NoiseSizeY)
							{
								int posX = traceX,
									posY = traceY2;

								RunTracer(noise, ref posX, ref posY);
								tracerPoses.Add(new Vector2i(posX, posY));
							}
						}
					}

					//Add tracers to the left/right of the point.
					for (int traceY = pixelY - TraceRadius + 1; traceY <= pixelY + TraceRadius - 1; ++traceY)
					{
						if (traceY >= 0 && traceY < NoiseSizeY)
						{
							int traceX1 = pixelX - TraceRadius,
								traceX2 = pixelX + TraceRadius;

							if (traceX1 >= 0)
							{
								int posX = traceX1,
									posY = traceY;

								RunTracer(noise, ref posX, ref posY);
								tracerPoses.Add(new Vector2i(posX, posY));
							}
							if (traceX2 < NoiseSizeX)
							{
								int posX = traceX2,
									posY = traceY;

								RunTracer(noise, ref posX, ref posY);
								tracerPoses.Add(new Vector2i(posX, posY));
							}
						}
					}
					 */
				}

				//Remove any tracers that ended up in the same location as another tracer.
				for (int i = 0; i < tracerPoses.Count; ++i)
				{
					for (int j = i + 1; j < tracerPoses.Count; ++j)
					{
						if (tracerPoses[i].x == tracerPoses[j].x && tracerPoses[i].y == tracerPoses[j].y)
						{
							tracerPoses.RemoveAt(j);
							j -= 1;
						}
					}
				}

				//Sort the tracer positions so that they are ordered clockwise around the center.
				List<Vector3> tracerPosesAndAngle = tracerPoses.ConvertAll<Vector3>(vi =>
					{
						float vx = (float)vi.x,
							  vy = (float)vi.y;
						return new Vector3(vx, vy, AngleCalculations.GetAngle(new Vector2(vx, vy) -
																			  Generator.CellPoints[x, y]));
					});
				tracerPosesAndAngle.Sort(new CompareZOfVectors());


				//Make a Voroni cell out of the ordered positions.
				cells.Add(new VoroniCell(tracerPosesAndAngle.ConvertAll(v3 => new Vector2(v3.x, v3.y))));
			}
		}

		return cells;
	}
	/// <summary>
	/// Pushes a "tracer" roughly from the point in the given grid cell towards the given target starting position until
	/// it reaches a local maximum in the noise grid. Returns its final resting position in the noise grid.
	/// </summary>
	private Vector2i RunTracer(float[,] noise, Vector2i cellPointLoc, Vector2 targetStartPos, float maxDistFromPoint)
	{
		Vector2 toTarget = (targetStartPos - Generator.CellPoints[cellPointLoc.x, cellPointLoc.y]).normalized,
				tracerPos = Generator.CellPoints[cellPointLoc.x, cellPointLoc.y] +
							(toTarget * maxDistFromPoint);

		int posX = Mathf.Clamp((int)tracerPos.x, 0, NoiseSizeX),
			posY = Mathf.Clamp((int)tracerPos.y, 0, NoiseSizeY);


		//Keep climbing up until the highest-possible value is found.
		while (true)
		{
			float lessX = (posX > 0 ? noise[posX - 1, posY] : float.MinValue),
				  moreX = (posX < (noise.GetLength(0) - 1) ? noise[posX + 1, posY] : float.MinValue),
				  lessY = (posY > 0 ? noise[posX, posY - 1] : float.MinValue),
				  moreY = (posY < (noise.GetLength(1) - 1) ? noise[posX, posY + 1] : float.MinValue);
			float current = noise[posX, posY];

			if (lessX > current && lessX >= moreX && lessX >= lessY && lessX >= moreY)
			{
				posX -= 1;
			}
			else if (moreX > current && moreX >= lessX && moreX >= lessY && moreX >= moreY)
			{
				posX += 1;
			}
			else if (lessY > current && lessY >= lessX && lessY >= moreX && lessY >= moreY)
			{
				posY -= 1;
			}
			else if (moreY > current && moreY >= lessX && moreY >= moreX && moreY >= lessY)
			{
				posY += 1;
			}
			else
			{
				break;
			}
		}

		return new Vector2i(posX, posY);
	}
}