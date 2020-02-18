using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.IO;

namespace GestureStudio
{
	class SampleGenerator
	{
		public static string SamplesFolder = "../../Data/samples/";
		public static int nsamples = 49;
		public static string ConnectivityErrorMessage = "Sample Generation failed, please ensure that there's an active internet connection and that G3's web service is up.";
		public static string MinExpressionsErrorMessage = "Please add at least one expression to the collection before starting the generation process.";

		public static async Task GeneratePoints(string datasetName, List<Expression> rules)
		{
			List<Task> tasks = new List<Task>();
			double m_factor = 20; //scaling factor
			int ms = 5; //200 Hz
			double speed = 0.1; //0.1 expression space units for every 5 ms
			for (int z = 0; z < rules.Count; z++)
			{
				string code;
				string title;
				string[] code_strokes;

				code = rules[z].expression;
				title = rules[z].title;

				if (code == "")
					break;
				code_strokes = code.Split(new[] { "|=" }, StringSplitOptions.None);

				List<List<List<int[]>>> strokes = new List<List<List<int[]>>>();
	  
				for (int g_i = 0; g_i < code_strokes.Length; g_i++)
				{
					string[] instructions = code_strokes[g_i].Split('+');
					int l = instructions.Length;

					Point last_position = new Point();

					List <List <int[]>> parts = new List<List<int[]>>();
					double global_time = 0;
					for (int i = 0; i < l; i++)
					{
						string inst = instructions[i].Trim();
						string action = inst.Split('(')[0];
						double x = Int32.Parse(inst.Split('(')[1].Split(',')[0]);
						double y = Int32.Parse(inst.Split(')')[0].Split(',')[1]);
		  
						x = x * m_factor;
						y = y * m_factor;

						switch (action)
						{
							case "P":
								last_position.X = x;
								last_position.Y = y;
								break;

							case "L":
							{ 
								double x2 = last_position.X + x;
								double y2 = last_position.Y + y;
								double x1 = last_position.X;
								double y1 = last_position.Y;

								double distance = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));

								double dx = x2 - x1;
								double dy = y2 - y1;
								double xm1 = 0;
								double ym1 = 0;
								List<int[]> current_part = new List<int[]>();
								current_part.Add(ArrayElement(x1, y1, global_time));

								global_time += ms;
								for (double j = speed * m_factor; j <= distance; j += speed * m_factor)
								{
									double xm2 = dx / distance * j;
									double ym2 = dy / distance * j;
									current_part.Add(ArrayElement(x1 + xm2, y1 + ym2, global_time));
									xm1 = xm2;
									ym1 = ym2;
									global_time += ms;
								}
								last_position.X = x2;
								last_position.Y = y2;
								parts.Add(current_part);
								break;
							}
							case "A_ACW":
							{
									double x2 = last_position.X + x;
									double y2 = last_position.Y + y;
									double x1 = last_position.X;
									double y1 = last_position.Y;

									double p1_x;
									double p1_y;
									double p2_x;
									double p2_y;

									double tan_x;
									double tan_y;
									double cen_x;
									double cen_y;

									double mid_x = (x1 + x2) / 2;
									double mid_y = (y1 + y2) / 2;
									double slp = -(x2 - x1) / (y2 - y1);
									double le = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2)) / 2;

									if (slp == 0)
									{
										p1_x = mid_x + le;
										p1_y = mid_y;

										p2_x = mid_x - le;
										p2_y = mid_y;
									}

									// if slope is infinte 
									else if (Double.IsInfinity(slp))
									{
										p1_x = mid_x;
										p1_y = mid_y + le;

										p2_x = mid_x;
										p2_y = mid_y - le;
									}
									else
									{
										double dxe = (le / Math.Sqrt(1 + (slp * slp)));
										double dye = slp * dxe;
										p1_x = mid_x + dxe;
										p1_y = mid_y + dye;
										p2_x = mid_x - dxe;
										p2_y = mid_y - dye;
									}

									if (Orientation(x1, y1, p1_x, p1_y, x2, y2) == 1)
									{

										tan_x = p1_x;
										tan_y = p1_y;
										cen_x = p2_x;
										cen_y = p2_y;
									}
									else
									{
										tan_x = p2_x;
										tan_y = p2_y;
										cen_x = p1_x;
										cen_y = p1_y;
									}

									if (Orientation(x1, y1, p1_x, p1_y, x2, y2) == 2)
									{
										tan_x = p1_x;
										tan_y = p1_y;
										cen_x = p2_x;
										cen_y = p2_y;
									}
									else
									{
										tan_x = p2_x;
										tan_y = p2_y;
										cen_x = p1_x;
										cen_y = p1_y;
									}

									double rad = Math.Sqrt(Math.Pow(x2 - tan_x, 2) + Math.Pow(y2 - tan_y, 2));
									
									double starting_angle = Math.Atan2(y1 - cen_y, x1 - cen_x);
									double ending_angle = Math.Atan2(y2 - cen_y, x2 - cen_x);

									if (starting_angle > ending_angle)
									{
										ending_angle = Math.PI * 2 + ending_angle;
									}


									double distance = Math.Abs((ending_angle - starting_angle) * rad);

									double dx = x2 - x1;
									double dy = y2 - y1;
									double xm1 = x1;
									double ym1 = y1;
									List<int[]> current_part = new List<int[]>();
									current_part.Add(ArrayElement(x1, y1, global_time));

									global_time += ms;
									double last_j = 0;
									double j;
									double cpx=0;
									double cpy=0;

									for (j = speed * m_factor; j <= distance; j += speed * m_factor)
									{

										double angle = j / rad + starting_angle;

										double xm2 = rad * Math.Cos(angle) + cen_x;
										double ym2 = rad * Math.Sin(angle) + cen_y;
										cpx = xm2;
										cpy = ym2;
										current_part.Add(ArrayElement(cpx, cpy, global_time));
										double bdst = Math.Sqrt(Math.Pow(cpx - xm1, 2) + Math.Pow(cpy - ym1, 2));
	
										xm1 = cpx;
										ym1 = cpy;
										global_time += ms;
										last_j = j;

									}

									if (j != distance)
									{
										global_time -= ms;
										double lx = cpx;
										double ly = cpy;
										double dst = Math.Sqrt(Math.Pow(x2 - lx, 2) + Math.Pow(y2 - ly, 2));
										global_time += (dst / (m_factor * speed)) * ms;
										current_part.Add(ArrayElement(x2, y2, global_time));
										global_time += ms;
									}

									last_position.X = x2;
									last_position.Y = y2;
									parts.Add(current_part);
									break;
								}
							

							case "A_CW":
							{
									double x2 = last_position.X + x;
									double y2 = last_position.Y + y;
									double x1 = last_position.X;
									double y1 = last_position.Y;

									double p1_x;
									double p1_y;
									double p2_x;
									double p2_y;

									double tan_x;
									double tan_y;
									double cen_x;
									double cen_y;

									// https://www.geeksforgeeks.org/find-points-at-a-given-distance-on-a-line-of-given-slope/

									double mid_x = (x1 + x2) / 2;
									double mid_y = (y1 + y2) / 2;
									double slp = -(x2 - x1) / (y2 - y1);
									double le = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2))/2;

