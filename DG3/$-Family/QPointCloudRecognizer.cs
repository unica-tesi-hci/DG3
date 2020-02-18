/**
 * Extract from The $Q Point-Cloud Recognizer (.NET Framework C# version)
 *
 * 	    Radu-Daniel Vatavu, Ph.D.
 *	    University Stefan cel Mare of Suceava
 *	    Suceava 720229, Romania
 *	    radu.vatavu@usm.ro
 *
 *	    Lisa Anthony, Ph.D.
 *      Department of CISE
 *      University of Florida
 *      Gainesville, FL 32611, USA
 *      lanthony@cise.ufl.edu
 *
 *	    Jacob O. Wobbrock, Ph.D.
 * 	    The Information School
 *	    University of Washington
 *	    Seattle, WA 98195-2840
 *	    wobbrock@uw.edu
 *
 * The academic publication for the $Q recognizer, and what should be 
 * used to cite it, is:
 *
 *	Vatavu, R.-D., Anthony, L. and Wobbrock, J.O. (2018).  
 *	  $Q: A Super-Quick, Articulation-Invariant Stroke-Gesture
 *    Recognizer for Low-Resource Devices. Proceedings of 20th International Conference on
 *    Human-Computer Interaction with Mobile Devices and Services (MobileHCI '18). Barcelona, Spain
 *	  (September 3-6, 2018). New York: ACM Press.
 *	  DOI: https://doi.org/10.1145/3229434.3229465
 *
 * The $Q Point-Cloud Recognizer (.NET Framework C# version) is distributed under the "New BSD License" agreement:
 *
 * Copyright (c) 2018, Radu-Daniel Vatavu, Lisa Anthony, and 
 * Jacob O. Wobbrock. All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *    * Redistributions of source code must retain the above copyright
 *      notice, this list of conditions and the following disclaimer.
 *    * Redistributions in binary form must reproduce the above copyright
 *      notice, this list of conditions and the following disclaimer in the
 *      documentation and/or other materials provided with the distribution.
 *    * Neither the names of the University Stefan cel Mare of Suceava, 
 *	    University of Washington, nor University of Florida, nor the names of its contributors 
 *	    may be used to endorse or promote products derived from this software 
 *	    without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS
 * IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL Radu-Daniel Vatavu OR Lisa Anthony
 * OR Jacob O. Wobbrock BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, 
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT 
 * OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
**/

using System;
namespace DG3
{
	class QPointCloudRecognizer
	{
		private const int SAMPLING_RESOLUTION = 64;                             // default number of points on the gesture path
        private const int MAX_INT_COORDINATES = 1024;                           // $Q only: each point has two additional x and y integer coordinates in the interval [0..MAX_INT_COORDINATES-1] used to operate the LUT table efficiently (O(1))
        public static int LUT_SIZE = 64;                                        // $Q only: the default size of the lookup table is 64 x 64
        public static int LUT_SCALE_FACTOR = MAX_INT_COORDINATES / LUT_SIZE;    // $Q only: scale factor to convert between integer x and y coordinates and the size of the LUT

		// $Q's two major optimization layers (Early Abandoning and Lower Bounding)
		// can be activated / deactivated as desired
		public static bool UseEarlyAbandoning = true;
		public static bool UseLowerBounding = true;

		/// <summary>
		/// Implements greedy search for a minimum-distance matching between two point clouds.
		/// Implements Early Abandoning and Lower Bounding (LUT) optimizations.
		/// </summary>
		public static float GreedyCloudMatch(Gesture gesture1, Gesture gesture2, float minSoFar)
		{
			int n = gesture1.Points.Length;       // the two clouds should have the same number of points by now
			float eps = 0.5f;                     // controls the number of greedy search trials (eps is in [0..1])
			int step = (int)Math.Floor(Math.Pow(n, 1.0f - eps));

			if (UseLowerBounding)
			{
				float[] LB1 = ComputeLowerBound(gesture1.Points, gesture2.Points, gesture2.LUT, step);  // direction of matching: gesture1 --> gesture2
				float[] LB2 = ComputeLowerBound(gesture2.Points, gesture1.Points, gesture1.LUT, step);  // direction of matching: gesture2 --> gesture1
				for (int i = 0, indexLB = 0; i < n; i += step, indexLB++)
				{
					if (LB1[indexLB] < minSoFar) minSoFar = Math.Min(minSoFar, CloudDistance(gesture1.Points, gesture2.Points, i, minSoFar));  // direction of matching: gesture1 --> gesture2 starting with index point i
					if (LB2[indexLB] < minSoFar) minSoFar = Math.Min(minSoFar, CloudDistance(gesture2.Points, gesture1.Points, i, minSoFar));  // direction of matching: gesture2 --> gesture1 starting with index point i   
				}
			}
			else
			{
				for (int i = 0; i < n; i += step)
				{
					minSoFar = Math.Min(minSoFar, CloudDistance(gesture1.Points, gesture2.Points, i, minSoFar));  // direction of matching: gesture1 --> gesture2 starting with index point i
					minSoFar = Math.Min(minSoFar, CloudDistance(gesture2.Points, gesture1.Points, i, minSoFar));  // direction of matching: gesture2 --> gesture1 starting with index point i   
				}
			}

			return minSoFar;
		}

