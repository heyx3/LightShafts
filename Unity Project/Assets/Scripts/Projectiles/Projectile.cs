using UnityEngine;


/// <summary>
/// A Rigidbody2D that destroys itself upon hitting any solid object.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
	public float Damage = 0.5f;
	public GameObject DroppedAmmoPrefab;

	public float FlightTime = 0.65f;
	public bool DropAmmoOnDestroyed = true;


	private float timeAlive = 0.0f;


	void Update()
	{
		timeAlive += Time.deltaTime;

		if (timeAlive > FlightTime)
		{
			if (DropAmmoOnDestroyed)
			{
				Transform dropped = ((GameObject)Instantiate(DroppedAmmoPrefab)).transform;
				dropped.position = transform.position;
				dropped.rotation = transform.rotation;
			}

			Destroy(gameObject);
		}
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		if (DropAmmoOnDestroyed)
		{
			SingleAmmo embedded = ((GameObject)Instantiate(DroppedAmmoPrefab)).GetComponent<SingleAmmo>();
			embedded.EmbedInCollidedObject(collision, transform.rotation);
		}

		Destroy(gameObject);
	}
}