									if (slp == 0)
									{
										p1_x = mid_x + le;
										p1_y = mid_y;

										p2_x = mid_x - le;
										p2_y = mid_y;
									}

									// if slope is infinte 
									else if (Double.IsInfinity(slp))
									{
										p1_x = mid_x;
										p1_y = mid_y + le;

										p2_x = mid_x;
										p2_y = mid_y - le;
									}
									else
									{
										double dxe = (le / Math.Sqrt(1 + (slp * slp)));
										double dye = slp * dxe;
										p1_x = mid_x + dxe;
										p1_y = mid_y + dye;
										p2_x = mid_x - dxe;
										p2_y = mid_y - dye;
									}

									if (Orientation(x1, y1, p1_x, p1_y, x2, y2) == 1)
									{
										
										tan_x = p1_x;
										tan_y = p1_y;
										cen_x = p2_x;
										cen_y = p2_y;
									}
									else
									{
										tan_x = p2_x;
										tan_y = p2_y;
										cen_x = p1_x;
										cen_y = p1_y;
									}


									double rad = Math.Sqrt(Math.Pow(x2 - tan_x, 2) + Math.Pow(y2 - tan_y, 2));
									double rad2 = Math.Sqrt(Math.Pow(x2 - cen_x, 2) + Math.Pow(y2 - cen_y, 2));
									double rad3 = Math.Sqrt(Math.Pow(x1 - cen_x, 2) + Math.Pow(y1 - cen_y, 2));
									double rad4 = Math.Sqrt(Math.Pow(x1 - tan_x, 2) + Math.Pow(y1 - tan_y, 2));
									
