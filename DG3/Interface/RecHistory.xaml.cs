using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DG3;

namespace GestureStudio
{
	/// <summary>
	/// Interaction logic for RecHistory.xaml
	/// </summary>
	public partial class RecHistory : Window
	{
		public RecHistory(List<Gesture> candidates, List<Gesture> recognized)
		{
			InitializeComponent();
			ShowHistory(HistoryGrid,candidates,recognized);
		}
		private void ShowHistory(StackPanel target, List<Gesture> candidates, List<Gesture> recognized)
		{
			for (int i = 0; i < candidates.Count; i++)
			{
				Gesture c = candidates[i];
				Gesture r = recognized[i];

				var gpb = new GroupBox();

				var dpl = new DockPanel();
				var timetxt = new System.Windows.Controls.Label();
				timetxt.Content = Math.Round(TimeSpan.FromMilliseconds(c.PointsRaw.Last().Time).TotalSeconds, 3);
				var img_candidate = new System.Windows.Controls.Image();
				img_candidate.Source = RenderImages.RenderToPNGImageSource(c);
				img_candidate.Width = 96;
				img_candidate.Height = 96;

				var rectxt = new System.Windows.Controls.Label();
				rectxt.Content = r.Name;
				var img_recognized = new System.Windows.Controls.Image();
				img_recognized.Source = RenderImages.RenderToPNGImageSource(r);
				img_recognized.Width = 96;
				img_recognized.Height = 96;

				gpb.Header = Math.Round(TimeSpan.FromMilliseconds(c.PointsRaw.Last().Time).TotalSeconds, 3) + "ms";

				var grid = new Grid();
				grid.ColumnDefinitions.Add(new ColumnDefinition());
				grid.ColumnDefinitions.Add(new ColumnDefinition());
				grid.Margin = new System.Windows.Thickness(10, 0, 0, 10);

				gpb.Content = grid;
				var stpInput = new StackPanel();
				var inputLabel = new System.Windows.Controls.Label();
				inputLabel.Content = "Gesture Input";
				stpInput.Children.Add(inputLabel);
				var inpBorder = new Border();
				inpBorder.BorderThickness = new System.Windows.Thickness(1);
				inpBorder.BorderBrush = System.Windows.Media.Brushes.Black;
				inpBorder.Child = img_candidate;
				inpBorder.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
				inpBorder.VerticalAlignment = System.Windows.VerticalAlignment.Center;
				inpBorder.Width = 100;
				inpBorder.Height = 100;
				stpInput.Children.Add(inpBorder);
				var stpPrediction = new StackPanel();
				stpPrediction.Children.Add(rectxt);
				var predBorder = new Border();
				predBorder.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
				predBorder.VerticalAlignment = System.Windows.VerticalAlignment.Center;
				predBorder.BorderThickness = new System.Windows.Thickness(1);
				predBorder.BorderBrush = System.Windows.Media.Brushes.Black;
				predBorder.Child = img_recognized;
				predBorder.Width = 100;
				predBorder.Height = 100;

				stpInput.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
				stpInput.VerticalAlignment = System.Windows.VerticalAlignment.Center;
				stpPrediction.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
				stpPrediction.VerticalAlignment = System.Windows.VerticalAlignment.Center;

				stpPrediction.Children.Add(predBorder);

				Grid.SetRow(stpInput, 0);
				Grid.SetColumn(stpInput, 0);
				Grid.SetRow(stpPrediction, 0);
				Grid.SetColumn(stpPrediction, 1);

				grid.Children.Add(stpInput);
				grid.Children.Add(stpPrediction);

				target.Children.Add(gpb);

			}
		}
	}
}
