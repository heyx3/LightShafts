using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Generates a Worley texture.
/// </summary>
[RequireComponent(typeof(MeshRenderer))]
public class WorleyVisualizer : MonoBehaviour
{
	public int TexWidth = 512,
			   TexHeight = 512;
	public WorleyGenerator Generator = new WorleyGenerator();

	public bool GenerateNewTex = true;

	[System.NonSerialized] public Texture2D OutTex;


	void Update()
	{
		if (GenerateNewTex)
		{
			GenerateNewTex = false;
			Generate();
		}
	}

	private void Generate()
	{
		//Set up the texture.
		OutTex = new Texture2D(TexWidth, TexHeight, TextureFormat.RGBA32, false, true);

		//Generate the values.
		float[,] vals = new float[TexWidth, TexHeight];
		Generator.Generate(vals);

		//Analyze the values.
		float min = vals[0, 0],
			  max = vals[0, 0];
		for (int x = 0; x < TexWidth; ++x)
			for (int y = 0; y < TexHeight; ++y)
			{
				min = Mathf.Min(min, vals[x, y]);
				max = Mathf.Max(max, vals[x, y]);
			}

		//Convert the values to colors and update the texture.
		for (int x = 0; x < TexWidth; ++x)
			for (int y = 0; y < TexHeight; ++y)
			{
				float lerpVal = Mathf.InverseLerp(min, max, vals[x, y]);
				OutTex.SetPixel(x, y, new Color(lerpVal, lerpVal, lerpVal, 1.0f));
			}
		OutTex.Apply();
		GetComponent<MeshRenderer>().material.mainTexture = OutTex;
	}
}