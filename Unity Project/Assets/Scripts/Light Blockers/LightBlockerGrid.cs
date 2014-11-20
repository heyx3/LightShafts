using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// A space-partitioning grid for light-blocking objects.
/// Each object could have several positions in the grid because it overlaps several grid cells.
/// The grid is centered at this object's position and should have an odd number of elements.
/// </summary>
public class LightBlockerGrid : MonoBehaviour
{
	/// <summary>
	/// A specific grid cell.
	/// </summary>
	public struct GridLocation
	{
		public int x, y; 
		public GridLocation(int _x, int _y) { x = _x; y = _y; }
		public override string ToString() { return "{" + x.ToString() + ", " + y.ToString() + "}"; }
	}
	/// <summary>
	/// A rectangular region of grid cells.
	/// </summary>
	public struct GridSpace
	{
		public GridLocation Min, Max;
		public GridSpace(GridLocation min, GridLocation max) { Min = min; Max = max; }
		public override string ToString() { return "[" + Min.ToString() + ", " + Max.ToString() + "]"; }
	}


	public static LightBlockerGrid Instance { get; private set; }
	public static List<LightBlockerBase>[,] Grid { get; private set; }


	public Vector2 GridCenter { get; private set; }


	public int NRows = 11,
			   NColumns = 11;
	public float GridSize = 1000.0f;


	/// <summary>
	/// Gets the grid location the given position is in.
	/// </summary>
	public GridLocation GetPosition(Vector2 pos)
	{
		//Transform the position so that the first grid element's min corner is at the origin.

		pos -= GridCenter;
		pos -= new Vector2(GridSize, GridSize) * 0.5f;

		return new GridLocation((int)(pos.x / GridSize) + (NColumns / 2),
								(int)(pos.y / GridSize) + (NRows / 2));
	}
	/// <summary>
	/// Calculates the grid space that the given area occupies.
	/// </summary>
	public GridSpace CalculateGridSpace(Rect area)
	{
		return new GridSpace(GetPosition(area.min), GetPosition(area.max));
	}

	/// <summary>
	/// Adds the given blocker to the given grid cells.
	/// </summary>
	public void AddBlocker(LightBlockerBase blocker, GridSpace space)
	{
		for (int x = space.Min.x; x <= space.Max.x; ++x)
			for (int y = space.Min.y; y <= space.Max.y; ++y)
				Grid[x, y].Add(blocker);
	}
	/// <summary>
	/// Removes the given blocker from the given grid cells.
	/// </summary>
	public void RemoveBlocker(LightBlockerBase blocker, GridSpace space)
	{
		for (int x = space.Min.x; x <= space.Max.x; ++x)
			for (int y = space.Min.y; y <= space.Max.y; ++y)
				Grid[x, y].Remove(blocker);
	}
	/// <summary>
	/// Moves the given blocker from the given old cells to the given new cells.
	/// </summary>
	public void MoveBlocker(LightBlockerBase blocker, GridSpace oldSpace, GridSpace newSpace)
	{
		RemoveBlocker(blocker, oldSpace);
		AddBlocker(blocker, newSpace);
	}


	void Awake()
	{
		if (Instance != null)
			Debug.LogError("More than one 'LightBlockerGrid'! The first is in '" + gameObject.name +
						       "' and the second is in '" + Instance.gameObject.name + "'.");
		Instance = this;
		
		if (NRows % 2 == 0 || NColumns % 2 == 0)
				Debug.LogError("'LightBlockerGrid' requires an odd number of rows and columns!");

		Grid = new List<LightBlockerBase>[NColumns, NRows];
		for (int x = 0; x < NColumns; ++x)
			for (int y = 0; y < NRows; ++y)
				Grid[x, y] = new List<LightBlockerBase>();

		GridCenter = transform.position;
	}
	void OnDrawGizmos()
	{
		Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.25f);

		Vector2 gridCenter = transform.position;
		
		for (int x = -NColumns / 2; x <= NColumns / 2; ++x)
		{
			float xPos = gridCenter.x - ((float)x * GridSize);
			for (int y = -NRows / 2; y <= NRows / 2; ++y)
			{
				float yPos = gridCenter.y - ((float)y * GridSize);
				Gizmos.DrawWireCube(new Vector3(xPos, yPos, 0.0f),
									new Vector3(GridSize, GridSize, 0.1f));
			}
		}
	}
}