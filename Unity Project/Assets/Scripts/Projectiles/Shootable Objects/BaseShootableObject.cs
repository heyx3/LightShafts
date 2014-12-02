using UnityEngine;


/// <summary>
/// Abstract class for a component that reacts to getting shot.
/// The "OnShot" function is called by projectiles that hit it.
/// </summary>
public abstract class BaseShootableObject : MonoBehaviour
{
	/// <summary>
	/// IMPORTANT NOTE: This is autmoatically called by the projectile that hit this object.
	/// </summary>
	public abstract void OnShot(Projectile projectile, Collision2D collsion);
}