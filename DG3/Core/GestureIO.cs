using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Text.RegularExpressions;
using System.Globalization;
using System;

namespace DG3
{
	public class GestureIOCustom
	{
		/// <summary>
		/// Reads a two-dimensional gesture from an XML file
		/// </summary>
		public static Gesture ReadGesture(string fileName)
		{
			List<Point> points = new List<Point>();
			Point last_point = new Point(0,0,-2,0);
			Point new_point;
			int not_unique = 0;
			List<Point> stroke_points = new List<Point>();
			Dictionary<int, List<int>> partition_indexes = new Dictionary<int, List<int>>();
			bool partition = false;
			XmlTextReader xmlReader = null;
			int currentStrokeIndex = -1;
			string gestureName = "";
			string sample_number = "";
			try
			{
				xmlReader = new XmlTextReader(File.OpenText(fileName));
				while (xmlReader.Read())
				{
					if (xmlReader.NodeType != XmlNodeType.Element) continue;
					switch (xmlReader.Name)
					{
						case "Gesture":
							gestureName = xmlReader["Name"];

							// MMG set compatibility
							if (gestureName.Contains("~"))
								gestureName = gestureName.Substring(0, gestureName.LastIndexOf('~'));
							if (gestureName.Contains("_"))
								gestureName = gestureName.Replace('_', ' ');

							// Old DG3 compatibility
							if (gestureName.Contains("%"))
								gestureName = gestureName.Substring(0, gestureName.LastIndexOf(')') + 1);

							sample_number = Regex.Match(fileName.Substring(0,fileName.Length-4), @"\d+$").Value;
							break;
						case "Stroke":
							if(currentStrokeIndex == -1 || stroke_points.Count - not_unique > 2)
							{
								points.AddRange(stroke_points);
								currentStrokeIndex++;
							}
							stroke_points.Clear();
							not_unique = 0;
							break;
						case "Point":
							new_point = new Point(
									float.Parse(xmlReader["X"], CultureInfo.InvariantCulture),
									float.Parse(xmlReader["Y"], CultureInfo.InvariantCulture),
									currentStrokeIndex == - 1 ? 0 : currentStrokeIndex,
									Convert.ToInt64(xmlReader["T"])
								);
							if (new_point.X == last_point.X && new_point.Y == last_point.Y &&
								new_point.Time == last_point.Time && new_point.StrokeID == last_point.StrokeID)
							{
								not_unique++;
							}
							
							if (currentStrokeIndex != -1)
							{
								stroke_points.Add(new_point);
							}
							else
							{
								points.Add(new_point);

							}
							last_point = new_point;
							
							break;
						case "Partition":
							partition = true;
							break;
						case "Spec":
							int parsed_stroke_index = int.Parse(xmlReader["Stroke"]);
							int parsed_index = int.Parse(xmlReader["Index"]);
							
							if (!partition_indexes.ContainsKey(parsed_stroke_index))
							{
								partition_indexes.Add(parsed_stroke_index, new List<int>());
							}

							partition_indexes[parsed_stroke_index].Add(parsed_index);
							break;
					}
				}
			}
			finally
			{
				if (currentStrokeIndex != -1)
				{
					points.AddRange(stroke_points);
				}
				if (xmlReader != null)
					xmlReader.Close();
			}
			if (partition)
			{
				return new Gesture(points.ToArray(), gestureName, currentStrokeIndex + 1, sample_number, false, false, partition_indexes);
			}
			else
			{
				return new Gesture(points.ToArray(), gestureName, currentStrokeIndex + 1, sample_number);
			}
			
		}

		public static Gesture[] LoadTrainingSet(string[] gestureFolders)
		{
			List<Gesture> gestures = new List<Gesture>();
			foreach (string folder in gestureFolders)
			{
				string[] gestureFiles = Directory.GetFiles(folder, "*.xml");
				foreach (string file in gestureFiles)
					gestures.Add(GestureIOCustom.ReadGesture(file));
			}
			return gestures.ToArray();
		}

		public static Gesture[] LoadTrainingSet(string folder)
		{
			List<Gesture> gestures = new List<Gesture>();

			string[] gestureFiles = Directory.GetFiles(folder, "*.xml");
			foreach (string file in gestureFiles)
			{
				gestures.Add(GestureIOCustom.ReadGesture(file));
			}
			return gestures.ToArray();
		}
	}
}