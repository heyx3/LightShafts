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


				if (true)
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
					int posX, posY;


					//If this point is on an edge, use special tracer placement.
					bool onBottomEdge = (y == 0),
						 onTopEdge = (y == Generator.NPointsY - 1),
						 onLeftEdge = (x == 0),
						 onRightEdge = (x == Generator.NPointsX - 1);
					if (onBottomEdge)
					{
						//Is this point on a bottom corner?
						if (onLeftEdge || onRightEdge)
						{
							int xEnd = (onLeftEdge ? 0 : Generator.NPointsX * Generator.GridSizeX);
							tracerPoses.Add(new Vector2i(xEnd, 0));

							//Trace upwards and horizontally outwards.

							Vector2 targetPos = (Generator.CellPoints[x, y + 1] + Generator.CellPoints[x, y]) * 0.5f;
							targetPos.x = xEnd;
							Vector2 toTarget = (targetPos - Generator.CellPoints[x, y]).normalized;
							Vector2 tracerPos = Generator.CellPoints[x, y] + (toTarget * distFromPoint);

							posX = Mathf.Clamp((int)tracerPos.x, 0, NoiseSizeX);
							posY = Mathf.Clamp((int)tracerPos.y, 0, NoiseSizeY);

							RunTracer(noise, ref posX, ref posY);
							tracerPoses.Add(new Vector2i(posX, posY));


							//Trace downwards and horizontally inwards.

							int inDir = (onLeftEdge ? 1 : -1);
							targetPos = (Generator.CellPoints[x + inDir, y] + Generator.CellPoints[x, y]) * 0.5f;
							toTarget = (targetPos - Generator.CellPoints[x, y]).normalized;
							tracerPos = Generator.CellPoints[x, y] + (toTarget * distFromPoint);
							
							posX = Mathf.Clamp((int)tracerPos.x, 0, NoiseSizeX);
							posY = Mathf.Clamp((int)tracerPos.y, 0, NoiseSizeY);

							RunTracer(noise, ref posX, ref posY);
							tracerPoses.Add(new Vector2i(posX, posY));


							//TODO: Trace the midpoint of the inner upward corner and the adjacent edges.
						}
						//Otherwise, it's just on the bottom edge.
						else
						{
							//TODO: Implement.
						}
					}
					else if (onTopEdge)
					{
						//TODO: Finsh.

						//Top-left corner.
						if (onLeftEdge || onRightEdge)
						{

						}
						//Otherwise, just on the top edge.
						else
						{

						}
					}
					//Is it on the left edge?
					else if (onLeftEdge)
					{
						//TODO: Implement.
					}
					//Is it on the right edge.
					else if(onRightEdge)
					{
						//TODO: Implement.
					}
					
					//Otherwise, iterate through all four "corners" relative to this point
					//    and trace towards the midpoint of the corner and each adjacent edge.
					for (int yDir = -1; yDir <= 1; yDir += 2)
					{
						for (int xDir = -1; xDir <= 1; xDir += 2)
						{
							//Put down a tracer moving between this corner and either edge next to it.
							Vector2 targetPos = (Generator.CellPoints[x + xDir, y] +
												 Generator.CellPoints[x + xDir, y + yDir]) *
												0.5f,
									toTarget = (targetPos - Generator.CellPoints[x, y]).normalized;
							Vector2 tracerPos = Generator.CellPoints[x, y] + (toTarget * distFromPoint);

							posX = (int)tracerPos.x - Mathf.Clamp(xDir, -1, 0);
							posY = (int)tracerPos.y - Mathf.Clamp(yDir, -1, 0);

							RunTracer(noise, ref posX, ref posY);
							tracerPoses.Add(new Vector2i(posX, posY));
						}
					}
				}
				else
				{
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
	private void RunTracer(float[,] noise, ref int posX, ref int posY)
	{
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
	}
}