									double starting_angle = Math.Atan2(y1 - cen_y, x1 - cen_x);
									double ending_angle = Math.Atan2(y2 - cen_y, x2 - cen_x);
									;
									
									if (starting_angle < ending_angle)
									{
										starting_angle = Math.PI * 2 + starting_angle;
									}

									double distance = Math.Abs((ending_angle - starting_angle) * rad);

									double dx = x2 - x1;
									double dy = y2 - y1;
									double xm1 = x1;
									double ym1 = y1;

									List<int[]> current_part = new List<int[]>();
									current_part.Add(ArrayElement(x1, y1, global_time));

									global_time += ms;

									double last_j = 0;
									double j;
									double cpx=0;
									double cpy=0;
									for (j = speed * m_factor; j <= distance; j += speed * m_factor)
									{

										double angle = starting_angle - (j / rad);

										double xm2 = rad * Math.Cos(angle) + cen_x;
										double ym2 = rad * Math.Sin(angle) + cen_y;
										cpx = xm2;
										cpy = ym2;
										current_part.Add(ArrayElement(cpx, cpy, global_time));
										double bdst = Math.Sqrt(Math.Pow(cpx - xm1, 2) + Math.Pow(cpy - ym1, 2));
										xm1 = cpx;
										ym1 = cpy;
										global_time += ms;
										last_j = j;
									}

									if (j != distance)
									{
										global_time -= ms;
										double lx = cpx;
										double ly = cpy;
										double dst = Math.Sqrt(Math.Pow(x2 - lx, 2) + Math.Pow(y2 - ly, 2));
										global_time += (dst / (m_factor * speed)) * ms;
										current_part.Add(ArrayElement(x2, y2, global_time));
										global_time += ms;
									}

