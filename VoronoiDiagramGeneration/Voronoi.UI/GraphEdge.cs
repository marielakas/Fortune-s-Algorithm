namespace Voronoi.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class GraphEdge
    {
        public double X1 { get; set; }

        public double X2 { get; set; }

        public double Y1 { get; set; }

        public double Y2 { get; set; }

        public int FirstSiteIndex { get; set; }

        public int SecondSiteIndex { get; set; }
    }
}
