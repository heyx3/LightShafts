using System;
using System.Collections.Generic;
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


	public float Radius = 50.0f;
	public float RotationRangeRadians = Mathf.PI * 0.25f;
	public Vector2 LightDir = new Vector2(1.0f, 0.0f);

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
	/// A line segment, including two points, their distances to the light source,
	///     and their relative angle from the light source.
	/// The first point is NOT assumed to have a smaller "angle" value than the second point.
	/// </summary>
	private class Segment
	{
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
		private float x1_x2, y1_y2,
					  x1y2_y1x2;
		/// <summary>
		/// Computes the pre-computed intersection test values.
		/// </summary>
		private void ComputeVals()
		{
			x1_x2 = P1.x - P2.x;
			y1_y2 = P1.y - P2.y;
			x1y2_y1x2 = (P1.x * P2.y) - (P1.y * P2.x);

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
			return new Vector2(((x1y2_y1x2 * otherLin.x1_x2) - (x1_x2 * otherLin.x1y2_y1x2)) * denominator,
							   ((x1y2_y1x2 * otherLin.y1_y2) - (y1_y2 * otherLin.x1y2_y1x2)) * denominator);
		}
		/// <summary>
		/// Gets the intersections between the given circle and the line this segment is a part of.
		/// Returns null, or 1 or 2 Vector2 values.
		/// If there are two intersections, the second is guaranteed to have a larger Y value than the first.
		/// </summary>
		public Vector2[] CircleLineIntersection(Vector2 circlePos, float radius)
		{
			float discriminant = (radius * radius * SegmentLength * SegmentLength) -
								 (x1y2_y1x2 * x1y2_y1x2);
			if (discriminant < 0.0f) return null;


			float numeratorX1 = x1y2_y1x2 * P1ToP2.y,
				  numeratorY1 = -x1y2_y1x2 * P1ToP2.x,
				  denominator = 1.0f / (SegmentLength * SegmentLength);
			if (discriminant == 0.0f)
				return new Vector2[1] { new Vector2(numeratorX1 * denominator, numeratorY1 * denominator) };


			float sqrtDiscriminant = Mathf.Sqrt(discriminant),
				  numeratorX2 = Sign(P1ToP2.y) * P1ToP2.x * sqrtDiscriminant,
				  numeratorY2 = Mathf.Abs(P1ToP2.y) * sqrtDiscriminant;
			return new Vector2[2]
			{
				new Vector2((numeratorX1 - numeratorX2) * denominator,
							(numeratorY1 - numeratorY2) * denominator),
				new Vector2((numeratorX1 + numeratorX2) * denominator,
							(numeratorY1 + numeratorY2) * denominator),
			};
		}

		public override string ToString()
		{
			return "[ " + P1.ToString() + " -- " + P2.ToString() + " ]";
		}
	}

	
	void Update()
	{
		//Build the light mesh.
		if (!Static || LightMesh.vertexCount == 0)
		{
			Vector3 lightPos3D = MyTransform.position;
			Vector2 lightPos = new Vector2(lightPos3D.x, lightPos3D.y);

			const float PIOver2 = Mathf.PI * 0.5f;

			float rotCenter = GetAngle(LightDir),
				  rotMin = rotCenter - (0.5f * RotationRangeRadians),
				  rotMax = rotCenter + (0.5f * RotationRangeRadians);

			float rotMinDownOne = rotMin - (2.0f * Mathf.PI),
				  rotMaxDownOne = rotMax - (2.0f * Mathf.PI),
				  rotMinUpOne = rotMin + (2.0f * Mathf.PI),
				  rotMaxUpOne = rotMax + (2.0f * Mathf.PI);


			//First, build the list of all line segments that can block the light.
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


			//Next, simplify the individual segments:
			//1) Flip any segments whose A1 is more than their A2 so that we always know
			//    a segment's P1 comes before its P2.
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
					bool firstIntersectIsP1 = (ang1 < ang2 || (ang1 > PIOver2 && ang2 < PIOver2));

					if (p1Out && p2Out)
					{
						if (firstIntersectIsP1)
						{
							segments[i] = new Segment(intersections[0], intersections[1], ang1, ang2,
													  (intersections[0] - lightPos).magnitude,
													  (intersections[1] - lightPos).magnitude);
						}
						else
						{
							segments[i] = new Segment(intersections[1], intersections[0], ang2, ang1,
													  (intersections[1] - lightPos).magnitude,
													  (intersections[0] - lightPos).magnitude);
						}
					}
					else if (p1Out)
					{
						if (firstIntersectIsP1)
						{
							segments[i] = new Segment(intersections[0], segments[i].P2, ang1, segments[i].A2,
													  (intersections[0] - lightPos).magnitude,
													  segments[i].D2);
						}
						else
						{
							segments[i] = new Segment(intersections[1], segments[i].P2, ang2, segments[i].A2,
													  (intersections[1] - lightPos).magnitude,
													  segments[i].D2);
						}
					}
					else
					{
						if (firstIntersectIsP1)
						{
							segments[i] = new Segment(segments[i].P1, intersections[1], segments[i].A1, ang2,
													  segments[i].D1,
													  (intersections[1] - lightPos).magnitude);
						}
						else
						{
							segments[i] = new Segment(segments[i].P1, intersections[0], segments[i].A1, ang1,
													  segments[i].D1,
													  (intersections[0] - lightPos).magnitude);
						}
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
					if (segments[i].A1 > segments[i].A2)
					{
						Debug.LogError("Assert failed: segment " + i + ", value " + segments[i].ToString());
					}
					if (segments[i + 1].A1 > segments[i + 1].A2)
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

				for (int j = 0; j < segments.Count; ++j)
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
								float fp1OnS_Angle = GetAngle(fp1OnS),
									  fp2OnS_Angle = GetAngle(fp2OnS);
								float fp1OnS_Dist = ,
									  fp2OnS_Dist = ;

								Segment firstPart = new Segment(second.P1, fp1OnS, second.A1, fp1OnS_Angle,)

								segments.Insert(j + 1, new Segment(second.P1, ));
								//TODO: Finish.
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
							//TODO: Finish.
						}
						//First intersects second from behind.
						else if (fa2)
						{
							//TODO: Finish.
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
								//TODO: Finish based on ealier case.
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

			
			//Now build the vertices of the light mesh.

			//Start by finding the first segment that isn't completely behind the rotation range.
			//TODO: Implement.


			//Now iterate through small increments of the rotation range.
			//TODO: Implement.
		}
	}
}