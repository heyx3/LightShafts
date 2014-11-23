using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A light blocker in the shape of an arbitrary polygon.
/// The assumptions made are:
/// 1) The polygon's edges don't intersect themselves.
/// 2) The polygon has at least three vertices.
/// 3) The polygon's vertices are specified in clockwise order.
/// </summary>
public class LightBlockerPolygon : LightBlockerBase
{
	public static Vector2 ClosestToLine(Vector2 p1, Vector2 p2, Vector2 otherP)
	{
		Vector2 p1ToP2 = p2 - p1,
				p1ToOtherP = otherP - p1;

		float t = Vector2.Dot(p1ToOtherP, p1ToP2) / p1ToP2.sqrMagnitude;

		if (t <= 0.0f) return p1;
		else if (t >= 1.0f) return p2;
		else return p1 + (t * p1ToP2);
	}


	public override Rect Bounds
	{
		get
		{
			return bnds;
		}
	}


	public List<Vector2> PointsObjectSpace = new List<Vector2>()
	{
		new Vector2(-250.0f, 0.0f),
		new Vector2(0.0f, 250.0f),
		new Vector2(250.0f, 0.0f),
		new Vector2(0.0f, -250.0f),
	};
	public float GizmoRadius = 50.0f;

	private Transform tr;
	private Rect bnds;
	private List<Vector2> transformed = new List<Vector2>(),
						  normalsUnNormalized = new List<Vector2>();


	void Awake()
	{
		tr = transform;

		ComputeGeometry();
	}
	protected override void OnUpdate()
	{
		if (!IsStatic || transformed.Count == 0)
		{
			ComputeGeometry();
		}
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		tr = transform;

		Matrix4x4 transformMat = tr.localToWorldMatrix;
		List<Vector2> worldPoints = PointsObjectSpace.ConvertAll(v =>
			{
				return (Vector2)transformMat.MultiplyPoint(v);
			});

		normalsUnNormalized.Clear();
		for (int i = 0; i < PointsObjectSpace.Count; ++i)
		{
			Vector2 toNext = worldPoints[(i + 1) % PointsObjectSpace.Count] - worldPoints[i];
			normalsUnNormalized.Add(new Vector2(-toNext.y, toNext.x));

			int next = (i + 1) % PointsObjectSpace.Count;
			Gizmos.DrawLine(worldPoints[i], worldPoints[i] + (GizmoRadius * 2.0f * normalsUnNormalized[i].normalized));
			Gizmos.DrawLine(worldPoints[next], worldPoints[next] + (GizmoRadius * 2.0f * normalsUnNormalized[i].normalized));
		}

		for (int i = 0; i < worldPoints.Count; ++i)
		{
			Gizmos.DrawSphere(worldPoints[i], GizmoRadius);
			Gizmos.DrawLine(worldPoints[i], worldPoints[(i + 1) % worldPoints.Count]);
		}
	}


	public override void GetBlockingSegments(Vector2 lightSource, float lightRadius,
											 List<LightSource.Segment> outSegments)
	{
		for (int i = 0; i < PointsObjectSpace.Count; ++i)
		{
			int next = (i + 1) % PointsObjectSpace.Count;

			Vector2 testPoint = ClosestToLine(transformed[i], transformed[next], lightSource);
			float dotted = Vector2.Dot(lightSource - testPoint, normalsUnNormalized[i]);
			if (dotted > 0.0f)
			{
				outSegments.Add(new LightSource.Segment(transformed[i], transformed[next], lightSource));
			}
		}
	}
	private void ComputeGeometry()
	{
		transformed.Clear();
		normalsUnNormalized.Clear();

		Matrix4x4 localToWorld = tr.localToWorldMatrix;
		Vector2 min = new Vector2(Single.MaxValue, Single.MaxValue),
				max = new Vector2(Single.MinValue, Single.MinValue);

		for (int i = 0; i < PointsObjectSpace.Count; ++i)
		{
			transformed.Add(localToWorld.MultiplyPoint(PointsObjectSpace[i]));

			min = Vector2.Min(min, transformed[i]);
			max = Vector2.Max(max, transformed[i]);
		}
		for (int i = 0; i < PointsObjectSpace.Count; ++i)
		{
			Vector2 toNext = transformed[(i + 1) % PointsObjectSpace.Count] - transformed[i];
			normalsUnNormalized.Add(new Vector2(-toNext.y, toNext.x));
		}

		bnds = new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
	}
}