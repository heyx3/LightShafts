using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Represents mesh data for a road (positions, UVs, and indices).
/// </summary>
public class TileVertexList
{
	public List<Vector3> Poses = new List<Vector3>();
	public List<Vector2> UVs = new List<Vector2>();
	public List<int> Indices = new List<int>();
}