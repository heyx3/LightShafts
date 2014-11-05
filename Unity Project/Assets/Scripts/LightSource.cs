﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// An emitter of light.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
public class LightSource : MonoBehaviour
{
	/// <summary>
	/// A statically-accessible list of all light sources.
	/// </summary>
	public static List<LightSource> Sources = new List<LightSource>();

	/// <summary>
	/// Gets the angle (radians) of the given direction.
	/// Pointing left ({-1, 0}) = PI and -PI
	/// Pointing up ({0, -1}) = -PI/2
	/// Pointing right ({1, 0}) = 0
	/// Pointing down ({0, 1}) = PI/2
	/// </summary>
	private static float GetAngle(Vector2 dir) { return Mathf.Atan2(dir.y, dir.x); }
	/// <summary>
	/// Gets the vector pointing from the origin in the direction of the given angle.
	/// </summary>
	private static Vector2 GetVector(float angle) { return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)); }
	/// <summary>
	/// Gets the smallest distance squared between the given line SEGMENT and the given other point.
	/// </summary>
	private static float DistanceToSegmentSqr(Vector2 p1, Vector2 p2, Vector2 otherP)
	{
		//Taken from http://stackoverflow.com/questions/849211/shortest-distance-between-a-point-and-a-line-segment

		Vector2 p1ToP2 = p2 - p1,
				p1ToOtherP = otherP - p1;

		float t = Vector2.Dot(p1ToOtherP, p1ToP2) / p1ToP2.sqrMagnitude;

		if (t <= 0.0f) return (p1ToOtherP).sqrMagnitude;
		else if (t >= 1.0f) return (otherP - p2).sqrMagnitude;
		else return (otherP - (p1 + (t * p1ToP2))).sqrMagnitude;
	}
	/// <summary>
	/// Gets the sign of the given value. Returns 1.0f if the value is exactly 0.0.
	/// </summary>
	private static float Sign(float x) { return (x < 0.0f ? -1.0f : 1.0f); }
	/// <summary>
	/// Converts the given Vector2 into a Vector3 with a Z of 0.0f.
	/// </summary>
	private static Vector3 ToV3(Vector2 v) { return new Vector3(v.x, v.y, 0.0f); }
	/// <summary>
	/// Wraps the angle around to keep it in the range [-PI, PI).
	/// </summary>
	private static float WrapAngle(float angle)
	{
		const float twoPi = 2.0f * Mathf.PI;
		while (angle >= Mathf.PI) angle -= twoPi;
		while (angle < -Mathf.PI) angle += twoPi;

		return angle;
	}


	public float Radius = 50.0f;
	public float RotationRangeRadians = Mathf.PI * 0.25f;
	public float LightAngle = 0.0f;
	public float Intensity = 1.0f;

	public Color Color = Color.white;

	public float MeshRotationIncrementRadians = 0.1f;
	public bool Static = false;


	public Transform MyTransform { get; private set; }
	public Mesh LightMesh { get; private set; }

	
	private List<Segment> segments = new List<Segment>();


	void Awake()
	{
		Sources.Add(this);
		MyTransform = transform;

		MeshFilter mf = GetComponent<MeshFilter>();
		if (mf.mesh == null)
		{
			mf.mesh = new Mesh();
		}
		LightMesh = mf.mesh;
	}
	void OnDestroy()
	{
		Sources.Remove(this);
	}


	/// <summary>
	/// A line segment. Defined as two points, plus
	/// their distances to the light source and their relative angle from the light source.
	/// Immutable except for the "SwapPoints" function.
	/// </summary>
	private class Segment
	{
		public static Vector2 LineIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
		{
			return new Segment(a1, a2, 0.0f, 0.0f, 0.0f, 0.0f).LineIntersection(b1, b2);
		}


		public Vector2 P1 { get; private set; }
		public Vector2 P2 { get; private set; }

		public float A1 { get; private set; }
		public float A2 { get; private set; }

		public float D1 { get; private set; }
		public float D2 { get; private set; }


		public Vector2 P1ToP2 { get; private set; }
		public Vector2 P1ToP2Norm { get; private set; }
		public float SegmentLength { get; private set; }

		/// <summary>
		/// Pre-computed values to speed up line intersection test.
		/// </summary>
		private float x1_x2, y1_y2;
		/// <summary>
		/// Pre-computed value to speed up line intersection test.
		/// </summary>
		private double x1y2_y1x2;
		/// <summary>
		/// Computes the pre-computed intersection test values.
		/// </summary>
		private void ComputeVals()
		{
			x1_x2 = P1.x - P2.x;
			y1_y2 = P1.y - P2.y;
			x1y2_y1x2 = (double)(P1.x * P2.y) - (double)(P1.y * P2.x);

			P1ToP2 = -(new Vector2(x1_x2, y1_y2));
			SegmentLength = P1ToP2.magnitude;
			P1ToP2Norm = P1ToP2 / SegmentLength;
		}


		public Segment(Vector2 p1, Vector2 p2, float a1, float a2, float d1, float d2)
		{
			P1 = p1;
			P2 = p2;
			A1 = a1;
			A2 = a2;
			D1 = d1;
			D2 = d2;
			ComputeVals();
		}
		public Segment(Vector2 p1, Vector2 p2, Vector2 lightSource)
		{
			P1 = p1;
			P2 = p2;
			D1 = Vector2.Distance(p1, lightSource);
			D2 = Vector2.Distance(p2, lightSource);

			A1 = GetAngle(p1 - lightSource);
			A2 = GetAngle(p2 - lightSource);
			
			ComputeVals();
		}


		/// <summary>
		/// Swaps this light source's P1 and P2, along with all related data.
		/// </summary>
		public void SwapPoints()
		{
			Vector2 p1 = P1;
			float a1 = A1,
				  d1 = D1;

			P1 = P2;
			D1 = D2;
			A1 = A2;

			P2 = p1;
			D2 = d1;
			A2 = a1;

			x1_x2 = -x1_x2;
			y1_y2 = -y1_y2;
			x1y2_y1x2 = -x1y2_y1x2;
			P1ToP2 *= -1.0f;
			P1ToP2Norm *= -1.0f;
		}
		/// <summary>
		/// Gets the intersections between the given line and the line this segment is a part of.
		/// Assumes the two are not parallel.
		/// </summary>
		public Vector2 LineIntersection(Vector2 otherLineP1, Vector2 otherLineP2)
		{
			//Create a "Segment" representing the given line.
			Segment otherLin = new Segment(otherLineP1, otherLineP2, 0.0f, 0.0f, 0.0f, 0.0f);

			//Do some linear algebra.
			float denominator = 1.0f / ((x1_x2 * otherLin.y1_y2) -
										(y1_y2 * otherLin.x1_x2));
			return new Vector2((float)((x1y2_y1x2 * otherLin.x1_x2) - (x1_x2 * otherLin.x1y2_y1x2)) *
							       denominator,
							   (float)((x1y2_y1x2 * otherLin.y1_y2) - (y1_y2 * otherLin.x1y2_y1x2)) *
								   denominator);
		}
		/// <summary>
		/// Gets the intersections between the given circle and the line this segment is a part of.
		/// Returns null, or 1 or 2 Vector2 values.
		/// If there are two intersections, the second is guaranteed to have a larger Y value than the first.
		/// </summary>
		public Vector2[] CircleLineIntersection(Vector2 circlePos, float radius)
		{
			//Transform the circle so it's at the origin.
			Vector2[] result = new Segment(P1 - circlePos, P2 - circlePos,
										   A1, A2, D1, D2).CircleLineIntersection(radius);
			if (result == null) return result;
			for (int i = 0; i < result.Length; ++i)
				result[i] += circlePos;

			return result;
		}
		/// <summary>
		/// Gets the intersections between the given circle (centered at the origin) and
		///     the line this segment is a part of.
		/// Returns null, or 1 or 2 Vector2 values.
		/// If there are two intersections, the second is guaranteed to have a larger Y value than the first.
		/// </summary>
		public Vector2[] CircleLineIntersection(float radius)
		{
			double discriminant = (radius * radius * SegmentLength * SegmentLength) -
								      (x1y2_y1x2 * x1y2_y1x2);
			if (discriminant < -0.01f) return null;


			double numeratorX1 = x1y2_y1x2 * P1ToP2.y,
				   numeratorY1 = -x1y2_y1x2 * P1ToP2.x,
				   denominator = 1.0f / (SegmentLength * SegmentLength);
			if (discriminant == 0.0f)
				return new Vector2[1]
				{
					new Vector2((float)(numeratorX1 * denominator),
								(float)(numeratorY1 * denominator)),
				};


			double sqrtDiscriminant = System.Math.Sqrt(discriminant),
				   numeratorX2 = Sign(P1ToP2.y) * P1ToP2.x * sqrtDiscriminant,
				   numeratorY2 = Mathf.Abs(P1ToP2.y) * sqrtDiscriminant;
			return new Vector2[2]
			{
				new Vector2((float)((numeratorX1 - numeratorX2) * denominator),
							(float)((numeratorY1 - numeratorY2) * denominator)),
				new Vector2((float)((numeratorX1 + numeratorX2) * denominator),
							(float)((numeratorY1 + numeratorY2) * denominator)),
			};
		}

		public override string ToString()
		{
			return "[ " + P1.ToString() + " -- " + P2.ToString() + " ]";
		}
	}

	
	void OnDrawGizmos()
	{
		float rotRange = Mathf.Clamp(RotationRangeRadians, 0.0001f, 2.0f * Mathf.PI);
		float rotMin = LightAngle - (0.5f * rotRange),
			  rotMax = LightAngle + (0.5f * rotRange);

		Vector3 pos = transform.position;

		Gizmos.color = Color * Intensity;

		//Draw the beginning/middle/end segments of the light cone.
		Gizmos.DrawLine(pos, pos + ToV3(Radius * GetVector(rotMin)));
		Gizmos.DrawLine(pos, pos + ToV3(Radius * GetVector(rotMax)));
		Gizmos.DrawLine(pos, pos + ToV3(Radius * GetVector(LightAngle)));
	}

	void Update()
	{
		//Build the light mesh if it needs to be rebuilt.
		if (Static && LightMesh.vertexCount > 0) return;

		RebuildMesh();
	}


	/// <summary>
	/// Builds/rebuilds the mesh that represents what this light source emits.
	/// </summary>
	public void RebuildMesh()
	{
		//Pre-compute some useful stuff.

		Vector3 lightPos3D = MyTransform.position;
		Vector2 lightPos = new Vector2(lightPos3D.x, lightPos3D.y);
		
		float rotCenter = WrapAngle(LightAngle);

		const float PIOver2 = Mathf.PI * 0.5f;

		float rotRange = Mathf.Clamp(RotationRangeRadians, 0.0001f, 2.0f * Mathf.PI);
		float rotMin = rotCenter - (0.5f * rotRange),
			  rotMax = rotCenter + (0.5f * rotRange);

		float rotMinDownOne = rotMin - (2.0f * Mathf.PI),
			  rotMaxDownOne = rotMax - (2.0f * Mathf.PI),
			  rotMinUpOne = rotMin + (2.0f * Mathf.PI),
			  rotMaxUpOne = rotMax + (2.0f * Mathf.PI);


		//First, build the list of all line segments that might block the light.
		//Ignore walls that are too far away.
		segments.Clear();
		for (int i = 0; i < BoxWall.Walls.Count; ++i)
		{
			Bounds bounds3D = BoxWall.Walls[i].Box.bounds;
			Rect bounds = new Rect(bounds3D.min.x, bounds3D.min.y, bounds3D.size.x, bounds3D.size.y);

			Vector2 center = bounds.center,
					min = bounds.min,
					max = bounds.max;

			//Ignore this wall if it's too far away.
			float wallRadius = new Vector2(bounds.size.x * 0.5f, bounds.size.y * 0.5f).magnitude;
			float maxDist = Radius + wallRadius;
			if ((lightPos - center).sqrMagnitude > (maxDist * maxDist))
				continue;

			//Only bother adding segments that are facing the light source.
			//Left segment.
			if (lightPos.x < min.x)
				segments.Add(new Segment(min, new Vector2(min.x, max.y), lightPos));
			//Right segment.
			if (lightPos.x > max.x)
				segments.Add(new Segment(new Vector2(max.x, min.y), max, lightPos));

			//Top segment.
			if (lightPos.y < min.y)
				segments.Add(new Segment(min, new Vector2(max.x, min.y), lightPos));
			//Bottom segment.
			if (lightPos.y > max.y)
				segments.Add(new Segment(new Vector2(min.x, max.y), max, lightPos));
		}


		//Next, simplify each individual segment:
		//1) Flip any segments whose A1 is more than their A2 so that we always know
		//    a segment's P1 comes before its P2 in terms of its angle.
		//2) If the segment extends beyond the light radius, clip it to the edges of the light.
		//3) If the segment crosses over the end of the unit circle (the transition from PI to -PI radians),
		//    split it at the crossover point.
		for (int i = 0; i < segments.Count; ++i)
		{
			//Do the points need to be swapped?
			if (segments[i].A1 > segments[i].A2)
			{
				segments[i].SwapPoints();
			}

			//Does the segment extend beyond the edge of the light radius?
			bool p1Out = segments[i].D1 > Radius,
				 p2Out = segments[i].D2 > Radius;
			if (p1Out || p2Out)
			{
				Vector2[] intersections = segments[i].CircleLineIntersection(lightPos, Radius);

				//If this segment doesn't really touch the light at all, ignore it.
				if (intersections == null || intersections.Length == 1)
				{
					segments.RemoveAt(i);
					i -= 1;
					continue;
				}

				//Otherwise, see which intersections correspond with which segment points
				//    and then swap intersections for segment points as necessary.

				float ang1 = GetAngle(intersections[0] - lightPos),
					  ang2 = GetAngle(intersections[1] - lightPos);

				bool firstIntersectIsP1 = (ang1 < ang2 || (ang1 > PIOver2 && ang2 < -PIOver2));
				if (!firstIntersectIsP1)
				{
					Vector2 int1 = intersections[0];
					intersections[0] = intersections[1];
					intersections[1] = int1;
				}

				if (p1Out && p2Out)
				{
					segments[i] = new Segment(intersections[0], intersections[1], ang1, ang2,
											  (intersections[0] - lightPos).magnitude,
											  (intersections[1] - lightPos).magnitude);
				}
				else if (p1Out)
				{
					segments[i] = new Segment(intersections[0], segments[i].P2, ang1, segments[i].A2,
											  (intersections[0] - lightPos).magnitude, segments[i].D2);
				}
				else
				{
					segments[i] = new Segment(segments[i].P1, intersections[1], segments[i].A1, ang2,
											  segments[i].D1, (intersections[1] - lightPos).magnitude);
				}
			}

			//Does the segment cross over the end of the unit circle?
			if (segments[i].A1 < -PIOver2 && segments[i].A2 > PIOver2)
			{
				//Get the point on the segment where the Y value is 0 (i.e. the exact point
				//    where it crosses the end of the unit circle).
				Vector2 velocity = segments[i].P2 - segments[i].P1;
				float t = -segments[i].P1.y / velocity.y;
				Vector2 splitPoint = segments[i].P1 + (velocity * t);
				float dist = Vector2.Distance(splitPoint, lightPos);

				//Insert the new segment for the top-half.
				segments.Insert(i + 1, new Segment(segments[i].P2, splitPoint, segments[i].A2, Mathf.PI,
												   segments[i].D2, dist));
				//Cut the original segment to the bottom-half.
				segments[i] = new Segment(splitPoint, segments[i].P1, -Mathf.PI, segments[i].A1,
										  dist, segments[i].D1);
				i += 1;


				//Sanity check.
				//TODO: Remove once this algorithm is verified.
				if (segments[i - 1].A1 > segments[i - 1].A2)
				{
					Debug.LogError("Assert failed: segment " + i + ", value " + segments[i].ToString());
				}
				if (segments[i].A1 > segments[i].A2)
				{
					Debug.LogError("Assert failed: segment " + (i + 1) + ", value " + segments[i + 1].ToString());
				}
			}
		}


		//Now get any segments that are at least partly obscured by other segments and trim them.
		//After this algorithm is finished, no segment will obscure any other segment at all.
		for (int i = 0; i < segments.Count; ++i)
		{
			Segment first = segments[i];

			for (int j = i; j < segments.Count; ++j)
			{
				if (j != i)
				{
					Segment second = segments[j];

					bool fa1 = (first.A1 > second.A1 && first.A1 < second.A2),
						 fa2 = (first.A2 > second.A1 && first.A2 < second.A2);
					bool sa1 = (second.A1 > first.A1 && second.A1 < first.A2),
						 sa2 = (second.A2 > first.A1 && second.A2 < first.A2);

					//First is inside second.
					if (fa1 && fa2)
					{
						//Figure out which segment is in front.
						//Assume the two segments don't intersect -- if "first.P1" is in front of
						//    the second segment, then "first.P2" is as well.
						Vector2 srcToFP1 = (first.P1 - lightPos).normalized;
						Vector2 fp1OnS = second.LineIntersection(lightPos, lightPos + srcToFP1);
						float dist1 = Vector2.Distance(fp1OnS, lightPos);

						//If the first segment is in front of the second, split into three segments --
						//    the part of "second" before "first", then "first", then the part of
						//    "second" in front of "first".
						if (dist1 > first.D1)
						{
							Vector2 srcToFP2 = (first.P2 - lightPos).normalized;
							Vector2 fp2OnS = second.LineIntersection(lightPos, lightPos + srcToFP2);
							float fp1OnS_Dist = (fp1OnS - lightPos).magnitude,
								  fp2OnS_Dist = (fp2OnS - lightPos).magnitude;

							Segment firstPart = new Segment(second.P1, fp1OnS, second.A1, first.A1,
															second.D1, fp1OnS_Dist),
									lastPart = new Segment(fp2OnS, second.P2, first.A2, second.A2,
														   fp2OnS_Dist, second.D2);

							segments[j] = firstPart;
							segments.Add(lastPart);
						}
						//Otherwise, ignore "first".
						else
						{
							segments.RemoveAt(i);
							i -= 1;
							break;
						}
					}
					//First intersects second from above.
					else if (fa1)
					{
						Vector2 srcToFP1 = (first.P1 - lightPos).normalized;
						Vector2 fp1OnS = second.LineIntersection(lightPos, lightPos + srcToFP1);
						float dist1 = Vector2.Distance(fp1OnS, lightPos);

						//If "first" is in front of "second", cut off end of "second"
						//    at the point where "first" starts.
						if (dist1 > first.D1)
						{
							segments[j] = new Segment(second.P1, fp1OnS, second.A1, first.A1,
													  second.D1, dist1);
						}
						//Otherwise, cut off the beginning of "first" at the point where "second" ends.
						else
						{
							Vector2 srcToSP2 = (second.P2 - lightPos).normalized;
							Vector2 sp2OnF = first.LineIntersection(lightPos, lightPos + srcToSP2);

							segments[i] = new Segment(sp2OnF, first.P2, second.A2, first.A2,
													  (sp2OnF - lightPos).magnitude, first.D2);
						}
					}
					//First intersects second from behind.
					else if (fa2)
					{
						Vector2 srcToFP2 = (first.P2 - lightPos).normalized;
						Vector2 fp2OnS = second.LineIntersection(lightPos, lightPos + srcToFP2);
						float dist2 = Vector2.Distance(fp2OnS, lightPos);

						//If "first" is in front of "second", cut off the beginning of "second"
						//    until the point where "first" ends.
						if (dist2 > first.D2)
						{
							segments[j] = new Segment(fp2OnS, second.P2, first.A2, second.A2,
													  dist2, second.D2);
						}
						//Otherwise, cut off the end of "first" at the beginning of "second".
						else
						{
							Vector2 srcToSP1 = (second.P1 - lightPos).normalized;
							Vector2 sp1OnF = first.LineIntersection(lightPos, lightPos + srcToSP1);
							float dist1 = Vector2.Distance(sp1OnF, lightPos);

							segments[i] = new Segment(first.P1, sp1OnF, first.A1, second.A1,
													  first.D1, dist1);
						}
					}
					//Second is inside first.
					else if (sa1 && sa2)
					{
						//Figure out which segment is in front.
						//Assume the two segments don't intersect -- if "second.P1" is in front of
						//    the first segment, then "second.P2" is as well.
						Vector2 srcToSP1 = (second.P1 - lightPos).normalized;
						Vector2 sp1OnF = first.LineIntersection(lightPos, lightPos + srcToSP1);
						float dist1 = Vector2.Distance(sp1OnF, lightPos);

						//If the second segment is in front of the first, split into three segments --
						//    the part of "first" before "second", then "second", then the part of
						//    "first" in front of "second".
						if (dist1 > second.D1)
						{
							Vector2 srcToSp2 = (second.P2 - lightPos).normalized;
							Vector2 sp2OnF = first.LineIntersection(lightPos, lightPos + srcToSp2);
							float sp1OnF_Dist = (sp1OnF - lightPos).magnitude,
								  sp2OnF_Dist = (sp2OnF - lightPos).magnitude;

							Segment firstPart = new Segment(first.P1, sp1OnF, first.A1, second.A1,
															first.D1, sp1OnF_Dist),
									lastPart = new Segment(sp2OnF, first.P2, second.A2, first.A2,
														   sp2OnF_Dist, first.D2);

							segments[i] = firstPart;
							segments.Add(lastPart);
						}
						//Otherwise, ignore "second".
						else
						{
							segments.RemoveAt(i);
							i -= 1;
							break;
						}
					}
				}
			}
		}


		//Finally, sort the segments based on their angle, from -PI to PI.
		List<Segment> sortedSegs = new List<Segment>();
		sortedSegs.Capacity = segments.Count;
		while (segments.Count > 0)
		{
			Segment lastSeg = segments[segments.Count - 1];
				
			int i;
			for (i = 0; i < sortedSegs.Count; ++i)
			{
				if (lastSeg.A1 > sortedSegs[i].A2)
					break;
			}

			sortedSegs.Insert(i, lastSeg);
			segments.RemoveAt(segments.Count - 1);
		}
		segments = sortedSegs;

		//DEBUG
		string str = "";
		foreach (Segment seg in segments)
			str += seg.ToString() + "; ";
		Debug.Log(segments.Count > 0 ? str.Substring(0, str.Length - 2) : str);

			
		//Now build the vertices of the light mesh.

		//Start by finding the first segment that isn't completely behind the rotation range.
		int segIndex;
		float startRot = WrapAngle(rotMin);
		for (segIndex = 0; segIndex < segments.Count; ++segIndex)
			if (segments[segIndex].A1 > startRot)
				break;


		//Now iterate through small increments of the rotation range and build the mesh.

		List<Vector2> poses = new List<Vector2>();
		List<Color> colors = new List<Color>();
		List<int> indices = new List<int>();

		poses.Add(lightPos);
		colors.Add(Color * Intensity);

		Vector2 lastPos = Vector2.zero;
		Color lastCol = new Color(0.0f, 0.0f, 0.0f, 0.0f);

		float invMaxDist = 1.0f / Radius;
		float endRot = startRot + RotationRangeRadians;
		bool firstIncrement = true;
		bool wasObstructed = false;

		for (float currentRot = startRot; currentRot <= endRot; currentRot += MeshRotationIncrementRadians)
		{
			//Get the current rotation.
			Vector2 currentDir = GetVector(currentRot);
			float wrappedRot = WrapAngle(currentRot);

			//See if the next segment is occluding this light shaft.
			if (segments.Count > 0 && segments[segIndex].A1 < wrappedRot)
			{
				wasObstructed = true;

				//Create the first vertex.
				
				//If this is the first segment, start at "startRot".
				if (firstIncrement)
				{
					Vector2 firstP = segments[segIndex].LineIntersection(lightPos, lightPos + currentDir);
					float firstDist = Vector2.Distance(firstP, lightPos);

					//Add this first vertex.
					poses.Add(firstP);
					colors.Add(colors[0] * (1.0f - (firstDist * invMaxDist)));
				}
				//Otherwise, just start at the beginning of the segment.
				else
				{
					//First add a triangle to catch up from the last position
					//    to the beginning of the segment.

					poses.Add(lastPos);
					colors.Add(lastCol);

					poses.Add(lightPos + (Radius * (segments[segIndex].P1 - lightPos).normalized));
					colors.Add(new Color(0.0f, 0.0f, 0.0f, 0.0f));

					indices.Add(0);
					indices.Add(poses.Count - 2);
					indices.Add(poses.Count - 1);


					//Now add the beginning of the segment to the next triangle.
					poses.Add(segments[segIndex].P1);
					colors.Add(colors[0] * (1.0f - (segments[segIndex].D1 * invMaxDist)));
				}


				//Create the second vertex.

				//If the segment passes through the end of the viewable range,
				//    figure out where exactly the light should end.
				if (segments[segIndex].A2 > endRot)
				{
					Vector2 toEnd = GetVector(endRot);
					Vector2 secondP = segments[segIndex].LineIntersection(lightPos, lightPos + toEnd);
					float secondDist = Vector2.Distance(secondP, lightPos);

					poses.Add(secondP);
					colors.Add(colors[0] * (1.0f - (secondDist * invMaxDist)));

					//We're done here, so move the rotation counter to
					//    something past the end of the range.
					currentRot = segments[segIndex].A2;
				}
				//Otherwise, just add light that covers the entirety of the segment.
				else
				{
					poses.Add(segments[segIndex].P2);
					colors.Add(colors[0] * (1.0f - (segments[segIndex].D2 * invMaxDist)));

					//Set the "previous" position to point towards the end of the segment,
					//    but with the size of the full light radius.
					lastPos = lightPos + (Radius * (segments[segIndex].P2 - lightPos).normalized);
					lastCol = new UnityEngine.Color(0.0f, 0.0f, 0.0f, 0.0f);

					//Move the rotation counter so that it starts at
					//    the end of the segment in the next iteration.
					currentRot = segments[segIndex].A2 - MeshRotationIncrementRadians;
				}

				//Add the indices.
				indices.Add(0);
				indices.Add(poses.Count - 2);
				indices.Add(poses.Count - 1);
				
				//Move the segment counter to the next segment.
				segIndex = (segIndex + 1) % segments.Count;
			}
			//Otherwise, we're clear to add unobstructed light.
			else
			{
				wasObstructed = false;

				//If this is the first iteration, don't add a triangle --
				//    just calculate the beginning of the light for the next iteration.
				if (firstIncrement)
				{
					lastPos = lightPos + (Radius * currentDir);
					lastCol = new Color(0.0f, 0.0f, 0.0f, 0.0f);
				}
				//Otherwise, add a triangle starting from the end of the last unobstructed triangle.
				else
				{
					poses.Add(lastPos);
					colors.Add(lastCol);

					poses.Add(lightPos + (Radius * currentDir));
					colors.Add(new Color(0.0f, 0.0f, 0.0f, 0.0f));

					indices.Add(0);
					indices.Add(poses.Count - 2);
					indices.Add(poses.Count - 1);

					lastPos = poses[poses.Count - 1];
					lastCol = colors[colors.Count - 1];
				}
			}

			firstIncrement = false;
		}
		//Fill in the last little gap between the end of the last triangle and the actual end of the range.
		if (!wasObstructed)
		{
			poses.Add(lastPos);
			colors.Add(lastCol);

			poses.Add(lightPos + (Radius * GetVector(endRot)));
			colors.Add(new Color(0.0f, 0.0f, 0.0f, 0.0f));

			indices.Add(0);
			indices.Add(poses.Count - 2);
			indices.Add(poses.Count - 1);
		}

		LightMesh.vertices = poses.ConvertAll(v => new Vector3(v.x, v.y, 0.0f)).ToArray();
		LightMesh.colors = colors.ToArray();
		LightMesh.triangles = indices.ToArray();
	}
}