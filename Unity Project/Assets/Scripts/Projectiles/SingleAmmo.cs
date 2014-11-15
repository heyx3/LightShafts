using UnityEngine;


/// <summary>
/// A single piece of ammo that the player picks up upon a collision.
/// Assumes this object's sprite is pointing to the right when not rotated.
/// Can be embedded into an object by calling "EmbedInCollidedObject()".
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class SingleAmmo : MonoBehaviour
{
	public Transform MyTransform
	{
		get
		{
			if (tr == null) tr = transform;
			return tr;
		}
	}
	private Transform tr = null;

	public SpriteRenderer MyRenderer
	{
		get
		{
			if (rnd == null) rnd = GetComponent<SpriteRenderer>();
			return rnd;
		}
	}
	private SpriteRenderer rnd = null;


	/// <summary>
	/// The distance this object sticks into a surface when embedded.
	/// </summary>
	public float StickInDistance = 10.0f;


	public void EmbedInCollidedObject(Collision2D collision, Quaternion rotation)
	{
		//Calculate the rough position/normal of the contact point.
		Vector2 avgContactPos = Vector2.zero;
		Vector2 avgContactNormal = Vector2.zero;
		for (int i = 0; i < collision.contacts.Length; ++i)
		{
			avgContactPos += collision.contacts[i].point;
			avgContactNormal += collision.contacts[i].normal;
		}
		avgContactPos /= (float)collision.contacts.Length;
		avgContactNormal /= (float)collision.contacts.Length;

		//Rotate this object to stick in the collided object.
		MyTransform.rotation = rotation;

		//Move this object to the point of collision with the other object.
		Bounds spriteBounds = MyRenderer.sprite.bounds;
		MyTransform.position = avgContactPos +
							   (avgContactNormal * (spriteBounds.extents.x - StickInDistance));
	}

	void OnCollisionEnter2D(Collision2D coll)
	{
		if (coll.gameObject == PlayerInput.Instance.gameObject)
		{
			PlayerInput.Instance.AmmoAmount += 1;
			Destroy(gameObject);
		}
	}
}