namespace Voronoi.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class SiteSorter : IComparer<Site>
    {
        public int Compare(Site firstSite, Site secondSite)
        {
            Point firstPoint = firstSite.Coordinates;
            Point secondPoint = secondSite.Coordinates;

            if (firstPoint.Y < secondPoint.Y)
            {
                return -1;
            }

            if (firstPoint.Y > secondPoint.Y)
            {
                return 1;
            }

            if (firstPoint.X < secondPoint.X)
            {
                return -1;
            }

            if (firstPoint.X > secondPoint.X)
            {
                return 1;
            }

            return 0;
        }
    }
}
