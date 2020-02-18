using System;
using System.Linq;
using System.Collections.Generic;

namespace DG3
{
	/// <summary>
	/// Implements the DG3 Recognizer
	/// </summary>
	public class Recognizer
	{
		/// <summary>
		/// Gesture part recognizer
		/// </summary>
		/// <returns></returns>
		public static string ClassifyParts(Gesture candidate, Gesture[] templateSet, bool complete = false, bool stroke_complete = false, bool allow_excess_strokes = false)
		{
			List<Gesture> matchingDataset = new List<Gesture>();
			if (!complete)
			{
				foreach (Gesture template in templateSet)
				{
					if (template.Part_Combinations.ContainsKey(candidate.StrokeNumber))
					{
						if (!stroke_complete)
						{
							matchingDataset.AddRange(template.Part_Combinations[candidate.StrokeNumber]);
						}
						else
						{
							matchingDataset.AddRange(template.Part_Combinations_Full[candidate.StrokeNumber]);
						}
					}
				}
			}
			else
			{
				matchingDataset.AddRange(templateSet.Where(x => x.StrokeNumber == candidate.StrokeNumber));
			}

			if (allow_excess_strokes && matchingDataset.Count < 1)
			{
				matchingDataset.AddRange(templateSet);
			}

			return CustomMatch(candidate, matchingDataset);
		}

