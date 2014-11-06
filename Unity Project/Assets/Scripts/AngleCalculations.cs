using System;
using UnityEngine;

/// <summary>
/// Handles radian angle math. Radians are defined as follows:
/// Pointing left ({-1, 0}) = PI and -PI.
/// Pointing down ({0, -1}) = -PI/2.
/// Pointing right ({1, 0}) = 0.
/// Pointing up ({0, 1}) = PI/2.
/// </summary>
public static class AngleCalculations
{
	/// <summary>
	/// Gets the angle (radians) of the given direction.
	/// </summary>
	public static float GetAngle(Vector2 dir) { return Mathf.Atan2(dir.y, dir.x); }
	/// <summary>
	/// Gets the vector pointing from the origin in the direction of the given angle.
	/// </summary>
	public static Vector2 GetVector(float angle) { return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)); }
	/// <summary>
	/// Transforms the given euler angle into a wrapped radian.
	/// </summary>
	public static float TransformEulerAngleToRadian(float ang)
	{
		return WrapAngle(ang * Mathf.Deg2Rad);
	}
	/// <summary>
	/// Wraps the angle around to keep it in the range [-PI, PI).
	/// </summary>
	public static float WrapAngle(float angle)
	{
		const float twoPi = 2.0f * Mathf.PI;
		while (angle >= Mathf.PI) angle -= twoPi;
		while (angle < -Mathf.PI) angle += twoPi;

		return angle;
	}
}