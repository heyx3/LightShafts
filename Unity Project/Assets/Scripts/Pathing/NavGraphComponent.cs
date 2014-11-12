using System;
using UnityEngine;


/// <summary>
/// Represents the nav graph for the scene. There should only be one of these at a time.
/// </summary>
public class NavGraphComponent : MonoBehaviour
{
	public static NavGraphComponent Instance { get; private set; }
	public static NavGraph Graph { get { return Instance.MyGraph; } }

	public NavGraph MyGraph = new NavGraph();


	void Awake()
	{
		if (Instance != null)
			Debug.LogError("There is more than one nav graph in the scene!");
		Instance = this;
	}
}