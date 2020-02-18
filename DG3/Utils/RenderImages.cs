using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows;
using System.Drawing;
using CustomTests;
using RecPoint = DG3.Point;
namespace DG3
{
	class RenderImages
	{
		private const double defaultDpi = 96.0;
		private const int canvasSize = 96;
		private const int canvasWidth = canvasSize;
		private const int canvasHeight = canvasSize;

		public static void GenerateComparisonImage(Category target, Gesture skeleton)
		{
			List<BitmapSource> imgSrcs = new List<BitmapSource>();
			BitmapImage src = new BitmapImage();
			int i = 0;
			foreach (var g in target.prototypes)
			{
				if (i >= 5)
				{
					break;
				}
				var indexes = Recognizer.SeparateGestureMulti(g.PointsRaw);
				Dictionary<int, List<int>> parts = new Dictionary<int, List<int>>();
				foreach (var ind in indexes)
				{
					int index = (int)ind[0];
					int strokeID = g.PointsRaw[index].StrokeID;
					if (!parts.ContainsKey(strokeID))
					{
						parts.Add(strokeID, new List<int>());
					}
					parts[strokeID].Add(index);
				}
				imgSrcs.Add(RenderToPNGImageSource(g, true, true, parts, true));
				i++;
			}

			var wb = new WriteableBitmap(canvasWidth, canvasHeight, defaultDpi, defaultDpi, PixelFormats.Pbgra32, null);
			var group = new DrawingGroup();
			DrawingVisual dv = new DrawingVisual();
			using (DrawingContext dc = dv.RenderOpen())
			{
				foreach (var img in imgSrcs)
				{
					dc.DrawImage(img, new Rect(0, 0, canvasWidth, canvasHeight));
				}	
			}
			var rtb = new RenderTargetBitmap(canvasWidth, canvasHeight, defaultDpi, defaultDpi, PixelFormats.Pbgra32);
			rtb.Render(dv);

			var b1 = RenderToPNGImageSource(skeleton, true, true);

			var width = b1.PixelWidth + rtb.PixelWidth;
			var height = b1.PixelHeight;
			var wb2 = new WriteableBitmap(width, height, defaultDpi, defaultDpi, PixelFormats.Pbgra32, null);

			var stride = (b1.PixelWidth * PixelFormats.Pbgra32.BitsPerPixel + 7) / 8;
			var size = b1.PixelHeight * stride;

				
			var buffer = new byte[size];
			b1.CopyPixels(buffer, stride, 0);
			wb2.WritePixels(
				new Int32Rect(0, 0, b1.PixelWidth, b1.PixelHeight),
				buffer, stride, 0);

			rtb.CopyPixels(buffer, stride, 0);
			wb2.WritePixels(
				new Int32Rect(b1.PixelWidth, 0, rtb.PixelWidth, rtb.PixelHeight),
				buffer, stride, 0);

			string filename = $"./comparison/comparison-{target.Name}.png";
				using (FileStream stream = new FileStream(filename, FileMode.Create))
				{
					PngBitmapEncoder encoder = new PngBitmapEncoder();
					encoder.Frames.Add(BitmapFrame.Create(wb2));
					encoder.Save(stream);
				}

		}

