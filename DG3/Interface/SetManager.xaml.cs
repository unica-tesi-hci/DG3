using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading;
using DG3;

namespace GestureStudio
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		List<Expression> SetExpressions = new List<Expression>();
		Gesture[] GestureDataset;
		
		// Canvas grid settings
		int GridSize = 20;
		System.Windows.Point GridCenter = new System.Windows.Point(10, 10);
		System.Windows.Point GridCenterRealSize = new System.Windows.Point(200, 200);

		int BrushType = 0;
		bool hold_part = false;
		bool arc_mode = false;
		bool clockwise = false;
		bool new_stroke = true;
		bool first = true;
		System.Windows.Point current_origin;
		LineGeometry current_line;
		ArcSegment current_arc;
		System.Windows.Shapes.Path current_path;
		PathFigure pathFigure;
		PathGeometry pathGeometry;
		GeometryGroup myGeometryGroup;
		Ellipse myEllipse;
		List<string> expression_strings = new List<string>();
		List<System.Windows.Point> expression_values = new List<System.Windows.Point>();
		int i = 0;

		public MainWindow()
		{
			InitializeComponent();

			ClearCanvas(paintSurface);
			NewDatasetName(DatasetNameTextBox);

		}
		public void ClearCanvas(Canvas c)
		{
			c.Children.Clear();
			
			for (int i = 0; i < c.Width; i+=20)
			{
				Line line = new Line();

				// Write Background Axis
				line.Stroke = Brushes.Gray;
				line.X1 = i;
				line.Y1 = 0;
				line.X2 = i;
				line.Y2 = c.Height;
				line.MouseDown += RectangleGeometry_MouseDown;
				line.MouseWheel += RectangleGeometry_MouseWheel;
				c.Children.Add(line);
			}
			for (int i = 0; i < c.Height; i += 20)
			{
				Line line = new Line();

				// Write Background Axis
				line.Stroke = Brushes.Gray;
				line.X1 = 0;
				line.Y1 = i;
				line.X2 = c.Width;
				line.Y2 = i;
				line.MouseDown += RectangleGeometry_MouseDown;
				line.MouseWheel += RectangleGeometry_MouseWheel;
				c.Children.Add(line);
			}

			Line line1 = new Line();

			// Write Background Axis
			line1.Stroke = Brushes.Black;
			line1.X1 = 0;
			line1.Y1 = c.Height / 2;
			line1.X2 = c.Width;
			line1.Y2 = c.Height / 2;
			line1.MouseDown += RectangleGeometry_MouseDown;
			line1.MouseWheel += RectangleGeometry_MouseWheel;
			c.Children.Add(line1);

			Line line2 = new Line();
			line2.Stroke = Brushes.Black;
			line2.X1 = c.Width / 2;
			line2.Y1 = 0;
			line2.X2 = c.Width / 2;
			line2.Y2 = c.Height;
			line2.MouseDown += RectangleGeometry_MouseDown;
			line2.MouseWheel += RectangleGeometry_MouseWheel;
			c.Children.Add(line2);
		}

		
		public void Canvas_MouseDown_1(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				current_origin = e.GetPosition((Canvas)sender);
				current_origin = SnapToGrid(current_origin);
				System.Windows.Point r = new System.Windows.Point(current_origin.X / GridSize - GridCenter.X, GridCenter.Y - (current_origin.Y / GridSize) );
				
				expression_values.Add(r);
				if (new_stroke)
				{
					if (!first)
					{
						this.expression_text_box.Text += " |= ";
					}
					else
					{
						first = false;
					}
					expression_strings.Add("P");
					new_stroke = false;
				}
				DrawPoint((Canvas)sender, current_origin);
				current_path = new System.Windows.Shapes.Path();
				SolidColorBrush blackBrush = new SolidColorBrush();
				blackBrush.Color = Colors.Black;
				current_path.Stroke = blackBrush;
				current_path.StrokeThickness = 3;
				myGeometryGroup = new GeometryGroup();
				if (arc_mode)
				{
					pathFigure = new PathFigure();
					current_arc = new ArcSegment();
					pathFigure.StartPoint = current_origin;
					current_arc.Point = current_origin;
					if (clockwise)
					{
						expression_strings.Add("A_CW");
						current_arc.SweepDirection = SweepDirection.Clockwise;
					}
					else
					{
						expression_strings.Add("A_ACW");
						current_arc.SweepDirection = SweepDirection.Counterclockwise;
					}
					PathSegmentCollection PathSegment = new PathSegmentCollection();
					PathSegment.Add(current_arc);
					pathFigure.Segments = PathSegment;
					pathGeometry = new PathGeometry(new List<PathFigure> { pathFigure });
					myGeometryGroup.Children.Add(pathGeometry);
					current_path.Data = myGeometryGroup;
					paintSurface.Children.Add(current_path);
				}
				else
				{
					expression_strings.Add("L");
					current_line = new LineGeometry();
					current_line.StartPoint = current_origin;
					current_line.EndPoint = current_origin;
					
					current_path.Data = current_line;
					paintSurface.Children.Add(current_path);

				}

				if (i > 0)
				{
					if (expression_strings[i - 1] == "P")
					{
						this.expression_text_box.Text += expression_strings[i - 1] + "(" + expression_values[i - 1].X + "," + expression_values[i - 1].Y + ")";
					}
					else
					{
						this.expression_text_box.Text += " + " + expression_strings[i - 1] + "(" + (expression_values[i - 1].X - expression_values[i - 2].X) + "," + (expression_values[i - 1].Y - expression_values[i - 2].Y) + ")";
					}

				}
				i++;

				hold_part = true;
			}
			if (e.RightButton == MouseButtonState.Pressed)
			{
				paintSurface.Children.Remove(current_path);
				if (hold_part)
				{
					if (expression_strings[i - 1] == "P")
					{
						this.expression_text_box.Text += expression_strings[i - 1] + "(" + expression_values[i - 1].X + "," + expression_values[i - 1].Y + ")";
					}
					else
					{
						this.expression_text_box.Text += " + " + expression_strings[i - 1] + "(" + (expression_values[i - 1].X - expression_values[i - 2].X) + "," + (expression_values[i - 1].Y - expression_values[i - 2].Y) + ")";
					}
					expression_strings.Clear();
					expression_values.Clear();
					i = 0;
				}
				hold_part = false;
				new_stroke = true;
			}
		}

		public void Canvas_MouseMove_1(object sender, MouseEventArgs e)
		{
			System.Windows.Point current_end = e.GetPosition((Canvas)sender);
			current_end = SnapToGrid(current_end);
			DrawPoint((Canvas)sender, current_end, true);
			if (hold_part)
			{
				if (arc_mode)
				{
					myGeometryGroup = UpdateArc(pathFigure, current_end, clockwise);
				}
				else
				{

					current_line = UpdateLine(current_line, current_end);
				}
			}
			else
			{
				current_origin = current_end;
			}
		}

		public void DrawPoint(Canvas myCanvas, System.Windows.Point p, bool update=false)
		{
			if (update)
			{
				myCanvas.Children.Remove(myEllipse);
			}
			myEllipse = new Ellipse();

			SolidColorBrush mySolidColorBrush = new SolidColorBrush();
			mySolidColorBrush.Color = Color.FromArgb(0, 0, 0, 0);
			myEllipse.Fill = mySolidColorBrush;
			myEllipse.StrokeThickness = 2;
			myEllipse.Stroke = Brushes.White;
			myEllipse.Width = 10;
			myEllipse.Height = 10;
			myEllipse.MouseDown += RectangleGeometry_MouseDown;
			myEllipse.MouseWheel += RectangleGeometry_MouseWheel;
			Canvas.SetLeft(myEllipse, p.X - myEllipse.Width/2);
			Canvas.SetTop(myEllipse, p.Y - myEllipse.Height/2);
			myCanvas.Children.Add(myEllipse);
		}

		public LineGeometry UpdateLine(LineGeometry line, System.Windows.Point end)
		{
			end = SnapToGrid(end);
			line.EndPoint = end;
			InvalidateVisual();
			return line;
		}
		public GeometryGroup UpdateArc(PathFigure figure, System.Windows.Point end, bool clockwise)
		{
			current_arc.Point = end;
			current_arc.IsLargeArc = false;
			double rad;
			if (Orientation(current_origin, new System.Windows.Point(current_origin.X,end.Y), end) == 1 && clockwise ||
				Orientation(current_origin, new System.Windows.Point(current_origin.X, end.Y), end) == 2 && !clockwise)
			{
				rad = Math.Sqrt(Math.Pow(end.X - current_origin.X, 2) + Math.Pow(end.Y - end.Y, 2));
			}
			else
			{
				rad = Math.Sqrt(Math.Pow(end.X - end.X, 2) + Math.Pow(end.Y - current_origin.Y, 2));
			}

			current_arc.Size = new Size(rad, rad);
			if (clockwise)
			{
				current_arc.SweepDirection = SweepDirection.Clockwise;
			}
			else
			{
				current_arc.SweepDirection = SweepDirection.Counterclockwise;
			}
			pathGeometry = new PathGeometry(new List<PathFigure> { pathFigure });
			myGeometryGroup.Children.Add(pathGeometry);

			InvalidateVisual();

			return myGeometryGroup;
		}
		public System.Windows.Point SnapToGrid(System.Windows.Point p)
		{
			p.X = p.X + (p.X % GridSize > GridSize / 2 ? GridSize - p.X % GridSize : - p.X % GridSize);
			p.Y = p.Y + (p.Y % GridSize > GridSize / 2 ? GridSize - p.Y % GridSize : - p.Y % GridSize);
			return p;
		}
		public int Orientation(System.Windows.Point p1, System.Windows.Point p2, System.Windows.Point p3)
		{
			double val = (p2.Y - p1.Y) * (p3.X - p2.X) -
					  (p2.X - p1.X) * (p3.Y - p2.Y);

			if (val == 0) return 0;  // colinear

			return (val > 0) ? 1 : 2; // clock or counterclock wise
		}

		private async void btn_generate_samples_Click(object sender, RoutedEventArgs e)
		{
			if (SetExpressions.Count > 0)
			{
				try
				{
					await SampleGenerator.GeneratePoints(DatasetNameTextBox.Text, SetExpressions);
					GestureDataset = GestureIOCustom.LoadTrainingSet(SampleGenerator.SamplesFolder + DatasetNameTextBox.Text + "//");
					ViewSamplesWindow _viewFrm = new ViewSamplesWindow(GestureDataset.ToList());
					_viewFrm.Owner = this;
					_viewFrm.Show();
				}
				catch
				{
					MessageBox.Show(SampleGenerator.ConnectivityErrorMessage, "Sample Generation Error");
				}
				
			}
			else
			{
				MessageBox.Show(SampleGenerator.MinExpressionsErrorMessage, "Sample Generation Error");
			}
		}

		private void btn_add_gesture_Click(object sender, RoutedEventArgs e)
		{
			SetExpressions.Add(new Expression(title_text_box.Text,expression_text_box.Text));
			ExpressionListView.Items.Add(SetExpressions.Last());
			ClearInterface();
		}

		private void ExpressionListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Expression r = (Expression)ExpressionListView.SelectedItem;
			title_text_box.Text = r.title;
			expression_text_box.Text = r.expression;
			DrawExpression(paintSurface, expression_text_box.Text);
		}

		private void load_menu_option_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			if (openFileDialog.ShowDialog() == true)
			{
				ResetInterface();
				SetExpressions = ParseJsonExpressions(openFileDialog.FileName);
				foreach (Expression r in SetExpressions)
				{
					ExpressionListView.Items.Add(r);
				}
				string[] filename_split = openFileDialog.FileName.Split('\\');
				DatasetNameTextBox.Text = filename_split[filename_split.Length-1].Split('.')[0];
			}
				
		}

		private void save_menu_option_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.FileName = DatasetNameTextBox.Text; // Default file name
			dlg.DefaultExt = ".json"; // Default file extension

			// Show save file dialog box
			Nullable<bool> result = dlg.ShowDialog();

			// Process save file dialog box results
			if (result == true)
			{
				// Save document
				string filename = dlg.FileName;
				using (StreamWriter file =
				new StreamWriter(filename))
				{

					file.WriteLine("{");
					for (int i = 0; i < SetExpressions.Count; i++)
					{
						Expression exp = SetExpressions[i];
						file.Write("\"" + exp.title + "\": \"" + exp.expression + "\"");
						if (i == SetExpressions.Count - 1)
						{
							file.Write("\n");
						}
						else
						{
							file.Write(",\n");
						}
					}
					file.WriteLine("}");
					file.Close();
				}

			}

		}
		public void ResetInterface()
		{
			SetExpressions.Clear();
			ExpressionListView.Items.Clear();
			NewDatasetName(DatasetNameTextBox);
			GestureDataset = null;
			ClearInterface();

		}

		public void ClearInterface()
		{
			ClearCanvas(paintSurface);
			NewGestureName(title_text_box);
			expression_text_box.Text = "";
			first = true;
		}

		static List<Expression> ParseJsonExpressions(string datasetFilename)
		{
			List<Expression> extractedExpressions = new List<Expression>();
			using (StreamReader r = new StreamReader(datasetFilename))
			{
				string json = r.ReadToEnd();
				JObject items = JsonConvert.DeserializeObject<JObject>(json);
				foreach (var exp in items)
				{
					extractedExpressions.Add(new Expression(exp.Key, (string) exp.Value));
				}
			}
			return extractedExpressions;
		}

		void DrawExpression(Canvas c, string code)
		{
			ClearCanvas(c);
			string[] strokes = code.Split(new[] { "|=" }, StringSplitOptions.None);
			foreach (string s in strokes)
			{
				System.Windows.Point current_position = GridCenterRealSize;
				string[] actions = s.Split('+');
				foreach (string a in actions)
				{
					char operation_command = a.Trim().Split('(')[0][0];
					int operation_x = Int32.Parse(a.Trim().Split('(')[1].Split(',')[0]);
					int operation_y = Int32.Parse(a.Trim().Split('(')[1].Split(',')[1].TrimEnd(')'));
					System.Windows.Point operation_moves = new System.Windows.Point(operation_x * GridSize, operation_y * GridSize);
					System.Windows.Point new_position = new System.Windows.Point(current_position.X + operation_moves.X, current_position.Y - operation_moves.Y);

					System.Windows.Shapes.Path path = new System.Windows.Shapes.Path();
					SolidColorBrush blackBrush = new SolidColorBrush();
					blackBrush.Color = Colors.Black;
					path.Stroke = blackBrush;
					path.StrokeThickness = 3;

					switch (operation_command)
					{
						case 'P':
							current_position.X = GridCenterRealSize.X + operation_moves.X;
							current_position.Y = GridCenterRealSize.Y - operation_moves.Y;
							break;
						case 'L':
							LineGeometry line = new LineGeometry();
							line.StartPoint = current_position;
							line.EndPoint = new_position;
							path.Data = line;
							c.Children.Add(path);
							current_position = new_position;
							break;
						case 'A':
							bool clockwise = a.Trim().Split('(')[0].Split('_')[1] == "CW";
							GeometryGroup myGeometryGroup = new GeometryGroup();
							PathFigure pathFigure = new PathFigure();
							ArcSegment current_arc = new ArcSegment();
							pathFigure.StartPoint = current_position;
							current_arc.Point = new_position;
							current_arc.IsLargeArc = false;
							double rad;
							if (Orientation(current_position, current_position, new_position) == 1 && clockwise ||
								Orientation(current_position, current_position, new_position) == 2 && !clockwise)
							{
								rad = Math.Sqrt(Math.Pow(new_position.X - current_position.X, 2) + Math.Pow(new_position.Y - new_position.Y, 2));
							}
							else
							{
								rad = Math.Sqrt(Math.Pow(new_position.X - new_position.X, 2) + Math.Pow(new_position.Y - current_position.Y, 2));
							}

							current_arc.Size = new Size(rad, rad);
							if (clockwise)
							{
								current_arc.SweepDirection = SweepDirection.Clockwise;
							}
							else
							{
								current_arc.SweepDirection = SweepDirection.Counterclockwise;
							}

							PathSegmentCollection PathSegment = new PathSegmentCollection();
							PathSegment.Add(current_arc);
							pathFigure.Segments = PathSegment;

							PathGeometry pathGeometry = new PathGeometry(new List<PathFigure> { pathFigure });
							myGeometryGroup.Children.Add(pathGeometry);

							path.Data = myGeometryGroup;
							c.Children.Add(path);
							current_position = new_position;
							break;
					}
					DrawPoint(c, current_position);
				}
			}
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			ClearInterface();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			RecognizerWindow recWin = new RecognizerWindow(GestureDataset);
			this.Hide();
			try
			{
				recWin.ShowDialog();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			this.Show();
		}
		
		private void paintSurface_MouseWheel(object sender, MouseWheelEventArgs e)
		{

			if (e.Delta > 0)
				BrushType = (BrushType + 1) % 3;

			else if (e.Delta < 0)
				BrushType = (BrushType - 1 + 3) % 3;

			ChangeDrawingTool((Canvas) sender, BrushType, e.GetPosition((Canvas) sender));
		}

		private void ChangeDrawingTool(Canvas c, int type, System.Windows.Point current_end=default(System.Windows.Point))
		{
			BrushType = type;
			switch (BrushType)
			{
				case 0:
					arc_mode = false;
					borderL.BorderThickness = new System.Windows.Thickness(1);
					borderACW.BorderThickness = new System.Windows.Thickness(0);
					borderAACW.BorderThickness = new System.Windows.Thickness(0);
					break;
				case 1:
					arc_mode = true;
					clockwise = true;
					borderL.BorderThickness = new System.Windows.Thickness(0);
					borderACW.BorderThickness = new System.Windows.Thickness(1);
					borderAACW.BorderThickness = new System.Windows.Thickness(0);
					break;
				case 2:
					arc_mode = true;
					clockwise = false;
					borderL.BorderThickness = new System.Windows.Thickness(0);
					borderACW.BorderThickness = new System.Windows.Thickness(0);
					borderAACW.BorderThickness = new System.Windows.Thickness(1);
					break;
			}
			paintSurface.Children.Remove(current_path);
			current_path = new System.Windows.Shapes.Path();
			SolidColorBrush blackBrush = new SolidColorBrush();
			blackBrush.Color = Colors.Black;
			current_path.Stroke = blackBrush;
			current_path.StrokeThickness = 3;
			myGeometryGroup = new GeometryGroup();
			if (arc_mode)
			{
				pathFigure = new PathFigure();
				current_arc = new ArcSegment();
				pathFigure.StartPoint = current_origin;
				current_arc.Point = current_origin;
				if (clockwise)
				{
					if (expression_strings.Count > 0)
					{
						expression_strings[expression_strings.Count - 1] = "A_CW";
					}

					current_arc.SweepDirection = SweepDirection.Clockwise;
				}
				else
				{
					if (expression_strings.Count > 0)
					{
						expression_strings[expression_strings.Count - 1] = "A_ACW";
					}
					current_arc.SweepDirection = SweepDirection.Counterclockwise;
				}
				PathSegmentCollection PathSegment = new PathSegmentCollection();
				PathSegment.Add(current_arc);
				pathFigure.Segments = PathSegment;
				pathGeometry = new PathGeometry(new List<PathFigure> { pathFigure });
				myGeometryGroup.Children.Add(pathGeometry);
				current_path.Data = myGeometryGroup;
				paintSurface.Children.Add(current_path);
			}
			else
			{
				if (expression_strings.Count > 0)
				{
					expression_strings[expression_strings.Count - 1] = "L";
				}
				current_line = new LineGeometry();
				current_line.StartPoint = current_origin;
				current_line.EndPoint = current_origin;

				current_path.Data = current_line;
				paintSurface.Children.Add(current_path);

			}

			if(current_end == default(System.Windows.Point))
			{
				current_end.X = c.Width / 2;
				current_end.Y = c.Height / 2;
			}
			current_end = SnapToGrid(current_end);

			if (arc_mode)
			{
				myGeometryGroup = UpdateArc(pathFigure, current_end, clockwise);
			}
			else
			{

				current_line = UpdateLine(current_line, current_end);
			}
		}

		void NewDatasetName(TextBox tb)
		{
			string DefaultString = "New_Dataset";
			string DatasetString = DefaultString;
			if (Directory.Exists(SampleGenerator.SamplesFolder + DefaultString + "//"))
			{
				int i = 2;
				
				do
				{
					DatasetString = DefaultString + i;
					i++;
				}
				while (Directory.Exists(SampleGenerator.SamplesFolder + DatasetString + "//"));
			}
			tb.Text = DatasetString;

		}
		void NewGestureName(TextBox tb)
		{
			string DefaultString = "Gesture_A";
			string GestureString = DefaultString;
			List<string> expression_names = new List<string>();

			foreach (Expression r in SetExpressions)
			{
				expression_names.Add(r.title);
			}

			if (expression_names.Contains(DefaultString))
			{
				int i = 0;

				do
				{
					GestureString = "Gesture_" + (char)(i + 65);
					i++;
				}
				while (expression_names.Contains(GestureString) && i<26); 
			}
			tb.Text = GestureString;
		}

		private void custom_test_1_select_Click(object sender, RoutedEventArgs e)
		{
			Thread thread = new Thread(CustomTests.AccuracyTests.RunExperiment);
			thread.Start();
		}
		private void custom_test_2_select_Click(object sender, RoutedEventArgs e)
		{
			Thread thread = new Thread(CustomTests.AccuracyTests.RunExperiment2);
			thread.Start();
		}
		private void custom_test_3_select_Click(object sender, RoutedEventArgs e)
		{
			Thread thread = new Thread(CustomTests.AccuracyTests.RunExperiment3);
			thread.Start();
		}
		private void custom_test_4_select_Click(object sender, RoutedEventArgs e)
		{
			Thread thread = new Thread(CustomTests.AccuracyTests.RunExperiment4);
			thread.Start();
		}

		private void custom_test_5_select_Click(object sender, RoutedEventArgs e)
		{
			Thread thread = new Thread(CustomTests.AccuracyTests.RunExperiment5);
			thread.Start();
		}

		private void about_select_Click(object sender, RoutedEventArgs e)
		{
			AboutBoxGS box = new AboutBoxGS();
			box.ShowDialog();
			
		}

		private void imageACW_MouseDown(object sender, MouseButtonEventArgs e)
		{
			ChangeDrawingTool(paintSurface, 1);
		}

		private void imageL_MouseDown(object sender, MouseButtonEventArgs e)
		{
			ChangeDrawingTool(paintSurface, 0);
		}

		private void imageAACW_MouseDown(object sender, MouseButtonEventArgs e)
		{
			ChangeDrawingTool(paintSurface, 2);
		}

		private void RectangleGeometry_MouseDown(object sender, MouseButtonEventArgs e)
		{
			Canvas_MouseDown_1(paintSurface, e);
		}
		private void RectangleGeometry_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			paintSurface_MouseWheel(paintSurface, e);
		}
	}
}
