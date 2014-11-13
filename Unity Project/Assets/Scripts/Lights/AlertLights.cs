using System;
using UnityEngine;


public class AlertLights : MonoBehaviour
{
	public float RotSpeed;
	public System.Collections.Generic.List<Transform> ToRotate = new System.Collections.Generic.List<Transform>();


	void Update()
	{
		Vector3 rotAmount = new Vector3 (0.0f, 0.0f, RotSpeed * Time.deltaTime);
		foreach (Transform tr in ToRotate)
						tr.eulerAngles += rotAmount;
	}
}