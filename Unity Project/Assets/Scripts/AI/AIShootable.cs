using System;
using UnityEngine;


/// <summary>
/// An AI entity that can be shot.
/// </summary>
public class AIShootable : BaseShootableObject
{
	public float Health = 1.0f;

	public GameObject CreateOnDestroyed = null;

	private Vector2 DEBUGSPAWNSTART;
	

	void Awake()
	{
		DEBUGSPAWNSTART = (Vector2)transform.position;
	}
	void Update()
	{
		if (Health <= 0)
		{
			Transform leftOver = ((GameObject)Instantiate(CreateOnDestroyed)).transform;
			leftOver.position = transform.position;
			leftOver.rotation = transform.rotation;

			leftOver.position = (Vector3)DEBUGSPAWNSTART;
			leftOver.GetComponent<AIShootable>().DEBUGSPAWNSTART = DEBUGSPAWNSTART;
			leftOver.GetComponent<AIShootable>().Health = 1.0f;

			Destroy(gameObject);
		}
	}

	public override void OnShot(Projectile projectile, Collision2D collsion)
	{
		Health -= projectile.Damage;
	}
}