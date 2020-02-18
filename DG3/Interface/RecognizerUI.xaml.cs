using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using DG3;
using RecPoint = DG3.Point;

namespace GestureStudio
{
	/// <summary>
	/// Interaction logic for the Recognizer's UI
	/// </summary>
	public partial class RecognizerWindow : Window
	{
		DispatcherTimer _tm;
		Gesture[] gesture_dataset;

		// History
		List<Gesture> candidates = new List<Gesture>();
		List<Gesture> recognized = new List<Gesture>();

		List<RecPoint> points = new List<RecPoint>();
		private bool isMouseDown = false;
		private int strokeIndex = -1;
		private int lastRecognizedPointIndex = 0;
		private Stopwatch gestureTime = new Stopwatch();
		bool allow_excess_strokes = false;
		bool highlight_parts = false;
		bool ishighlighted = false;
		int g = 0;

		public RecognizerWindow(Gesture[] selected_dataset = null)
		{
			InitializeComponent();
			gesture_dataset = selected_dataset;
			if (selected_dataset == null || selected_dataset.Length <= 0)
			{
				FolderBrowserDialog dlg = new FolderBrowserDialog();
				dlg.Description = "Select dataset folder.";
				dlg.SelectedPath = Directory.GetCurrentDirectory();
				if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					gesture_dataset = GestureIOCustom.LoadTrainingSet(dlg.SelectedPath);
				}
			}
			if (gesture_dataset == null || gesture_dataset.Length <= 0)
			{
				this.Close();
			}
			else
			{
				ShowDataset(false);
			}
		}
	
