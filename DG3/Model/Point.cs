namespace DG3
{
    /// <summary>
    /// Implements a 2D Point with timing and strokeID information
    /// </summary>
    public class Point
    {
        public float X, Y;       // point coordinates
        public int StrokeID;     // the stroke index to which this point belongs
        public int intX, intY;   // integer coordinates for LUT indexing
		public long Time;

		public Point(float x, float y, int strokeId, long T=0)
        {
            this.X = x;
            this.Y = y;
            this.StrokeID = strokeId;
            this.intX = 0;
            this.intY = 0;
			this.Time = T;
        }
	}
}
