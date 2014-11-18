using UnityEngine;


/// <summary>
/// An entity that has health and takes damage from projectiles.
/// When its health reaches 0, a certain other component gets activated.
/// </summary>
public class HealthComponent : BaseShootableObject
{
	public float Health = 10.0f;
	public MonoBehaviour ActivateWhenKilled;


	public override void OnShot(Projectile projectile, Collision2D collsion)
	{
		Health -= projectile.Damage;
	}

	void Update()
	{
		if (Health <= 0.0f)
		{
			ActivateWhenKilled.enabled = true;
		}
	}
}