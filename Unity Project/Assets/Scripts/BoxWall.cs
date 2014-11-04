using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A wall that blocks the player and light.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class BoxWall : MonoBehaviour
{
	/// <summary>
	/// A statically-accessible reference to all walls.
	/// </summary>
	public static List<BoxWall> Walls = new List<BoxWall>();


	public Transform MyTransform { get; private set; }
	public BoxCollider2D Box { get; private set; }


	void Awake()
	{
		Box = GetComponent<BoxCollider2D>();
		MyTransform = transform;

		Walls.Add(this);
	}
	void OnDestroy()
	{
		Walls.Remove(this);
	}
}