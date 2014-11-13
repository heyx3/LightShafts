using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Tests pathing by constantly trying to move to the player's nearest path node.
/// </summary>
[RequireComponent(typeof(MovementHandler))]
public class PathTesterEntity : MonoBehaviour
{
	/// <summary>
	/// The node this instance is trying to get to.
	/// </summary>
	public NavNode Destination { get { return PlayerInput.Instance.MyMovement.ClosestNode.MyNode; } }


	public float PathErrorSqr = 10.0f;
	public float PathUpdateFrequency = 1.0f;

	private MovementHandler mvt;
	private Transform tr;

	private PathFinder<NavNode> pather;
	private List<NavNode> path = new List<NavNode>();


	void Awake()
	{
		tr = transform;
	}
	void Start()
	{
		mvt = GetComponent<MovementHandler>();
		pather = new PathFinder<NavNode>(NavGraphComponent.Graph, (n1, n2) => new NavEdge(n1, n2));

		StartCoroutine(UpdatePathCoroutine());
	}
	void Update()
	{
		Vector2 myPos = (Vector2)tr.position;

		//If the current path node has been reached, remove it from the path.
		if (path.Count > 0 && Vector2.SqrMagnitude(path[0].Pos - myPos) <= PathErrorSqr)
		{
			path.RemoveAt(0);
		}

		//If the path is empty, just move towards the player.
		if (path.Count == 0)
		{
			mvt.MovementInput = ((Vector2)PlayerInput.Instance.MyTransform.position - myPos).normalized;
		}
		//Otherwise, move towards the next part of the path.
		else
		{
			mvt.MovementInput = (path[0].Pos - myPos).normalized;
		}


		//Always face the player.
		Vector2 dir = mvt.MovementInput;
		float ang = Mathf.Atan2(dir.y, dir.x);
		tr.eulerAngles = new Vector3(0.0f, 0.0f, ang * Mathf.Rad2Deg);
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.white;

		if (path.Count > 0)
		{
			Gizmos.DrawLine(tr.position, (Vector3)path[0].Pos);

			for (int i = 1; i < path.Count; ++i)
				Gizmos.DrawLine((Vector3)path[i - 1].Pos, (Vector3)path[i].Pos);
		}
		else if (Application.isPlaying)
		{
			Gizmos.DrawLine(tr.position, PlayerInput.Instance.MyTransform.position);
		}
	}

	
	private IEnumerator UpdatePathCoroutine()
	{
		if (mvt.ClosestNode != null)
		{
			//Calculate the path.
			pather.Start = mvt.ClosestNode.MyNode;
			pather.End = Destination;
			pather.FindPath();
			path = pather.CurrentPath;

			if (path.Count > 1)
			{
				//If the second node is in sight, ignore the first one.
				Vector2 dir = path[1].Pos - (Vector2)tr.position;
				float dirLen = dir.magnitude;
				RaycastHit2D castResult = mvt.CastRay(dir / dirLen, dirLen);
				if (castResult.collider == null)
				{
					path.RemoveAt(0);
				}
			}
		}

		//Wait a bit then update the path again.
		yield return new WaitForSeconds(PathUpdateFrequency);
		StartCoroutine(UpdatePathCoroutine());
	}
}