		/// <summary>
		/// Chooses the gesture distance calculation method
		/// </summary>
		/// <returns></returns>
		private static string CustomMatch(Gesture gesture, List<Gesture> dataset)
		{
			float minDistance = float.MaxValue;
			string gestureClass = "";
			foreach (Gesture template in dataset)
			{
				float dist;
				if (gesture.StrokeNumber == 1 && template.StrokeNumber == 1)
				{
					dist = (float)Dollar.OptimalCosineDistance(gesture.Vector, template.Vector)[0];
				}
				else
				{
					dist = QPointCloudRecognizer.GreedyCloudMatch(gesture, template, minDistance);
				}
				if (dist < minDistance)
				{
					minDistance = dist;
					gestureClass = template.Name;
				}
			}
			return gestureClass;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public static int DividePart(List<Point> RawPoints, Point p, int lp)
		{
			double threshold = 25 * Math.PI / 180;
			int m = 100;
			int rvalue = -1;
			double elapsed;
			if (RawPoints.Last().StrokeID == RawPoints[lp].StrokeID)
			{
				elapsed = RawPoints.Last().Time - RawPoints[lp].Time;
			}
			else
			{
				elapsed = RawPoints.Last().Time - RawPoints.Where(x => x.StrokeID == RawPoints.Last().StrokeID).First().Time;
			}

			if (elapsed >= m * 2)
			{

				int k = RawPoints.Count - 1;
				double time = p.Time;
				double timediff = 0;
				int zsd1 = -1;
				int zsd2 = -1;
				bool flag1 = true;
				bool flag2 = true;
				while (k >= 0 && flag2)
				{
					timediff = time - RawPoints[k].Time;
					if (timediff >= m / 2)
					{
						if (flag1)
						{
							zsd1 = k;
							flag1 = false;
						}
						else if (timediff >= m)
						{
							zsd2 = k;
							flag2 = false;
						}
					}
					k--;
				}

				if (zsd1 != -1 && zsd2 != -1)
				{

					double x1 = RawPoints[zsd2].X;
					double y1 = RawPoints[zsd2].Y;
					double x2 = RawPoints[zsd1].X;
					double y2 = RawPoints[zsd1].Y;
					double x3 = p.X;
					double y3 = p.Y;
					double[] vector1 = new double[2] { x1 - x2, y1 - y2 };
					double[] vector2 = new double[2] { x2 - x3, y2 - y3 };
					double dot = vector1[0] * vector2[0] + vector1[1] * vector2[1];
					double mag1 = Math.Sqrt(Math.Pow(vector1[0], 2) + Math.Pow(vector1[1], 2));
					double mag2 = Math.Sqrt(Math.Pow(vector2[0], 2) + Math.Pow(vector2[1], 2));
					double angle = Math.Acos(dot / (mag1 * mag2));

					if (angle >= threshold)
					{
						rvalue = zsd1;
					}


				}

			}
			return rvalue;
		}

		/// <summary>
		/// Used for testing
		/// </summary>
		/// <returns></returns>
		public static List<long[]> SeparateGestureMulti(Point[] RawPoints)
		{
			double threshold = 25 * Math.PI / 180;
			int lp = 0;
			int j = 1;

			//TimePointF v1;
			Point v2 = new Point(0, 0, 0);
			List<long[]> change_indexes = new List<long[]>();

			List<Point> current_list = new List<Point>();
			long start_time = RawPoints[0].Time;
			int current_stroke = RawPoints[0].StrokeID;

			int m = 100;

			for (int i = 0; i < RawPoints.Length; i++)
			{
				Point p = RawPoints[i];

				if (p.StrokeID != current_stroke)
				{
					Point g = RawPoints[i - 1];
					change_indexes.Add(new long[3] { i - 1, g.Time - start_time, 1 });
					j = 0;
					lp = i;
					current_list.Clear();
					current_stroke = p.StrokeID;
					continue;
				}

				double elapsed; // = RawPoints.Last().Time - RawPoints[lp].Time; ;


				if (RawPoints[i].StrokeID == RawPoints[lp].StrokeID)
				{
					elapsed = RawPoints[i].Time - RawPoints[lp].Time;
				}
				else
				{
					elapsed = RawPoints[i].Time - RawPoints.Where(x => x.StrokeID == RawPoints[i].StrokeID).First().Time;
				}

				if (elapsed >= m * 2)
				{
					int k = i;
					double time = p.Time;
					double timediff = 0;
					int zsd1 = -1;
					int zsd2 = -1;
					bool flag1 = true;
					bool flag2 = true;
					while (k >= 0 && flag2)
					{
						timediff = time - RawPoints[k].Time;
						if (timediff >= m / 2)
						{
							if (flag1)
							{
								zsd1 = k;
								flag1 = false;
							}
							else if (timediff >= m)
							{
								zsd2 = k;
								flag2 = false;
							}
						}
						k--;
					}

					if (zsd1 != -1 && zsd2 != -1)
					{
						double x1 = RawPoints[zsd2].X;
						double y1 = RawPoints[zsd2].Y;
						double x2 = RawPoints[zsd1].X;
						double y2 = RawPoints[zsd1].Y;
						double x3 = p.X;
						double y3 = p.Y;

						double[] vector1 = new double[2] { x1 - x2, y1 - y2 };
						double[] vector2 = new double[2] { x2 - x3, y2 - y3 };
						double dot = vector1[0] * vector2[0] + vector1[1] * vector2[1];
						double mag1 = Math.Sqrt(Math.Pow(vector1[0], 2) + Math.Pow(vector1[1], 2));
						double mag2 = Math.Sqrt(Math.Pow(vector2[0], 2) + Math.Pow(vector2[1], 2));
						double angle = Math.Acos(dot / (mag1 * mag2));


						if (angle > threshold)
						{
							//i = current_list.Count / 2;
							change_indexes.Add(new long[3] { zsd1, p.Time - start_time, 0 });
							j = 0;
							lp = zsd1;
							current_list.Clear();
						}
					}
				}
				current_list.Add(p);
				j++;
			}

			int z = RawPoints.Length - 1;
			Point pz = RawPoints[z];
			change_indexes.Add(new long[3] { z, pz.Time - start_time, 1 });
			return (change_indexes);
		}

		/// <summary>
		/// Classify method without part recognition
		/// </summary>
		/// <returns></returns>
		public static string ClassifyFull(Gesture candidate, Gesture[] templateSet, bool complete = false, bool stroke_complete = false, bool allow_excess_strokes = false)
		{
			List<Gesture> matchingDataset = new List<Gesture>();

			matchingDataset.AddRange(templateSet.Where(x => x.StrokeNumber == candidate.StrokeNumber));

			if (allow_excess_strokes && matchingDataset.Count < 1)
			{
				matchingDataset.AddRange(templateSet);
			}

			return CustomMatch(candidate, matchingDataset);
		}
	}
}