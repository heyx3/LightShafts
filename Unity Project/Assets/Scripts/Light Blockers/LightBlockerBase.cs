using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Some object that blocks light. This class uses Start(), Update(), and OnDestroy(),
/// so override the provided virtual functions to add behavior to those events.
/// </summary>
public abstract class LightBlockerBase : MonoBehaviour
{
	public LightBlockerGrid Instance { get { return LightBlockerGrid.Instance; } }


	/// <summary>
	/// This object's bounding box. Used for recalculating position in the grid.
	/// </summary>
	public abstract Rect Bounds { get; }
	/// <summary>
	/// This object's place in the light blocker grid.
	/// </summary>
	public LightBlockerGrid.GridSpace GridSpace { get; private set; }


	/// <summary>
	/// If true, it can be assumed that this blocker won't change in any way
	/// over the course of its lifetime (it can still be destroyed).
	/// </summary>
	public bool IsStatic = false;


	/// <summary>
	/// Gets all segments that might occlude the given light source
	/// and puts them into the given output collection.
	/// </summary>
	public abstract void GetBlockingSegments(Vector2 lightSource, float lightRadius,
											 List<LightSource.Segment> outSegments);

	protected virtual void OnStart() { }
	protected virtual void OnUpdate() { }
	protected virtual void OnDestroyed() { }


	void Start()
	{
		GridSpace = LightBlockerGrid.Instance.CalculateGridSpace(Bounds);
		LightBlockerGrid.Instance.AddBlocker(this, GridSpace);

		OnStart();
	}
	void Update()
	{
		if (!IsStatic)
		{
			LightBlockerGrid.GridSpace newSpace = LightBlockerGrid.Instance.CalculateGridSpace(Bounds);
			LightBlockerGrid.Instance.MoveBlocker(this, GridSpace, newSpace);
			GridSpace = newSpace;
		}

		OnUpdate();
	}
	void OnDestroy()
	{
		LightBlockerGrid.Instance.RemoveBlocker(this, GridSpace);

		OnDestroyed();
	}
}