		/// <summary>
		/// Computes lower bounds for each starting point and the direction of matching from points1 to points2 
		/// </summary>
		private static float[] ComputeLowerBound(Point[] points1, Point[] points2, int[][] LUT, int step)
		{
			int n = points1.Length;
			float[] LB = new float[n / step + 1];
			float[] SAT = new float[n];

			LB[0] = 0;
			for (int i = 0; i < n; i++)
			{
				int index = LUT[points1[i].intY / LUT_SCALE_FACTOR][points1[i].intX / LUT_SCALE_FACTOR];
				float dist = SqrEuclideanDistance(points1[i], points2[index]);
				SAT[i] = (i == 0) ? dist : SAT[i - 1] + dist;
				LB[0] += (n - i) * dist;
			}

			for (int i = step, indexLB = 1; i < n; i += step, indexLB++)
				LB[indexLB] = LB[0] + i * SAT[n - 1] - n * SAT[i - 1];
			return LB;
		}

		/// <summary>
		/// Computes the distance between two point clouds by performing a minimum-distance greedy matching
		/// starting with point startIndex
		/// </summary>
		/// <param name="points1"></param>
		/// <param name="points2"></param>
		/// <param name="startIndex"></param>
		/// <returns></returns>
		private static float CloudDistance(Point[] points1, Point[] points2, int startIndex, float minSoFar)
		{
			int n = points1.Length;                // the two point clouds should have the same number of points by now
			int[] indexesNotMatched = new int[n];  // stores point indexes for points from the 2nd cloud that haven't been matched yet
			for (int j = 0; j < n; j++)
				indexesNotMatched[j] = j;

			float sum = 0;                // computes the sum of distances between matched points (i.e., the distance between the two clouds)
			int i = startIndex;           // start matching with point startIndex from the 1st cloud
			int weight = n;               // implements weights, decreasing from n to 1
			int indexNotMatched = 0;      // indexes the indexesNotMatched[..] array of points from the 2nd cloud that are not matched yet
			do
			{
				int index = -1;
				float minDistance = float.MaxValue;
				for (int j = indexNotMatched; j < n; j++)
				{
					float dist = SqrEuclideanDistance(points1[i], points2[indexesNotMatched[j]]);  // use the squared Euclidean distance
					if (dist < minDistance)
					{
						minDistance = dist;
						index = j;
					}
				}
				indexesNotMatched[index] = indexesNotMatched[indexNotMatched];  // point indexesNotMatched[index] of the 2nd cloud is now matched to point i of the 1st cloud
				sum += (weight--) * minDistance;           // weight each distance with a confidence coefficient that decreases from n to 1

				if (UseEarlyAbandoning)
				{
					if (sum >= minSoFar)
						return sum;       // implement early abandoning
				}

				i = (i + 1) % n;                           // advance to the next point in the 1st cloud
				indexNotMatched++;                         // update the number of points from the 2nd cloud that haven't been matched yet
			} while (i != startIndex);
			return sum;
		}

		/// <summary>
		/// Computes the Squared Euclidean Distance between two points in 2D
		/// </summary>
		public static float SqrEuclideanDistance(Point a, Point b)
		{
			return (a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y);
		}
		/// <summary>
		 /// Computes the Euclidean Distance between two points in 2D
		 /// </summary>
		public static float EuclideanDistance(Point a, Point b)
		{
			return (float)Math.Sqrt(SqrEuclideanDistance(a, b));
		}

		/// <summary>
		/// Normalizes the gesture path. 
		/// </summary>
		public static Point[] Normalize(Point[] PointsRaw)
		{
			// standard $-family processing: resample, scale, and translate to origin

			Point[] Points = Resample(PointsRaw, SAMPLING_RESOLUTION);
			Points = Scale(Points);
			Points = TranslateTo(Points, Centroid(Points));
			return Points;
		}
		/// <summary>
		/// Constructs a lookup table for fast lower bounding (used by $Q)
		/// </summary>
		/// <param name="g"></param>
		public static int[][] ComputeLUT(Point[] Points)
		{
			Points = TransformCoordinatesToIntegers(Points);
			return(ConstructLUT(Points));
		}
		
		#region gesture pre-processing steps: scale normalization, translation to origin, and resampling

		/// <summary>
		/// Performs scale normalization with shape preservation into [0..1]x[0..1]
		/// </summary>
		/// <param name="points"></param>
		/// <returns></returns>
		private static Point[] Scale(Point[] points)
		{
			float minx = float.MaxValue, miny = float.MaxValue, maxx = float.MinValue, maxy = float.MinValue;
			for (int i = 0; i < points.Length; i++)
			{
				if (minx > points[i].X) minx = points[i].X;
				if (miny > points[i].Y) miny = points[i].Y;
				if (maxx < points[i].X) maxx = points[i].X;
				if (maxy < points[i].Y) maxy = points[i].Y;
			}

			Point[] newPoints = new Point[points.Length];
			float scale = Math.Max(maxx - minx, maxy - miny);
			for (int i = 0; i < points.Length; i++)
				newPoints[i] = new Point((points[i].X - minx) / scale, (points[i].Y - miny) / scale, points[i].StrokeID);
			return newPoints;
		}

