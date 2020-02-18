/**
 * Extract from The $1 Unistroke Recognizer (C# version)
 *
 *		Jacob O. Wobbrock, Ph.D.
 * 		The Information School
 *		University of Washington
 *		Mary Gates Hall, Box 352840
 *		Seattle, WA 98195-2840
 *		wobbrock@u.washington.edu
 *
 *		Andrew D. Wilson, Ph.D.
 *		Microsoft Research
 *		One Microsoft Way
 *		Redmond, WA 98052
 *		awilson@microsoft.com
 *
 *		Yang Li, Ph.D.
 *		Department of Computer Science and Engineering
 * 		University of Washington
 *		The Allen Center, Box 352350
 *		Seattle, WA 98195-2840
 * 		yangli@cs.washington.edu
 *
 * The Protractor enhancement was published by Yang Li and programmed here by 
 * Jacob O. Wobbrock.
 *
 *	Li, Y. (2010). Protractor: A fast and accurate gesture 
 *	  recognizer. Proceedings of the ACM Conference on Human 
 *	  Factors in Computing Systems (CHI '10). Atlanta, Georgia
 *	  (April 10-15, 2010). New York: ACM Press, pp. 2169-2172.
 * 
 * The $1 Unistroke Recognizer (C# version) is distributed under the "New BSD License" agreement:
 * 
 * Copyright (c) 2007-2011, Jacob O. Wobbrock, Andrew D. Wilson and Yang Li.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *    * Redistributions of source code must retain the above copyright
 *      notice, this list of conditions and the following disclaimer.
 *    * Redistributions in binary form must reproduce the above copyright
 *      notice, this list of conditions and the following disclaimer in the
 *      documentation and/or other materials provided with the distribution.
 *    * Neither the names of the University of Washington nor Microsoft,
 *      nor the names of its contributors may be used to endorse or promote 
 *      products derived from this software without specific prior written
 *      permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS
 * IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL Jacob O. Wobbrock OR Andrew D. Wilson
 * OR Yang Li BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, 
 * OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
**/

using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DG3;

namespace CustomTests
{
	/// <summary>
	/// Tests made using the same format used in the $1 Recognizer
	/// </summary>
	public static class AccuracyTests
	{
		static bool ShowDebugMessages = true;
		static bool ShowSuccessMesssageBox = true;
		static string ExperimentOverMessage = "Experiment Over";
		static string samples_folder = "../../Data/samples/";
		static string dataset_folder = "../../Data/datasets/";
		public static void RunExperiment()
		{
			string selectedPath = dataset_folder + "1$/xml/xml_logs/";
			string dataset_dir = samples_folder + "/$1/";
			PrepareExperiment(selectedPath, dataset_dir);
			
		}
		public static void RunExperiment2()
		{
			string selectedPath = dataset_folder + "N$/mmg/";
			string dataset_dir = samples_folder + "$N/";
			string[] excludedGestureNames = new string[] { "line", "five_point_star" };
			PrepareExperiment(selectedPath, dataset_dir, excludedGestureNames);
		}

		public static void RunExperiment3()
		{
			string selectedPath = dataset_folder + "Mix/";
			string dataset_dir = samples_folder + "Mix/";
			string[] excludedGestureNames = new string[] { "line", "five_point_star", "rectangle", "check", "caret", "left_sq_bracket", "right_sq_bracket", "\\v", "delete_mark", "\\x", "-T-", "-D-", "-X-", "arrowhead", "-I-", "exclamation_point" };
			PrepareExperiment(selectedPath, dataset_dir, excludedGestureNames);
		}

