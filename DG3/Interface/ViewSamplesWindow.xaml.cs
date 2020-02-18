using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using DG3;

namespace GestureStudio
{
	/// <summary>
	/// Interaction logic for ViewSamplesWindow.xaml
	/// </summary>
	public partial class ViewSamplesWindow : Window
	{
		public List<Gesture> samples = null;
		public ViewSamplesWindow(List<Gesture> prototypes)
		{
			InitializeComponent();
			samples = prototypes;
			int row_index = 0;
			int column_index = 0;
			foreach (Gesture g in prototypes)
			{
				if (column_index >= SamplesGrid.ColumnDefinitions.Count)
				{
					SamplesGrid.ColumnDefinitions.Add(new ColumnDefinition());
				}

				var dp = new DockPanel();
				var txt = new System.Windows.Controls.Label();
				txt.Content = g.Name;
				var img = new System.Windows.Controls.Image();
				img.Source = RenderImages.RenderToPNGImageSource(g, true, true);
				img.Width = 96;
				img.Height = 96;
				img.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
				img.VerticalAlignment = System.Windows.VerticalAlignment.Center;
				txt.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
				txt.VerticalAlignment = System.Windows.VerticalAlignment.Center;

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
				SamplesGrid.Children.Add(b);
				b.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler((sender, e) => changeSample(sender, PartsGrid));
				b.MouseEnter += new System.Windows.Input.MouseEventHandler(HighlightSelection);
				b.MouseLeave += new System.Windows.Input.MouseEventHandler(MouseLeaveSelection);
				column_index++;
			}

		}
		private void changeSample(Object sender, Grid targetGrid)
		{
			targetGrid.Children.Clear();
			InvalidateVisual();
			DockPanel dp = (DockPanel)((Border)sender).Child;
			string gestureName = (string)((System.Windows.Controls.Label)(dp.Children[0])).Content;
			Gesture g = (Gesture) samples.Where(x => x.Name.StartsWith(gestureName)).First();
			ShowParts(targetGrid, g);
		}
		private void ShowParts(Grid targetGrid, Gesture t)
		{
			int row_index = 0;
			int column_index = 0;

			foreach (var d in t.Part_Combinations)
			{
				foreach (var g in d.Value)
				{
					if (column_index >= targetGrid.ColumnDefinitions.Count)
					{
						targetGrid.ColumnDefinitions.Add(new ColumnDefinition());
					}

					var dp = new DockPanel();
					var txt = new System.Windows.Controls.Label();
					txt.Content = g.Name;
					var img = new System.Windows.Controls.Image();
					img.Source = RenderImages.RenderToPNGImageSource(g, true, true);
					img.Width = 96;
					img.Height = 96;
					img.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
					img.VerticalAlignment = System.Windows.VerticalAlignment.Center;
					txt.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
					txt.VerticalAlignment = System.Windows.VerticalAlignment.Center;

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
					targetGrid.Children.Add(b);
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

	}
}