		public void GestureSurface_MouseDown(object sender, System.Windows.Input.MouseEventArgs e)
		{
			StopTimer();
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				if (strokeIndex == -1)
				{
					points = new List<RecPoint>();

					candidates = new List<Gesture>();
					recognized = new List<Gesture>();

					g = 0;
					GestureSurface.Children.Clear();
					PredictedName.Text = "";
					PredictedImage.Source = null;
					InvalidateVisual();
					lastRecognizedPointIndex = 0;
					gestureTime = new Stopwatch();
					gestureTime.Start();
				}
				else
				{
					RecognizeGesture_Thread(true, false);
				}
				isMouseDown = true;
				strokeIndex++;
			}
			else if (e.RightButton == MouseButtonState.Pressed)
			{
				RecognizeGesture_Thread(true, true);
				gestureTime.Stop();
				strokeIndex = -1;
			}
		}

		public void GestureSurface_MouseUp(object sender, System.Windows.Input.MouseEventArgs e)
		{
			isMouseDown = false;
			StartTimer();
		}


		public void GestureSurface_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if (!isMouseDown)
				return;
			Canvas c = (Canvas)sender;
			var point = e.GetPosition(c);
			RecPoint new_point = new RecPoint((float)point.X, (float)point.Y, strokeIndex, gestureTime.ElapsedMilliseconds);
			if (points.Count > 0)
			{
				RecPoint last = points.Last();

				if (new_point.Time - last.Time >= 5) // 200Hz
				{
					if (strokeIndex == last.StrokeID)
					{
						System.Windows.Shapes.Path current_path = new System.Windows.Shapes.Path();
						SolidColorBrush blackBrush = new SolidColorBrush();
						blackBrush.Color = ColorArray[g];
						current_path.Stroke = blackBrush;
						current_path.StrokeThickness = 3;
						GeometryGroup myGeometryGroup = new GeometryGroup();

						LineGeometry current_line = new LineGeometry();
						current_line.StartPoint = new System.Windows.Point(last.X, last.Y);
						current_line.EndPoint = new System.Windows.Point(point.X, point.Y);

						current_path.Data = current_line;
						c.Children.Add(current_path);
						int lrp = Recognizer.DividePart(points, new_point, lastRecognizedPointIndex);
						if (lrp > -1)
						{
							RecognizeGesture_Thread(false, false, lrp);
						}
					}
					points.Add(new_point);
				}
			}
			else
			{
				points.Add(new_point);
			}
		}

		

		public void RecognizeGesture_Thread(bool strokeEnd, bool gestureEnd, int lrp = -1)
		{
			if (lrp == -1)
			{
				lrp = points.Count - 1;
			}
			lastRecognizedPointIndex = lrp;
			Action a;
			a = () =>
			{
				RecognizeGesture(points, strokeEnd, gestureEnd, lastRecognizedPointIndex);
				if (strokeEnd && gestureEnd)
				{
					strokeIndex = -1;
				}
			};
			var t = new Thread(a.Invoke);
			t.Start();
			if (highlight_parts)
			{
				g = (g + 1) % ColorArray.Length;
				var paths = GestureSurface.Children.OfType<System.Windows.Shapes.Path>();
				int i = paths.Count() - 1;
				while (i >= lrp)
				{
					var path = paths.ElementAt(i);
					SolidColorBrush blackBrush = new SolidColorBrush();
					blackBrush.Color = ColorArray[g];
					path.Stroke = blackBrush;
					i--;
				}
			}
		}

		public void RecognizeGesture(List<RecPoint> points_at, bool strokeEnd, bool gestureEnd, int lp)
		{
		Gesture candidate = new Gesture(points_at.GetRange(0,lp+1).ToArray(), "", points_at[lp].StrokeID + 1);
			string gestureClass = "";
			gestureClass = DG3.Recognizer.ClassifyParts(candidate, gesture_dataset, gestureEnd, strokeEnd, allow_excess_strokes);

			candidates.Add(candidate);
			if (gestureClass == "")
			{
				Gesture np = new Gesture(null, "Exceeded max number of strokes");
				recognized.Add(np);
				System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => {
					PredictedName.Text = "Exceeded max number of strokes";
					PredictedImage.Source = RenderImages.RenderToPNGImageSource(np);

				}));
			}
			else if (gestureClass.Contains('['))
			{
				string gestureTemplate = gestureClass.Split('[')[0].TrimEnd();
				int nstrokes = gestureClass.Split(',').Count();
				foreach (Gesture g in gesture_dataset)
				{
					if (g.Name == gestureTemplate)
					{
						foreach (Gesture gp in g.Part_Combinations[nstrokes])
						{
							if (gp.Name == gestureClass)
							{
								recognized.Add(gp);
								System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => {
									PredictedName.Text = "Recognized as: " + gestureClass;
									PredictedImage.Source = RenderImages.RenderToPNGImageSource(gp);
								}));
								break;

							}
						}
						break;
					}
				}
			}
			else
			{
				foreach (Gesture g in gesture_dataset)
				{
					if (g.Name == gestureClass)
					{
						recognized.Add(g);
						System.Windows.Application.Current.Dispatcher.Invoke(new Action(() => {
							PredictedName.Text = "Recognized as: " + gestureClass;
							PredictedImage.Source = RenderImages.RenderToPNGImageSource(g);
						}));
						break;
					}
				}
			}



		}
		

		private void StartTimer()
		{
			_tm = new DispatcherTimer();
			_tm.Interval = TimeSpan.FromSeconds(1);
			_tm.Tick += new EventHandler(_tm_Elapsed);
			_tm.Start();
		}
		private void StopTimer()
		{
			if (_tm != null)
			{
				_tm.Stop();
				_tm.Tick -= new EventHandler(_tm_Elapsed);
			}

		}
		void _tm_Elapsed(object sender, EventArgs e)
		{
			StopTimer();
			gestureTime.Stop();
			RecognizeGesture_Thread(true, true);
		}

		private void return_btn_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
		
		private void highlight_btn_Click(object sender, RoutedEventArgs e)
		{
			DatasetGrid.Children.Clear();
			ishighlighted = !ishighlighted;
			ShowDataset(ishighlighted);
		}
		private void history_btn_Click(object sender, RoutedEventArgs e)
		{
			RecHistory recHis = new RecHistory(candidates, recognized);
			recHis.Show();
		}


		private void viewGestureSkeleton(object sender, EventArgs e)
		{
			DockPanel dp = (DockPanel) ((Border)sender).Child;
			string gestureclass = (string)((System.Windows.Controls.Label)(dp.Children[0])).Content;
			List<Gesture> samples = (List<Gesture>)gesture_dataset.Where(x => x.Name.StartsWith(gestureclass)).ToList();
			ViewSamplesWindow _viewFrm = new ViewSamplesWindow(samples);
			_viewFrm.Owner = this;
			_viewFrm.Show();
		}
		private void ShowDataset(bool HighlightStrokes, bool HighlightParts=false)
		{
			int row_index = 0, column_index = 0;
			foreach (Gesture g in gesture_dataset)
			{
				if (g.SampleNumber == "01")
				{
					if (column_index >= DatasetGrid.ColumnDefinitions.Count)
					{
						column_index = 0;
						row_index++;
					}
					if (row_index >= DatasetGrid.RowDefinitions.Count)
					{
						DatasetGrid.RowDefinitions.Add(new RowDefinition());
					}

					var dp = new DockPanel();
					var txt = new System.Windows.Controls.Label();
					string pattern = @"\d+$";
					string replacement = "";
					Regex rgx = new Regex(pattern);
					txt.Content = rgx.Replace(g.Name, replacement);
					var img = new System.Windows.Controls.Image();

					img.Source = RenderImages.RenderToPNGImageSource(g,HighlightStrokes, HighlightParts);
					img.Width = 96;
					img.Height = 96;
					img.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
					img.VerticalAlignment = System.Windows.VerticalAlignment.Center;
					txt.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
					txt.VerticalAlignment = System.Windows.VerticalAlignment.Center;

					RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);

					DockPanel.SetDock(txt, Dock.Top);
					DockPanel.SetDock(img, Dock.Top);
					
					dp.Children.Add(txt);
					dp.Children.Add(img);
					

					Border b = new Border();
					b.BorderBrush = new SolidColorBrush(Colors.DarkBlue);
					b.BorderThickness = new Thickness(0);
					b.Background = System.Windows.Media.Brushes.Transparent;
					b.Child = dp;
					b.Width = 164;
					b.Height = 164;

					Grid.SetRow(b, row_index);
					Grid.SetColumn(b, column_index);
					DatasetGrid.Children.Add(b);
					b.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(viewGestureSkeleton);
					b.MouseEnter += new System.Windows.Input.MouseEventHandler(HighlightSelection);
					b.MouseLeave += new System.Windows.Input.MouseEventHandler(MouseLeaveSelection);
					column_index++;
				}

			}
		}

		private void HighlightSelection(object sender, EventArgs e)
		{
			Border b = (Border)sender;
			b.BorderThickness = new Thickness(1);
		}
		private void MouseLeaveSelection(object sender, EventArgs e)
		{
			Border b = (Border)sender;
			b.BorderThickness = new Thickness(0);
		}

		
		private void check_ignore_maxstrokes_Checked(object sender, RoutedEventArgs e)
		{
			allow_excess_strokes = !allow_excess_strokes;
		}
		
		System.Windows.Media.Color[] ColorArray = new System.Windows.Media.Color[]
		{
			Colors.Orange,
			Colors.AliceBlue,
			Colors.DarkRed,
			Colors.LawnGreen,
			Colors.MediumPurple
		};
		
		private void check_highlight_parts_Checked(object sender, RoutedEventArgs e)
		{
			highlight_parts = !highlight_parts;
		}
	}
}