		public static void RunExperiment4()
		{
			string selectedPath = dataset_folder + "/$1+$N/";
			string dataset_dir = samples_folder + "$1+$N/";
			string[] excludedGestureNames = new string[] { "line", "five_point_star" };
			PrepareExperiment(selectedPath, dataset_dir, excludedGestureNames);
		}
		public static void RunExperiment5()
		{
			FolderBrowserDialog dlg1 = new FolderBrowserDialog();
			FolderBrowserDialog dlg2 = new FolderBrowserDialog();
			bool dialogResult=false;
			System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => {
				
				dlg1.Description = "Select samples folder";
				dlg1.SelectedPath = Directory.GetCurrentDirectory();
				if (dlg1.ShowDialog() == DialogResult.OK)
				{
					dlg2.Description = "Select templates folder";
					dlg2.SelectedPath = Directory.GetCurrentDirectory();
					if (dlg2.ShowDialog() == DialogResult.OK)
					{
						dialogResult = true;
					}
				}

			}));
			if (dialogResult)
			{
				PrepareExperiment(dlg1.SelectedPath, dlg2.SelectedPath);
			}
		}

		public static string[] TestBatchCustomParts(string subject, string speed, List<Category> categories, string dir, Gesture[] trainingSet)
		{
			StreamWriter mw = null;
			string[] filenames = new string[2];
			try
			{
				int start = Environment.TickCount;
				filenames[0] = String.Format("{0}\\$P_main_{1}.txt", dir, start); 

				mw = new StreamWriter(filenames[0], false, Encoding.UTF8);
				mw.WriteLine("Subject = {0}, Speed = {1}, StartTime(ms) = {2}", subject, speed, start);
				mw.WriteLine("Subject\tSpeed\tNumTraining\tGestureType\tCorrect\tRecognized\n");

				// determine the number of gesture categories and the number of examples in each one
				int numCategories = categories.Count;
				int numExamples = categories[0].NumExamples;

				for (int n = 0; n < numExamples; n++)
				{
					// storage for the final avg results for each category for this N
					//double[] results = new double[numCategories];
					for (int i = 0; i < numCategories; i++)

					{
						if (n >= categories[i].Count())
						{
							continue;
						}

						
						Gesture p = categories[i][n];

						Point[] testTimePts = p.PointsRaw;
						
						int nPoints = p.PointsRaw.Length;

						List<long[]> change_indexes = Recognizer.SeparateGestureMulti(testTimePts);
						long start_time = testTimePts[0].Time;
						long total_time = testTimePts[testTimePts.Length - 1].Time - start_time;
						for (int c_index = 0; c_index < change_indexes.Count; c_index++)
						{
							double point_percentage = 100 * (c_index + 1) / change_indexes.Count;
							double time_percentage = 100 * change_indexes[c_index][1] / total_time;

							int end_count = (int)change_indexes[c_index][0] + 1;
							Point[] testPtsPartial = new Point[end_count];
							Array.Copy(testTimePts, 0, testPtsPartial, 0, end_count);
							Gesture candidate = new Gesture(testPtsPartial,"", testPtsPartial[testPtsPartial.Length - 1].StrokeID + 1);

							string category = DG3.Recognizer.ClassifyParts(candidate, trainingSet, point_percentage == 100, change_indexes[c_index][2] == 1);

							int correct;
							String general_category = "";
							if (category.Contains("%"))
							{
								general_category = category.Split('(')[0];
								String result_percentage = category.Split('%')[1].Trim(')');
							}
							else if (category.Contains("["))
							{
								general_category = category.Split('[')[0].TrimEnd();
								general_category = Category.ParseName(general_category);
							}
							else
							{
								general_category = Category.ParseName(category);
							}
							correct = (general_category.ToLower() == categories[i].Name.ToLower()) ? 1 : 0;
							
							Category c = (Category)categories[i];

							mw.WriteLine("{0}\t{1}\tDG3\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}",
								subject,
								point_percentage,
								time_percentage, 
								speed,
								n,
								p.Name,
								candidate.StrokeNumber,
								correct,
								category,
								general_category
								);
						}


					}

				}

				// time-stamp the end of the processing
				int end = Environment.TickCount;
				mw.WriteLine("\nEndTime(ms) = {0}, Minutes = {1:F2}", end, Math.Round((end - start) / 60000.0, 2));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				filenames = null;
			}
			finally
			{
				if (mw != null)
					mw.Close();
			}
			return filenames;
		}

		public static List<Category> AssembleBatch(string[] filenames)
		{
			Dictionary<string, Category> categories = new Dictionary<string, Category>();
			for (int i = 0; i < filenames.Length; i++)
			{

				string filename = filenames[i];
				Gesture p = GestureIOCustom.ReadGesture(filename);

				string catName = Category.ParseName(p.Name); // e.g., "circle"
				if (categories.ContainsKey(catName))
				{
					Category cat = categories[catName];
					cat.AddExample(p); // if the category has been made before, just add to it
				}
				else // create new category
				{
					categories.Add(catName, new Category(catName, p));
				}
				
			}

			List<Category> list = null;
			if (categories != null)
			{
				list = new List<Category>(categories.Values);
			}
			return list;
		}

		public static void PrepareExperiment(string dlgSelectedPath, string dataset_dir, string[] excludedGestureClasses = null)
		{
			string path = String.Format("{0}\\{1}__{2}.txt", dlgSelectedPath, dlgSelectedPath.Substring(FindLastIndex(dlgSelectedPath) + 1), Environment.TickCount);
			string subject = "";
			string speed = "";
			Gesture[] trainingSet = DG3.GestureIOCustom.LoadTrainingSet(dataset_dir);
			string[] subjectDirectories = Directory.GetDirectories(dlgSelectedPath);
			for (int i = 0; i < subjectDirectories.Length; i++)
			{
				string[] speedDirectories = Directory.GetDirectories(subjectDirectories[i]);
				if (speedDirectories.Length == 0)
				{
					string[] sep_filename = subjectDirectories[i].Substring(FindLastIndex(subjectDirectories[i]) + 1).Split('-');
					if (sep_filename.Length > 1)
					{
						subject = sep_filename[0];
						for (int j = 1; j < sep_filename.Length - 1; j++)
						{
							subject += "-" + sep_filename[j];
						}
						speed = sep_filename[sep_filename.Length - 1];
					}
					else
					{
						subject = sep_filename[0];
					}
					Experiment(path, subjectDirectories[i], subject, speed, dlgSelectedPath, trainingSet, excludedGestureClasses);
				}
				else
				{
					subject = subjectDirectories[i].Substring(FindLastIndex(subjectDirectories[i]) + 1);
					for (int j = 0; j < speedDirectories.Length; j++)
					{
						speed = speedDirectories[j].Substring(FindLastIndex(speedDirectories[j]) + 1);
						Experiment(path, speedDirectories[j], subject, speed, dlgSelectedPath, trainingSet, excludedGestureClasses);
					}
				}
			}
			ExperimentOver();
		}
		public static void Experiment(string path, string dir, string subject, string speed, string selectedPath, Gesture[] trainingSet, string[] excludedGestureClasses = null)
		{
			if (ShowDebugMessages)
			{
				Console.WriteLine(dir);
			}
			string[] filenames = Directory.GetFiles(dir);
			List<string> filenames_removed = new List<string>();
			for (int i = 0; i < filenames.Length; i++)
			{
				if(filenames[i].EndsWith(".xml"))
				{
					if (excludedGestureClasses != null)
					{
						int j = 0;
						while (j < excludedGestureClasses.Length && !filenames[i].Contains(excludedGestureClasses[j]))
						{
							j++;
						}
						if (j == excludedGestureClasses.Length)
						{
							filenames_removed.Add(filenames[i]);
						}
					}
					else
					{
						filenames_removed.Add(filenames[i]);
					}
				}
			}
			
			List<Category> categories = AssembleBatch(filenames_removed.ToArray());

			if (categories != null)
			{
				string[] rstr = TestBatchCustomParts(
					subject,
					speed,
					categories,
					selectedPath,
					trainingSet
					);

				using (StreamWriter outfile = new StreamWriter(path, true, Encoding.UTF8))
				{
					if (rstr != null)
					{
						StreamReader r = new StreamReader(rstr[0], Encoding.UTF8);
						string line = "tmp";
						while ((line = r.ReadLine()) != String.Empty) { };
						while ((line = r.ReadLine()) == String.Empty) { };
						while (line != String.Empty)
						{
							outfile.WriteLine(line);
							line = r.ReadLine();

						}
						r.Close();
					}
					else // failure
					{
						string msg = String.Format("There was an error in the current log.");
						if (ShowDebugMessages)
						{
							Console.WriteLine(msg);
						}
						if (ShowSuccessMesssageBox)
						{
							MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						}
					}
				}
			}
			else // error assembling batch
			{
				string msg = String.Format("Error in the assembling batch of files.");
				if (ShowDebugMessages)
				{
					Console.WriteLine(msg);
				}
				if (ShowSuccessMesssageBox)
				{
					MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}
		public static void ExperimentOver()
		{
			if (ShowDebugMessages)
			{
				Console.WriteLine(ExperimentOverMessage);
			}
			if (ShowSuccessMesssageBox)
			{
				MessageBox.Show(ExperimentOverMessage, "Accuracy Test");
			}
		}
		private static int FindLastIndex(string path)
		{
			return Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));
		}
	}
}
 