		public static BitmapSource RenderToPNGImageSource(Gesture target, bool highlightstrokes=false, bool highlightparts = false, Dictionary<int, List<int>> parts = null, bool drawPoints = false)
		{
			BitmapImage src = new BitmapImage();
			RenderTargetBitmap rtb;
			System.Drawing.Pen pen = null;
			RecPoint[] points = target.PointsRaw;
			
			if(parts == null)
			{ 
				parts = target.Partition_Indexes;
			}
			if (parts.Count == 0)
			{
				highlightparts = false;
			}
			if (highlightparts == true)
			{
				highlightstrokes = true;
			}

			if (target.Name == "exceeded max number of strokes")
			{
				rtb = new RenderTargetBitmap(canvasWidth, canvasHeight, defaultDpi, defaultDpi, PixelFormats.Pbgra32);
				return rtb;
			}

			// CUSTOM
			DrawingImage dtp = new DrawingImage();
			DrawingGroup dwg = new DrawingGroup();

			GeometryGroup myGeometryGroup = new GeometryGroup();

			Graphics graphics = null;
			double stroke_thickness = 1;
			Bitmap bitmp = new Bitmap( canvasWidth + (int)stroke_thickness, canvasHeight + (int)stroke_thickness);

			try
			{
				pen = new System.Drawing.Pen(System.Drawing.Color.OrangeRed, 3);

				
				graphics = Graphics.FromImage(bitmp);
				graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
				graphics.Clear(System.Drawing.Color.White);

				double left =  points[0].X;
				double right = points[0].X;
				double up = points[0].Y;
				double down = points[0].Y;

				foreach (var p in points)
				{
					if (p.X > right)
						right = p.X;
					if (p.X < left)
						left = p.X;
					if (p.Y > up)
						up = p.Y;
					if (p.Y < down)
						down = p.Y;
				}

				double c_width = right - left;
				double c_height = up - down;

				double p_width=0, p_height=0;

				if (c_width >= c_height)
				{
					p_width = canvasWidth / c_width;
				}
				if (c_height >= c_width)
				{
					p_height = canvasHeight / c_height;
				}

				;
				if (p_width != p_height)
				{
					if (p_height == 0)
					{
						
						p_height = p_width;
						down -= ((canvasHeight - c_height * p_height)/ 2)/ p_height;
					}
					else
					{
						p_width = p_height;
						left -= ((canvasWidth - c_width * p_width) / 2) / p_width; 
					}
				}
	
				DrawingVisual dv = new DrawingVisual();
				System.Windows.Media.SolidColorBrush[] BrushesArray = new System.Windows.Media.SolidColorBrush[]{
					System.Windows.Media.Brushes.Green,
					System.Windows.Media.Brushes.Red,
					System.Windows.Media.Brushes.Blue,
					System.Windows.Media.Brushes.Magenta,
					System.Windows.Media.Brushes.DarkOrange
				};

				System.Drawing.Color[] ColorsArray = new System.Drawing.Color[]{
					System.Drawing.Color.Green,
					System.Drawing.Color.Red,
					System.Drawing.Color.Blue,
					System.Drawing.Color.Magenta,
					System.Drawing.Color.DarkOrange
				};
				int g = 0;

				using (DrawingContext dc = dv.RenderOpen())
				{
					int i = 0;
					while (i < points.Length)
					{
						System.Windows.Media.Pen myPen = new System.Windows.Media.Pen(BrushesArray[g], stroke_thickness);
						var geometry = new StreamGeometry();
						using (StreamGeometryContext ctx = geometry.Open())
						{
							double startX = ((points[i].X - left) * p_width);
							double startY = ((points[i].Y - down) * p_height);
							ctx.BeginFigure(new System.Windows.Point(startX, startY),
								true,  // is filled 
								false // is closed 
								);
							if (drawPoints)
							{
								dc.DrawEllipse(BrushesArray[g], myPen, new System.Windows.Point(startX, startY),1,1);
							}
							i++;
							while (i < points.Length && points[i].StrokeID == points[i - 1].StrokeID && (!highlightparts || !parts[points[i].StrokeID].Contains(i-1)) )
							{
								

								double endX = ((points[i].X - left) * p_width);
								double endY = ((points[i].Y - down) * p_height);

								ctx.LineTo(new System.Windows.Point(endX, endY), 
								true // is stroked 
								, false // is smooth join 
								);

								if (drawPoints)
								{
									dc.DrawEllipse(BrushesArray[g], myPen, new System.Windows.Point(endX, endY), 1, 1);
								}

								i++;

							}
						}
						geometry.Freeze();

						dc.DrawGeometry(null, myPen, geometry);
						

						if (highlightstrokes || highlightparts)
						{
							g = (g + 1) % BrushesArray.Length;
						}
					}
				}
				rtb = new RenderTargetBitmap(canvasWidth,canvasHeight, defaultDpi, defaultDpi, PixelFormats.Pbgra32);
				rtb.Render(dv);
			}
			finally
			{
				if (pen != null)
					pen.Dispose();
				
				if (graphics != null)
					graphics.Dispose();
			}
			return rtb;
		}
	}
}
