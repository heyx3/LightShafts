using System;
using UnityEngine;


/// <summary>
/// Handles player animation.
/// </summary>
public class PlayerAnimation : MonoBehaviour
{
	public SpriteRenderer MyRenderer { get; private set; }


	public Sprite Idle, Walk,
				  IdleBow, WalkBow;


	/// <summary>
	/// Sets this player's sprite to the given value.
	/// If it already has the given sprite, the animation is not interrupted.
	/// </summary>
	public void SetSprite(Sprite newSpr)
	{
		if (MyRenderer.sprite != newSpr)
		{
			MyRenderer.sprite = newSpr;
		}
	}


	void Awake()
	{
		MyRenderer = GetComponent<SpriteRenderer>();
	}
}