									last_position.X = x2;
									last_position.Y = y2;
									parts.Add(current_part);
									break;
							}
						}
					}
					strokes.Add(parts);
				}
				if (strokes.Count != 0)
				{
					List<List<int[]>> strokes_flat = new List<List<int[]>>();
					List <int[]> part_indexes = new List<int[]>();

					int last_length = 0;
					for (int ii = 0; ii < strokes.Count; ii++)
					{
						List<int[]> part_points = new List<int[]>();
						for (int jj = 0; jj < strokes[ii].Count; jj++)
						{
							part_points.AddRange(strokes[ii][jj]);
							last_length = last_length + strokes[ii][jj].Count;
							part_indexes.Add(new int[2] { ii, last_length - 1 });
						}
						strokes_flat.Add(part_points);
					}
					tasks.Add(G3_Call(datasetName, title, strokes_flat, part_indexes));

				}
			}
			foreach (Task t in tasks)
			{
				await t;
			}
		}

		public static async Task G3_Call(string datasetName, string title, List<List<int[]>> strokes, List<int[]> part_indexes)
		{
			HttpClient client = new HttpClient();
			string strokes_string = JsonConvert.SerializeObject(strokes); //FIX MULTIPLE
			var stringContent = new StringContent("{\"strokes\": " + strokes_string + "}", UnicodeEncoding.UTF8, "application/json");

			var response = await client.PostAsync("https://g3.prhlt.upv.es/reconstruct", stringContent);
			string responseString = await response.Content.ReadAsStringAsync();
			JObject responseJSON = JsonConvert.DeserializeObject<JObject>(responseString);

			stringContent = new StringContent("{\"reconstruction\": " + responseJSON["result"].ToString() + ", \"nsamples\":" + nsamples + ", \"shapevar\": 1.0, \"sametimes\": true}", UnicodeEncoding.UTF8, "application/json");
			response = await client.PostAsync("https://g3.prhlt.upv.es/generate", stringContent);

			responseString = await response.Content.ReadAsStringAsync();
			responseJSON = JsonConvert.DeserializeObject<JObject>(responseString);
			JToken gesturesJ = responseJSON["result"];

			List<List<List<int[]>>> gestures = new List<List<List<int[]>>>();
			gestures.Add(strokes);
			foreach (JArray g in gesturesJ)
			{
				JArray gs = JsonConvert.DeserializeObject<JArray>(g.ToString());
				List<List<int[]>> gs_strokes = new List<List<int[]>>();
				foreach (var stroke in gs)
				{
					List<int[]> gs_points = new List<int[]>();
					foreach (var point in stroke)
					{
						int x = (int)point[0];
						int y = (int)point[1];
						int t = (int)point[2];
						int p = (int)point[3];
						gs_points.Add(new int[] { x, y, t, p });
					}
					gs_strokes.Add(gs_points);
				}
				gestures.Add(gs_strokes);
			}
			GenerateXml(datasetName, title, gestures, part_indexes);
		}

		public static void GenerateXml(string DatasetName, string title, List<List<List<int[]>>> samples, List<int[]> part_indexes)
		{
			string StoreFolder = SamplesFolder + DatasetName + "//";
			Directory.CreateDirectory(StoreFolder);

			XmlWriterSettings xmlsettings = new XmlWriterSettings();
			xmlsettings.Indent = true;

			int i = 1;
			foreach (List<List<int[]>> sample in samples)
			{

				string gesture_title = title.ToLower() + i.ToString("D2");
				string path = StoreFolder + gesture_title + ".xml";

				XmlWriter xmlWriter = XmlWriter.Create(path, xmlsettings);

				string gesture_num_points = part_indexes.Last()[1].ToString();
				DateTime gesture_datetime = DateTime.Now;
				string gesture_date = gesture_datetime.ToString("yyyy/%M/%d");
				string gesture_daytime = gesture_datetime.ToString("%H:%m:%s");
				int max = 0;
				foreach (List<int[]> stroke in sample)
				{
					if (stroke.Last()[2] > max)
					{
						max = stroke.Last()[2];
					}
				}
				string gesture_time = max.ToString();

				xmlWriter.WriteStartDocument();
				xmlWriter.WriteStartElement("Gesture");
				xmlWriter.WriteAttributeString("AppName", "Gestures");
				xmlWriter.WriteAttributeString("AppVer", "3.5.0.0");
				xmlWriter.WriteAttributeString("Date", gesture_date);
				xmlWriter.WriteAttributeString("Milliseconds", gesture_time);
				xmlWriter.WriteAttributeString("Name", gesture_title);
				xmlWriter.WriteAttributeString("NumPts", gesture_num_points);
				xmlWriter.WriteAttributeString("Speed", "Custom");
				xmlWriter.WriteAttributeString("Subject", "Custom");
				xmlWriter.WriteAttributeString("TimeOfDay", gesture_daytime);


				xmlWriter.WriteStartElement("Partition");
				foreach (int[] index in part_indexes)
				{
					xmlWriter.WriteStartElement("Spec");
					xmlWriter.WriteAttributeString("Stroke", index[0].ToString());
					xmlWriter.WriteAttributeString("Index", index[1].ToString());
					xmlWriter.WriteEndElement();
				}
				xmlWriter.WriteEndElement();

				int stroke_index = 1;
				foreach (var stroke in sample)
				{
					xmlWriter.WriteStartElement("Stroke");
					xmlWriter.WriteAttributeString("index", stroke_index.ToString());
					foreach (var point in stroke)
					{
						xmlWriter.WriteStartElement("Point");
						xmlWriter.WriteAttributeString("T", point[2].ToString());
						xmlWriter.WriteAttributeString("X", point[0].ToString());
						xmlWriter.WriteAttributeString("Y", point[1].ToString());
						xmlWriter.WriteEndElement();
					}
					xmlWriter.WriteEndElement();
					stroke_index++;
				}
				xmlWriter.WriteEndElement();
				xmlWriter.WriteEndDocument();
				xmlWriter.Close();
				i++;
			}
		}

		public static int[] ArrayElement(double x, double y, double t)
		{
			return new int[] { (int) Math.Round(x) + 600,  200 - (int) Math.Round(y), (int) Math.Round(t), 1 };
		}
		public static int Orientation(double p1_x, double p1_y, double p2_x, double p2_y, double p3_x, double p3_y)
		{
			double val = (p2_y - p1_y) * (p3_x - p2_x) -
					  (p2_x - p1_x) * (p3_y - p2_y);

			if (val == 0) return 0;  // colinear

			return (val > 0) ? 1 : 2; // clock or counterclock wise
		}
		
	}
}
