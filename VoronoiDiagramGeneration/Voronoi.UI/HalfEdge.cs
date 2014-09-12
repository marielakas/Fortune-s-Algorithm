namespace Voronoi.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class HalfEdge
    {
        public HalfEdge()
        {
            this.NextPriorityQueueMember = null;
        }

        public HalfEdge LeftHalfEdge { get; set; }

        public HalfEdge RightHalfEdge { get; set; }

        public Edge Edge { get; set; }

        public bool IsDeleted { get; set; }

        public int ELpm { get; set; }

        public Site Vertex { get; set; }

        public double YStart { get; set; }

        public HalfEdge NextPriorityQueueMember { get; set; }
    }
}
