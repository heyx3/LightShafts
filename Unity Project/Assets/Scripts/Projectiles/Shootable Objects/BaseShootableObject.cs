using UnityEngine;


/// <summary>
/// Abstract class for a component that reacts to getting shot.
/// Has an "OnCollisionEnter2D" event method; child classes should
///     override "OnShot" and "OnOtherCollision".
/// </summary>
public abstract class BaseShootableObject : MonoBehaviour
{
	/// <summary>
	/// Called when this abstract class's "OnCollisionEnter2D" event is triggered by a projectile.
	/// </summary>
	public abstract void OnShot(Projectile projectile, Collision2D	collsion);

	/// <summary>
	/// Called when this abstract class's "OnCollisionEnter2D" event is triggered by
	///     something other than a projectile.
	/// Default behavior: do nothing.
	/// </summary>
	public virtual void OnOtherCollision(Collision2D collision) { }


	private void OnCollisionEnter2D(Collision2D collision)
	{
		Projectile proj = collision.collider.GetComponent<Projectile>();
		if (proj != null)
		{
			OnShot(proj, collision);
		}
		else
		{
			OnOtherCollision(collision);
		}
	}
}