		/// <summary>
		/// Translates the array of points by p
		/// </summary>
		/// <param name="points"></param>
		/// <param name="p"></param>
		/// <returns></returns>
		public static Point[] TranslateTo(Point[] points, Point p)
		{
			Point[] newPoints = new Point[points.Length];
			for (int i = 0; i < points.Length; i++)
				newPoints[i] = new Point(points[i].X - p.X, points[i].Y - p.Y, points[i].StrokeID);
			return newPoints;
		}

		/// <summary>
		/// Computes the centroid for an array of points
		/// </summary>
		/// <param name="points"></param>
		/// <returns></returns>
		public static Point Centroid(Point[] points)
		{
			float cx = 0, cy = 0;
			for (int i = 0; i < points.Length; i++)
			{
				cx += points[i].X;
				cy += points[i].Y;
			}
			return new Point(cx / points.Length, cy / points.Length, 0);
		}

		/// <summary>
		/// Resamples the array of points into n equally-distanced points
		/// </summary>
		/// <param name="points"></param>
		/// <param name="n"></param>
		/// <returns></returns>
		public static Point[] Resample(Point[] points, int n)
		{
			Point[] newPoints = new Point[n];
			newPoints[0] = new Point(points[0].X, points[0].Y, points[0].StrokeID);
			int numPoints = 1;

			float I = PathLength(points) / (n - 1); // computes interval length
			float D = 0;
			for (int i = 1; i < points.Length; i++)
			{
				if (points[i].StrokeID == points[i - 1].StrokeID)
				{
					float d = EuclideanDistance(points[i - 1], points[i]);
					if (D + d >= I)
					{
						Point firstPoint = points[i - 1];
						while (D + d >= I)
						{
							// add interpolated point
							float t = Math.Min(Math.Max((I - D) / d, 0.0f), 1.0f);
							if (float.IsNaN(t)) t = 0.5f;
							newPoints[numPoints++] = new Point(
								(1.0f - t) * firstPoint.X + t * points[i].X,
								(1.0f - t) * firstPoint.Y + t * points[i].Y,
								points[i].StrokeID
							);

							// update partial length
							d = D + d - I;
							D = 0;
							firstPoint = newPoints[numPoints - 1];
						}
						D = d;
					}
					else D += d;
				}
			}

			if (numPoints == n - 1) // sometimes we fall a rounding-error short of adding the last point, so add it if so
				newPoints[numPoints++] = new Point(points[points.Length - 1].X, points[points.Length - 1].Y, points[points.Length - 1].StrokeID);
			return newPoints;
		}

		/// <summary>
		/// Computes the path length for an array of points
		/// </summary>
		/// <param name="points"></param>
		/// <returns></returns>
		private static float PathLength(Point[] points)
		{
			float length = 0;
			for (int i = 1; i < points.Length; i++)
				if (points[i].StrokeID == points[i - 1].StrokeID)
					length += EuclideanDistance(points[i - 1], points[i]);
			return length;
		}

		/// <summary>
		/// Scales point coordinates to the integer domain [0..MAXINT-1] x [0..MAXINT-1]
		/// </summary>
		private static Point[] TransformCoordinatesToIntegers(Point[] points)
		{
			for (int i = 0; i < points.Length; i++)
			{
				points[i].intX = (int)((points[i].X + 1.0f) / 2.0f * (MAX_INT_COORDINATES - 1));
				points[i].intY = (int)((points[i].Y + 1.0f) / 2.0f * (MAX_INT_COORDINATES - 1));
			}
			return points;
		}

		/// <summary>
		/// Constructs a Lookup Table that maps grip points to the closest point from the gesture path
		/// </summary>
		private static int[][] ConstructLUT(Point[] points)
		{
			int[][] LUT = new int[LUT_SIZE][];
			for (int i = 0; i < LUT_SIZE; i++)
				LUT[i] = new int[LUT_SIZE];

			for (int i = 0; i < LUT_SIZE; i++)
				for (int j = 0; j < LUT_SIZE; j++)
				{
					int minDistance = int.MaxValue;
					int indexMin = -1;
					for (int t = 0; t < points.Length; t++)
					{
						int row = points[t].intY / LUT_SCALE_FACTOR;
						int col = points[t].intX / LUT_SCALE_FACTOR;
						int dist = (row - i) * (row - i) + (col - j) * (col - j);
						if (dist < minDistance)
						{
							minDistance = dist;
							indexMin = t;
						}
					}
					LUT[i][j] = indexMin;
				}
			return LUT;
		}

		#endregion
	}
}
