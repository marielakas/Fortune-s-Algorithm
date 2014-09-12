namespace Voronoi.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Edge
    {
        private double a = 0;
        private double b = 0;
        private double c = 0;

        public Edge()
        {
            this.EdgePoints = new Site[2];
            this.Region = new Site[2];
        }

        public Site[] EdgePoints { get; set; }

        public Site[] Region { get; set; }

        public int EdgeN { get; set; }

        public double A
        {
            get
            {
                return this.a;
            }

            set
            {
                this.a = value;
            }
        }

        public double B
        {
            get
            {
                return this.b;
            }

            set
            {
                this.b = value;
            }
        }

        public double C
        {
            get
            {
                return this.c;
            }

            set
            {
                this.c = value;
            }
        }
    }
}
