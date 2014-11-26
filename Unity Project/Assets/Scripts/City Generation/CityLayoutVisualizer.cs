using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Draws the result of a city layout generation, using Gizmos.
/// </summary>
public class CityLayoutVisualizer : MonoBehaviour
{
	public CityLayoutGenerator Generator = new CityLayoutGenerator();

	public bool ShouldGenerateNow = true;

	public Color BlockColor = Color.white,
				 OpenSpaceColor = Color.white,
				 RoadColor = new Color(0.4f, 0.1f, 0.1f);
	public float RoadRadius = 50.0f;


	void Update()
	{
		if (ShouldGenerateNow)
		{
			ShouldGenerateNow = false;
			Generator.Generate();
		}
	}
	void OnDrawGizmos()
	{
		//The blocks.
		Gizmos.color = BlockColor;
		foreach (Rect block in Generator.Blocks)
			Gizmos.DrawCube(new Vector3(block.center.x, block.center.y, 0.0f),
							new Vector3(block.width, block.height, 0.1f));

		//The open spaces.
		Gizmos.color = OpenSpaceColor;
		foreach (Rect space in Generator.OpenSpaces)
			Gizmos.DrawCube(new Vector3(space.center.x, space.center.y, 0.0f),
							new Vector3(space.width, space.height, 0.1f));

		//The roads.
		Gizmos.color = RoadColor;
		foreach (KeyValuePair<Vector2, List<Vector2>> intersection in Generator.RoadConnections)
		{
			Gizmos.DrawSphere(new Vector3(intersection.Key.x, intersection.Key.y, 0.0f),
							  RoadRadius);

			foreach (Vector2 roadEnd in intersection.Value)
				Gizmos.DrawLine(new Vector3(intersection.Key.x, intersection.Key.y, 0.0f),
								new Vector3(roadEnd.x, roadEnd.y, 0.0f));
		}
	}
}