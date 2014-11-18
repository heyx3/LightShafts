using UnityEngine;


/// <summary>
/// A light source that stays attached to an object.
/// </summary>
[RequireComponent(typeof(LightSource))]
public class AuraLight : MonoBehaviour
{
	public Transform Parent;

	private Transform tr;


	void Awake()
	{
		tr = transform;

		if (Parent == null)
			Debug.LogError("AuraLight '" + gameObject.name + "' doesn't have a 'Parent' field!");
	}

	void Update()
	{
		tr.position = Parent.position;
	}
}