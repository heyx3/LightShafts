﻿using UnityEngine;


/// <summary>
/// Handles an entity's motion.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class MovementHandler : MonoBehaviour
{
	public float Accel = 100.0f,
				 MaxSpeed = 500.0f;

	
	/// <summary>
	/// Set this value to indicate the direction this entity should accelerate towards.
	/// This vector's magnitude scales the acceleration.
	/// </summary>
	public Vector2 MovementInput { get; set; }

	public Transform MyTransform { get; private set; }
	public Rigidbody2D MyRigidbody { get; private set; }


	void Awake()
	{
		MyTransform = transform;
		MyRigidbody = rigidbody2D;
	}
	void FixedUpdate()
	{
		//Keep speed below the max.
		if (MyRigidbody.velocity.sqrMagnitude > (MaxSpeed * MaxSpeed))
			MyRigidbody.velocity = MyRigidbody.velocity.normalized * MaxSpeed;

		//Set the acceleration.
		MyRigidbody.AddForce(MovementInput * Accel);
	}
}