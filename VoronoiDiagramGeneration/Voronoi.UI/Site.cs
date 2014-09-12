namespace Voronoi.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    // For sites and vertices
    public class Site
    { 
        public Site()
        {
            this.Coordinates = new Point();
        }

        public Point Coordinates { get; set; }

        public int SiteIndex { get; set; }
    }
}
