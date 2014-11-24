using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Visualizes generated Voroni cells using Gizmos.
/// </summary>
public class VoroniVisualizer : MonoBehaviour
{
	public VoroniGenerator Generator = new VoroniGenerator();

	public int GizmoColorSeed = 42;
	public float VertexGizmoRadius = 10.0f;

	public bool ShouldGenerateNow = true;

	public bool[] ShouldDisplayCell = new bool[1] { false };


	private List<VoroniCell> cells = new List<VoroniCell>();


	void Update()
	{
		if (ShouldGenerateNow)
		{
			ShouldGenerateNow = false;

			cells = Generator.Generate();

			ShouldDisplayCell = new bool[cells.Count];
			for (int i = 0; i < ShouldDisplayCell.Length; ++i)
				ShouldDisplayCell[i] = false;
		}
	}
	void OnDrawGizmos()
	{
		Random.seed = GizmoColorSeed;

		foreach (VoroniCell cell in cells)
		{
			if (!ShouldDisplayCell[cells.IndexOf(cell)])
				continue;


			Gizmos.color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 0.25f);

			Gizmos.DrawSphere((Vector3)cell.Vertices[0], VertexGizmoRadius);
			for (int i = 1; i < cell.Vertices.Count; ++i)
			{
				Gizmos.DrawSphere((Vector3)cell.Vertices[i], VertexGizmoRadius);

				Gizmos.DrawLine((Vector3)cell.Vertices[i - 1], (Vector3)cell.Vertices[i]);
			}
			Gizmos.DrawLine((Vector3)cell.Vertices[cell.Vertices.Count - 1],
							(Vector3)cell.Vertices[0]);
		}
	}
}