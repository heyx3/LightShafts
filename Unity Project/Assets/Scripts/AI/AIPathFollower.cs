using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Directs this AI entity to calculate and follow a path.
/// If the target is null, this component doesn't push the entity anywhere.
/// </summary>
public class AIPathFollower : AIBaseComponent
{
	public float PathErrorMarginSqr = 10.0f;
	public float PathUpdateInterval = 0.6f,
				 PathUpdateIntervalVariance = 0.2f;


	public NavNode Target;
	private PathFinder<NavNode> pather;
	private List<NavNode> path = new List<NavNode>();


	void Start()
	{
		pather = new PathFinder<NavNode>(NavGraphComponent.Graph, (n1, n2) => new NavEdge(n1, n2));
		StartCoroutine(UpdatePathCoroutine());
	}
	void Update()
	{
		//If the target is missing, stop any and all pathing.
		if (Target == null)
		{
			path.Clear();
		}
		//Otherwise, apply path movement.
		else if (path.Count > 0)
		{
			Vector2 myPos = (Vector2)MyTransform.position;

			//If the current path node has been reached, remove it from the path.
			if (Vector2.SqrMagnitude(path[0].Pos - myPos) <= PathErrorMarginSqr)
			{
				path.RemoveAt(0);
			}

			//If there is still a path left after the current node was possibly just removed,
			//    path to the next node.
			if (path.Count > 0)
			{
				MyMovement.MovementInput += (path[0].Pos - myPos).normalized;
			}
		}
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.white;

		if (path.Count > 0)
		{
			Gizmos.DrawLine(MyTransform.position, (Vector3)path[0].Pos);
			for (int i = 1; i < path.Count; ++i)
				Gizmos.DrawLine((Vector3)path[i - 1].Pos, (Vector3)path[i].Pos);
		}
	}


	private IEnumerator UpdatePathCoroutine()
	{
		//If there is a clear start and end for the path, get the path.
		if (MyMovement.ClosestNode != null && Target != null)
		{
			//Calculate the path.
			pather.Start = MyMovement.ClosestNode.MyNode;
			pather.End = Target;
			pather.FindPath();
			path = pather.CurrentPath;

			//If the second nod is in sight, ignore the first one.
			if (path.Count > 1)
			{
				Vector2 dir = path[1].Pos - (Vector2)MyTransform.position;
				float dirLen = dir.magnitude;

				RaycastHit2D castResult = MyMovement.CastRay(dir / dirLen, dirLen);
				if (castResult.collider == null)
				{
					path.RemoveAt(0);
				}
			}
		}
		//Otherwise, there is no path.
		else
		{
			path.Clear();
		}

		yield return new WaitForSeconds(UnityEngine.Random.Range(PathUpdateInterval - PathUpdateIntervalVariance,
																 PathUpdateInterval + PathUpdateIntervalVariance));
		StartCoroutine(UpdatePathCoroutine());
	}
}