namespace Voronoi.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Point
    {
        public Point()
        {
        }

        public double X { get; set; }

        public double Y { get; set; }

        public void SetPointCoordinates(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }
    }   
}
