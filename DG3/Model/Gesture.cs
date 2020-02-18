using System;
using System.Collections.Generic;
using System.Linq;

namespace DG3
{
    public class Gesture
    {
		public Point[] PointsRaw = null;         // gesture points (original)
		public Point[] Points = null;            // gesture points (normalized)
        public string Name = "";                 // gesture class
		public string SampleNumber;
		public List<double> Vector;
		public int[][] LUT = null;               // lookup table

		public int StrokeNumber;
		public bool IsRoot = false;
		public bool IsPartial = false;
		public bool IsSubpart = false;

		public Dictionary<int, List<int>> Partition_Indexes = new Dictionary<int, List<int>>();
		public Dictionary<int, List<Gesture>> Part_Combinations = new Dictionary<int, List<Gesture>>();
		public Dictionary<int, List<Gesture>> Part_Combinations_Full = new Dictionary<int, List<Gesture>>();

		/// <summary>
		/// Constructs a gesture from an array of points
		/// </summary>
		/// <param name="points"></param>
		public Gesture(Point[] points, string gestureName = "", int nstrokes = 1, string nsample="", bool sub = false, bool part=false,  Dictionary<int, List<int>> partition = null)
        {
			this.Name = gestureName.ToLower();

			if (points == null)
			{
				return;
			}
            
			
            this.PointsRaw = new Point[points.Length];
            for (int i = 0; i < points.Length; i++)
                this.PointsRaw[i] = new Point(points[i].X, points[i].Y, points[i].StrokeID, points[i].Time);

			this.Points = QPointCloudRecognizer.Normalize(PointsRaw);
			
			
			this.SampleNumber = nsample;

			this.StrokeNumber = nstrokes;

			this.IsPartial = part;
			this.IsSubpart = sub;

			if (nstrokes == 1)
			{
				Vector = Dollar.Vectorize(this.Points);
			}

			LUT = QPointCloudRecognizer.ComputeLUT(this.Points);
			

			if (partition != null)
			{
				this.IsRoot = true;
				this.Partition_Indexes = partition;
				for (int i = 1; i <= nstrokes; i++)
				{
					Part_Combinations.Add(i, new List<Gesture>());
				}
				
				for (int i = 1; i < Math.Pow(2, nstrokes); i++)
				{
					char[] binary_string = Convert.ToString(i, 2).ToCharArray();
					List<bool> binary = new List<bool>();
					List<Point> part_points = new List<Point>();
					int j2 = 0;
					int part_strokes = 0;
					for (int j = 0; j < nstrokes; j++)
					{
						if (j < nstrokes - binary_string.Length)
						{
							binary.Add(false);
						}
						else
						{
							binary.Add(binary_string[j2] == '1');
							part_strokes += binary_string[j2] == '1' ? 1 : 0;
							j2++;
						}
					}
					string completed_name = this.Name + " [";
					for (int str = 0; str < this.StrokeNumber; str++)
					{
						completed_name += str + ",";
					}
					completed_name = completed_name.TrimEnd(',') + "]";

					string part_name = this.Name + " [";
					int[] part_counter = new int[]{ 0, 0 }; //stroke and part
					List<string> duplicate_list = new List<string>();
					for (part_counter[0] = 0; part_counter[0] < nstrokes; part_counter[0]++)
					{
						
						if (!binary[part_counter[0]])
						{
							continue;
						}
						for (part_counter[1] = 0; part_counter[1] < Partition_Indexes[part_counter[0]].Count; part_counter[1]++)
						{
							for (int j = 0; j < binary.Count; j++)
							{
								if (binary[j])
								{
									int previous_index = 0;
									int part_length = 0;
									if (j > 0)
									{
										previous_index = Partition_Indexes[j - 1][Partition_Indexes[j - 1].Count - 1] ;
									}
									if (j == part_counter[0])
									{
										part_length = Partition_Indexes[j][part_counter[1]] - previous_index;
										part_name += "#";
									}
									else
									{
										part_length = Partition_Indexes[j][Partition_Indexes[j].Count - 1] - previous_index;
									}
									
									Point[] part_points_b = new Point[part_length];

									part_name += j + ",";

									Array.Copy(PointsRaw, previous_index + 1, part_points_b, 0, part_length);
									for (int k = 0; k < part_points_b.Length; k++)
									{
										part_points.Add(part_points_b[k]);
									}
								}

							}
							part_name = part_name.TrimEnd(',') + "]";
							if (part_counter[1] + 1 < Partition_Indexes[part_counter[0]].Count)
							{
								part_name +=  " " + (part_counter[1] + 1) + "/" + Partition_Indexes[part_counter[0]].Count;
								Part_Combinations[part_strokes].Add(new Gesture(part_points.ToArray(), part_name, part_strokes, nsample,true,true));

							}
							else 
							{
								part_name = part_name.Replace("#","");
								if (part_name == completed_name)
								{
									part_name = this.Name;
								}
								if (!duplicate_list.Contains(part_name))
								{ 
									Part_Combinations[part_strokes].Add(new Gesture(part_points.ToArray(), part_name, part_strokes, nsample, true, false));
									duplicate_list.Add(part_name);
								}
							}
							
							part_name = this.Name + " [";
							part_points.Clear();
						}
						
					}	
				}
				for (int i = 1; i <= this.StrokeNumber; i++)
				{
					Part_Combinations_Full[i] = Part_Combinations[i].Where(x => !x.IsPartial).ToList();
				}
			}
		}

		public string FullName()
		{
			return (this.Name + this.SampleNumber);
		}
	}
}