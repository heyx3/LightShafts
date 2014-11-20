using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// An axis-aligned bounding box that blocks light.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class LightBlockerAABB : LightBlockerBase
{
	public override Rect Bounds
	{
		get
		{
			Bounds b = box.bounds;
			return new Rect(b.min.x, b.min.y, b.size.x, b.size.y);
		}
	}
	private BoxCollider2D box;


	public override void GetBlockingSegments(Vector2 lightPos, float lightRadius,
											 List<LightSource.Segment> segments)
	{
		//Only bother adding segments that are facing the light source.
		Bounds b = box.bounds;
		Vector2 min = new Vector2(b.min.x, b.min.y),
				max = new Vector2(b.max.x, b.max.y);

		//Left segment.
		if (lightPos.x < min.x)
			segments.Add(new LightSource.Segment(min, new Vector2(min.x, max.y), lightPos));
		//Right segment.
		if (lightPos.x > max.x)
			segments.Add(new LightSource.Segment(new Vector2(max.x, min.y), max, lightPos));

		//Top segment.
		if (lightPos.y < min.y)
			segments.Add(new LightSource.Segment(min, new Vector2(max.x, min.y), lightPos));
		//Bottom segment.
		if (lightPos.y > max.y)
			segments.Add(new LightSource.Segment(new Vector2(min.x, max.y), max, lightPos));
	}


	void Awake()
	{
		box = GetComponent<BoxCollider2D>();
	}
}