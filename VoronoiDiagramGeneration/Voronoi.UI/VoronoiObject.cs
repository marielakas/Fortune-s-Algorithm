namespace Voronoi.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class VoronoiObject
    {
        private const int LEdge = 0;
        private const int REdge = 1;

        private double borderMinX;
        private double borderMaxX;
        private double borderMinY;
        private double borderMaxY;
        private int siteIndex;
        private double minX;
        private double maxX;
        private double minY;
        private double maxY;
        private double deltaX;
        private double deltaY;

        private int verticesCount;
        private int edgesCount;
        private int sitesCount;
        private Site[] sites;
        private Site bottomSite;
        private int sitesCountSqrt;
        private double minDistanceBetweenSites;
        private int priorityQueueMembersCount;
        private int priorityQueueMinMember;
        private int priorityQueueHashSize;
        private HalfEdge[] priorityQueueHash;

        private int edgeListHashSize;
        private HalfEdge[] edgeListHash;
        private HalfEdge edgeListLeftEnd;
        private HalfEdge edgeListRightEnd;
        private List<GraphEdge> graphEdgesList;

        public VoronoiObject(double minDistanceBetweenSites)
        {
            this.siteIndex = 0;
            this.sites = null;

            this.graphEdgesList = null;
            this.minDistanceBetweenSites = minDistanceBetweenSites;
        }

        public List<GraphEdge> GenerateVoronoiDiagram(double[] valuesX, double[] valuesY, double minX, double maxX, double minY, double maxY)
        {
            this.Sort(valuesX, valuesY, valuesX.Length);

            if (minX > maxX)
            {
                this.Swap(minX, maxX);
            }

            if (minY > maxY)
            {
                this.Swap(minY, maxY);
            }

            this.borderMinX = minX;
            this.borderMinY = minY;
            this.borderMaxX = maxX;
            this.borderMaxY = maxY;

            this.siteIndex = 0;
            this.VoronoiBoundaries();
            return this.graphEdgesList;
        }

        private void Swap(double firstValue, double secondValue)
        {
            double temp = 0.0d;
            temp = firstValue;
            firstValue = secondValue;
            secondValue = temp;
        }

        private void Sort(double[] valuesX, double[] valuesY, int count)
        {
            this.sites = null;
            this.graphEdgesList = new List<GraphEdge>();

            this.sitesCount = count;
            this.verticesCount = 0;
            this.edgesCount = 0;

            double sitesN = (double)this.sitesCount + 4;

            this.sitesCountSqrt = (int)Math.Sqrt(sitesN);

            // Copy the inputs so we don't modify the original arrays
            double[] copiedX = new double[count];
            double[] copiedY = new double[count];

            for (int i = 0; i < count; i++)
            {
                copiedX[i] = valuesX[i];
                copiedY[i] = valuesY[i];
            }

            this.SortNode(copiedX, copiedY, count);
        }

        private void SortSites(Site[] sites)
        {
            List<Site> sitesList = new List<Site>(sites.Length);

            for (int i = 0; i < sites.Length; i++)
            {
                sitesList.Add(sites[i]);
            }

            sitesList.Sort(new SiteSorter());

            // Copy back into the array
            for (int i = 0; i < sites.Length; i++)
            {
                sites[i] = sitesList[i];
            }
        }

        private void SortNode(double[] valuesX, double[] valuesY, int pointsCount)
        {
            this.sitesCount = pointsCount;
            this.sites = new Site[this.sitesCount];
            this.minX = valuesX[0];
            this.minY = valuesY[0];
            this.maxX = valuesX[0];
            this.maxY = valuesY[0];

            for (int i = 0; i < this.sitesCount; i++)
            {
                this.sites[i] = new Site();
                this.sites[i].Coordinates.SetPointCoordinates(valuesX[i], valuesY[i]);
                this.sites[i].SiteIndex = i;

                if (valuesX[i] < this.minX)
                {
                    this.minX = valuesX[i];
                }
                else if (valuesX[i] > this.maxX)
                {
                    this.maxX = valuesX[i];
                }

                if (valuesY[i] < this.minY)
                {
                    this.minY = valuesY[i];
                }
                else if (valuesY[i] > this.maxY)
                {
                    this.maxY = valuesY[i];
                }
            }

            this.SortSites(this.sites);

            this.deltaX = this.maxX - this.minX;
            this.deltaY = this.maxY - this.minY;
        }

        private Site GoToNextSite()
        {
            if (this.siteIndex < this.sitesCount)
            {
                Site site = this.sites[this.siteIndex];
                this.siteIndex++;
                return site;
            }

            return null;
        }

        private Edge BisectEdge(Site firstSite, Site secondSite)
        {
            double dX = 0.0d;
            double dY = 0.0d;
            double adX = 0.0d;
            double adY = 0.0d;

            Edge newEdge = new Edge();

            newEdge.Region[0] = firstSite;
            newEdge.Region[1] = secondSite;

            newEdge.EdgePoints[0] = null;
            newEdge.EdgePoints[1] = null;

            dX = secondSite.Coordinates.X - firstSite.Coordinates.X;
            dY = secondSite.Coordinates.Y - firstSite.Coordinates.Y;

            adX = dX > 0 ? dX : -dX;
            adY = dY > 0 ? dY : -dY;

            newEdge.C = (double)((firstSite.Coordinates.X * dX) + (firstSite.Coordinates.Y * dY) + (((dX * dX) + (dY * dY)) * 0.5));

            if (adX > adY)
            {
                newEdge.A = 1.0;
                newEdge.B = dY / dX;
                newEdge.C /= dX;
            }
            else
            {
                newEdge.A = dX / dY;
                newEdge.B = 1.0;
                newEdge.C /= dY;
            }

            newEdge.EdgeN = this.edgesCount;
            this.edgesCount++;

            return newEdge;
        }

        private void CreateVertex(Site vertex)
        {
            vertex.SiteIndex = this.verticesCount;
            this.verticesCount++;
        }

        private bool InitPriorityQueue()
        {
            this.priorityQueueMembersCount = 0;
            this.priorityQueueMinMember = 0;
            this.priorityQueueHashSize = 4 * this.sitesCountSqrt;
            this.priorityQueueHash = new HalfEdge[this.priorityQueueHashSize];

            for (int i = 0; i < this.priorityQueueHashSize; i++)
            {
                this.priorityQueueHash[i] = new HalfEdge();
            }

            return true;
        }

        private int GetPriorityQueueBucket(HalfEdge halfEdge)
        {
            int bucket = (int)((halfEdge.YStart - this.minY) / this.deltaY * this.priorityQueueHashSize);

            if (bucket < 0)
            {
                bucket = 0;
            }

            if (bucket >= this.priorityQueueHashSize)
            {
                bucket = this.priorityQueueHashSize - 1;
            }

            if (bucket < this.priorityQueueMinMember)
            {
                this.priorityQueueMinMember = bucket;
            }

            return bucket;
        }

        // push the HalfEdge into the ordered linked list of vertices
        private void InsertHalfEdgeInPriorityQueue(HalfEdge halfEdge, Site site, double offset)
        {
            HalfEdge nextHalfEdge;

            halfEdge.Vertex = site;
            halfEdge.YStart = (double)(site.Coordinates.Y + offset);
            HalfEdge lastHalfEdge = this.priorityQueueHash[this.GetPriorityQueueBucket(halfEdge)];

            while ((nextHalfEdge = lastHalfEdge.NextPriorityQueueMember) != null &&
                  (halfEdge.YStart > nextHalfEdge.YStart || 
                  (halfEdge.YStart == nextHalfEdge.YStart && 
                   site.Coordinates.X > nextHalfEdge.Vertex.Coordinates.X)))
            {
                lastHalfEdge = nextHalfEdge;
            }

            halfEdge.NextPriorityQueueMember = lastHalfEdge.NextPriorityQueueMember;
            lastHalfEdge.NextPriorityQueueMember = halfEdge;
            this.priorityQueueMembersCount++;
        }

        // remove the HalfEdge from the list of vertices
        private void DeleteHalfEdgeFromPriorityQueue(HalfEdge halfEdge)
        {
            if (halfEdge.Vertex != null)
            {
                HalfEdge lastHalfEdge = this.priorityQueueHash[this.GetPriorityQueueBucket(halfEdge)];

                while (lastHalfEdge.NextPriorityQueueMember != halfEdge)
                {
                    lastHalfEdge = lastHalfEdge.NextPriorityQueueMember;
                }

                lastHalfEdge.NextPriorityQueueMember = halfEdge.NextPriorityQueueMember;
                this.priorityQueueMembersCount--;
                halfEdge.Vertex = null;
            }
        }

        private bool IsPriorityQueueEmpty()
        {
            bool isEmpty = this.priorityQueueMembersCount == 0;
            return isEmpty;
        }

        private Point ExtractMinPointFromPriorityQueue()
        {
            Point point = new Point();

            while (this.priorityQueueHash[this.priorityQueueMinMember].NextPriorityQueueMember == null)
            {
                this.priorityQueueMinMember++;
            }

            point.X = this.priorityQueueHash[this.priorityQueueMinMember].NextPriorityQueueMember.Vertex.Coordinates.X;
            point.Y = this.priorityQueueHash[this.priorityQueueMinMember].NextPriorityQueueMember.YStart;

            return point;
        }

        private HalfEdge ExtractMinHalfEdgeFromPriorityQueue()
        {
            HalfEdge currentMin = this.priorityQueueHash[this.priorityQueueMinMember].NextPriorityQueueMember;
            this.priorityQueueHash[this.priorityQueueMinMember].NextPriorityQueueMember = currentMin.NextPriorityQueueMember;
            this.priorityQueueMembersCount--;

            return currentMin;
        }

        private HalfEdge CreateHalfEdge(Edge edge, int pm)
        {
            HalfEdge halfEdge = new HalfEdge();
            halfEdge.Edge = edge;
            halfEdge.ELpm = pm;
            halfEdge.NextPriorityQueueMember = null;
            halfEdge.Vertex = null;

            return halfEdge;
        }

        private bool InitEdgeList()
        {
            this.edgeListHashSize = 2 * this.sitesCountSqrt;
            this.edgeListHash = new HalfEdge[this.edgeListHashSize];

            for (int i = 0; i < this.edgeListHashSize; i++)
            {
                this.edgeListHash[i] = null;
            }

            this.edgeListLeftEnd = this.CreateHalfEdge(null, 0);
            this.edgeListRightEnd = this.CreateHalfEdge(null, 0);
            this.edgeListLeftEnd.LeftHalfEdge = null;
            this.edgeListLeftEnd.RightHalfEdge = this.edgeListRightEnd;
            this.edgeListRightEnd.LeftHalfEdge = this.edgeListLeftEnd;
            this.edgeListRightEnd.RightHalfEdge = null;
            this.edgeListHash[0] = this.edgeListLeftEnd;
            this.edgeListHash[this.edgeListHashSize - 1] = this.edgeListRightEnd;

            return true;
        }

        private HalfEdge RightHalfEdge(HalfEdge halfEdge)
        {
            return halfEdge.RightHalfEdge;
        }

        private HalfEdge LeftHalfEdge(HalfEdge halfEdge)
        {
            return halfEdge.LeftHalfEdge;
        }

        private Site GetLeftRegion(HalfEdge halfEdge)
        {
            if (halfEdge.Edge == null)
            {
                return this.bottomSite;
            }

            Site leftRegion = halfEdge.ELpm == LEdge ? halfEdge.Edge.Region[LEdge] : halfEdge.Edge.Region[REdge];
            return leftRegion;
        }

        private void InsertHalfEdge(HalfEdge leftBoundary, HalfEdge newHalfEdge)
        {
            newHalfEdge.LeftHalfEdge = leftBoundary;
            newHalfEdge.RightHalfEdge = leftBoundary.RightHalfEdge;
            leftBoundary.RightHalfEdge.LeftHalfEdge = newHalfEdge;
            leftBoundary.RightHalfEdge = newHalfEdge;
        }

        private void DeleteHalfEdge(HalfEdge halfEdge)
        {
            halfEdge.LeftHalfEdge.RightHalfEdge = halfEdge.RightHalfEdge;
            halfEdge.RightHalfEdge.LeftHalfEdge = halfEdge.LeftHalfEdge;
            halfEdge.IsDeleted = true;
        }

        private HalfEdge GetHalfEdgeHash(int index)
        {
            if (index < 0 || index >= this.edgeListHashSize)
            {
                return null;
            }

            HalfEdge halfEdge = this.edgeListHash[index];

            if (halfEdge == null || !halfEdge.IsDeleted)
            {
                return halfEdge;
            }

            this.edgeListHash[index] = null;

            return null;
        }

        private HalfEdge FindLeftBoundary(Point point)
        {
            int bucket = (int)((point.X - this.minX) / this.deltaX * this.edgeListHashSize);

            if (bucket < 0)
            {
                bucket = 0;
            }

            if (bucket >= this.edgeListHashSize)
            {
                bucket = this.edgeListHashSize - 1;
            }

            HalfEdge halfEdge = this.GetHalfEdgeHash(bucket);

            // if the halfedge isn't found, search backwards and forwards in the hash map
            // for the first non-null entry
            if (halfEdge == null)
            {
                for (int i = 1; i < this.edgeListHashSize; i++)
                {
                    if ((halfEdge = this.GetHalfEdgeHash(bucket - i)) != null)
                    {
                        break;
                    }

                    if ((halfEdge = this.GetHalfEdgeHash(bucket + i)) != null)
                    {
                        break;
                    }
                }
            }

            // Search linear list of halfedges for the correct one 
            if (halfEdge == this.edgeListLeftEnd || (halfEdge != this.edgeListRightEnd && this.GetIsRightOf(halfEdge, point)))
            {
                do
                {
                    halfEdge = halfEdge.RightHalfEdge;
                }
                while (halfEdge != this.edgeListRightEnd && this.GetIsRightOf(halfEdge, point));

                halfEdge = halfEdge.LeftHalfEdge;
            }
            else
            {
                do
                {
                    halfEdge = halfEdge.LeftHalfEdge;
                }
                while (halfEdge != this.edgeListLeftEnd && !this.GetIsRightOf(halfEdge, point));
            }

            // Update hash table 
            if (bucket > 0 && bucket < this.edgeListHashSize - 1)
            {
                this.edgeListHash[bucket] = halfEdge;
            }

            return halfEdge;
        }

        private void PushGraphEdge(Site leftSite, Site rightSite, double x1, double y1, double x2, double y2)
        {
            GraphEdge newEdge = new GraphEdge();
            this.graphEdgesList.Add(newEdge);
            newEdge.X1 = x1;
            newEdge.Y1 = y1;
            newEdge.X2 = x2;
            newEdge.Y2 = y2;

            newEdge.FirstSiteIndex = leftSite.SiteIndex;
            newEdge.SecondSiteIndex = rightSite.SiteIndex;
        }

        private void ClipLine(Edge edge)
        {
            Site firstSite, secondSite;

            double x1 = edge.Region[0].Coordinates.X;
            double y1 = edge.Region[0].Coordinates.Y;
            double x2 = edge.Region[1].Coordinates.X;
            double y2 = edge.Region[1].Coordinates.Y;
            double dX = x2 - x1;
            double dY = y2 - y1;

            // if the distance between the two points this line was created from is
            // less than the square root of x*x + y*y then ignore it
            if (Math.Sqrt((dX * dX) + (dY * dY)) < this.minDistanceBetweenSites)
            {
                return;
            }

            double minPX = this.borderMinX;
            double minPY = this.borderMinY;
            double maxPX = this.borderMaxX;
            double maxPY = this.borderMaxY;

            if (edge.A == 1.0 && edge.B >= 0.0)
            {
                firstSite = edge.EdgePoints[1];
                secondSite = edge.EdgePoints[0];
            }
            else
            {
                firstSite = edge.EdgePoints[0];
                secondSite = edge.EdgePoints[1];
            }

            if (edge.A == 1.0)
            {
                y1 = minPY;

                if (firstSite != null && firstSite.Coordinates.Y > minPY)
                {
                    y1 = firstSite.Coordinates.Y;
                }

                if (y1 > maxPY)
                {
                    y1 = maxPY;
                }

                x1 = edge.C - (edge.B * y1);
                y2 = maxPY;

                if (secondSite != null && secondSite.Coordinates.Y < maxPY)
                {
                    y2 = secondSite.Coordinates.Y;
                }

                if (y2 < minPY)
                {
                    y2 = minPY;
                }

                x2 = edge.C - (edge.B * y2);

                if (((x1 > maxPX) & (x2 > maxPX)) | ((x1 < minPX) & (x2 < minPX)))
                {
                    return;
                }

                if (x1 > maxPX)
                {
                    x1 = maxPX;
                    y1 = (edge.C - x1) / edge.B;
                }

                if (x1 < minPX)
                {
                    x1 = minPX;
                    y1 = (edge.C - x1) / edge.B;
                }

                if (x2 > maxPX)
                {
                    x2 = maxPX;
                    y2 = (edge.C - x2) / edge.B;
                }

                if (x2 < minPX)
                {
                    x2 = minPX;
                    y2 = (edge.C - x2) / edge.B;
                }
            }
            else
            {
                x1 = minPX;

                if (firstSite != null && firstSite.Coordinates.X > minPX)
                {
                    x1 = firstSite.Coordinates.X;
                }

                if (x1 > maxPX)
                {
                    x1 = maxPX;
                }

                y1 = edge.C - (edge.A * x1);
                x2 = maxPX;

                if (secondSite != null && secondSite.Coordinates.X < maxPX)
                {
                    x2 = secondSite.Coordinates.X;
                }

                if (x2 < minPX)
                {
                    x2 = minPX;
                }

                y2 = edge.C - (edge.A * x2);

                if (((y1 > maxPY) & (y2 > maxPY)) | ((y1 < minPY) & (y2 < minPY)))
                {
                    return;
                }

                if (y1 > maxPY)
                {
                    y1 = maxPY;
                    x1 = (edge.C - y1) / edge.A;
                }

                if (y1 < minPY)
                {
                    y1 = minPY;
                    x1 = (edge.C - y1) / edge.A;
                }

                if (y2 > maxPY)
                {
                    y2 = maxPY;
                    x2 = (edge.C - y2) / edge.A;
                }

                if (y2 < minPY)
                {
                    y2 = minPY;
                    x2 = (edge.C - y2) / edge.A;
                }
            }

            this.PushGraphEdge(edge.Region[0], edge.Region[1], x1, y1, x2, y2);
        }

        private void SetEndPoint(Edge edge, int index, Site site)
        {
            edge.EdgePoints[index] = site;

            if (edge.EdgePoints[REdge - index] == null)
            {
                return;
            }

            this.ClipLine(edge);
        }

        // returns true if point is to right of halfedge 
        private bool GetIsRightOf(HalfEdge halfEdge, Point point)
        {
            bool isRightOfHalfEdge;
            bool isAbove, isFast;
            double dXP, dYP, dXS, t1, t2, t3, yl;

            Edge edge = halfEdge.Edge;
            Site topSite = edge.Region[1];

            if (point.X > topSite.Coordinates.X)
            {
                isRightOfHalfEdge = true;
            }
            else
            {
                isRightOfHalfEdge = false;
            }

            if (isRightOfHalfEdge && halfEdge.ELpm == LEdge)
            {
                return true;
            }

            if (!isRightOfHalfEdge && halfEdge.ELpm == REdge)
            {
                return false;
            }

            if (edge.A == 1.0)
            {
                dXP = point.X - topSite.Coordinates.X;
                dYP = point.Y - topSite.Coordinates.Y;
                isFast = false;

                if ((!isRightOfHalfEdge & (edge.B < 0.0)) | (isRightOfHalfEdge & (edge.B >= 0.0)))
                {
                    isAbove = dYP >= edge.B * dXP;
                    isFast = isAbove;
                }
                else
                {
                    isAbove = point.X + (point.Y * edge.B) > edge.C;
                    if (edge.B < 0.0)
                    {
                        isAbove = !isAbove;
                    }

                    if (!isAbove)
                    {
                        isFast = true;
                    }
                }

                if (!isFast)
                {
                    dXS = topSite.Coordinates.X - edge.Region[0].Coordinates.X;
                    isAbove = edge.B * ((dXP * dXP) - (dYP * dYP)) < dXS * dYP * (1.0 + (2.0 * dXP / dXS) + (edge.B * edge.B));

                    if (edge.B < 0)
                    {
                        isAbove = !isAbove;
                    }
                }
            }
            else 
            {
                yl = edge.C - (edge.A * point.X);
                t1 = point.Y - yl;
                t2 = point.X - topSite.Coordinates.X;
                t3 = yl - topSite.Coordinates.Y;
                isAbove = (t1 * t1) > (t2 * t2) + (t3 * t3);
            }

            bool answer = halfEdge.ELpm == LEdge ? isAbove : !isAbove;
            return answer;
        }

        private Site GetRightRegion(HalfEdge halfEdge)
        {
            // if this halfedge has no edge, return the bottom site
            if (halfEdge.Edge == (Edge)null)
            {
                return this.bottomSite;
            }

            // if ELpm = 0, return the site 0 that this edge bisects, otherwise return site 1
            Site answer = halfEdge.ELpm == LEdge ? halfEdge.Edge.Region[REdge] : halfEdge.Edge.Region[LEdge];
            return answer;
        }

        private double CalculateDistance(Site firstSite, Site secondSite)
        {
            double dX = firstSite.Coordinates.X - secondSite.Coordinates.X;
            double dY = firstSite.Coordinates.Y - secondSite.Coordinates.Y;
            return Math.Sqrt((dX * dX) + (dY * dY));
        }

        // create a new site where the HalfEdges intersect
        private Site Intersect(HalfEdge firstHalfEdge, HalfEdge secondHalfEdge)
        {
            Edge edge;
            HalfEdge halfEdge;
            bool isRightOfSite; // vertex

            Edge firstEdge = firstHalfEdge.Edge;
            Edge secondEdge = secondHalfEdge.Edge;

            if (firstEdge == null || secondEdge == null)
            {
                return null;
            }

            // if the two edges bisect the same parent, return null
            if (firstEdge.Region[1] == secondEdge.Region[1])
            {
                return null;
            }

            double distance = (firstEdge.A * secondEdge.B) - (firstEdge.B * secondEdge.A);

            if (-1.0e-10 < distance && distance < 1.0e-10)
            {
                return null;
            }

            double intersectionPointX = ((firstEdge.C * secondEdge.B) - (secondEdge.C * firstEdge.B)) / distance;
            double intersectionPointY = ((secondEdge.C * firstEdge.A) - (firstEdge.C * secondEdge.A)) / distance;

            if ((firstEdge.Region[1].Coordinates.Y < secondEdge.Region[1].Coordinates.Y)
                || (firstEdge.Region[1].Coordinates.Y == secondEdge.Region[1].Coordinates.Y && 
                firstEdge.Region[1].Coordinates.X < secondEdge.Region[1].Coordinates.X))
            {
                halfEdge = firstHalfEdge;
                edge = firstEdge;
            }
            else
            {
                halfEdge = secondHalfEdge;
                edge = secondEdge;
            }

            isRightOfSite = intersectionPointX >= edge.Region[1].Coordinates.X;

            if ((isRightOfSite && halfEdge.ELpm == LEdge) || (!isRightOfSite && halfEdge.ELpm == REdge))
            {
                return null;
            }

            // create a new site at the point of intersection - this is a new vector; event waiting to happen
            Site intersectionSite = new Site();
            intersectionSite.Coordinates.X = intersectionPointX;
            intersectionSite.Coordinates.Y = intersectionPointY;

            return intersectionSite;
        }

        private bool VoronoiBoundaries()
        {
            Site bottom, top, temp, point;
            Site vertex;
            Point newIntersectionStart = null;
            int index;
            HalfEdge leftBoundary, rightBoundary, aboveHalfEdgeLeftBoundary, lowestHalfEdgeRightBoundary, bisector;
            Edge bisectionEdge;

            this.InitPriorityQueue();
            this.InitEdgeList();

            this.bottomSite = this.GoToNextSite();
            Site newSite = this.GoToNextSite();

            while (true)
            {
                if (!this.IsPriorityQueueEmpty())
                {
                    newIntersectionStart = this.ExtractMinPointFromPriorityQueue();
                }

                /// if the lowest site has a smaller y value than the lowest vector intersection, process the site;
                ///otherwise process the vector intersection
                if (newSite != null && (this.IsPriorityQueueEmpty()
                                        || newSite.Coordinates.Y < newIntersectionStart.Y
                                        || (newSite.Coordinates.Y == newIntersectionStart.Y
                                            && newSite.Coordinates.X < newIntersectionStart.X)))
                {
                    /// new site is smallest -this is a site event;
                    /// get the first half edge to the left of the new site
                    leftBoundary = this.FindLeftBoundary(newSite.Coordinates);

                    // get the first HalfEdge to the RIGHT of the new site
                    rightBoundary = this.RightHalfEdge(leftBoundary);

                    // if this halfedge has no edge,bot =bottom site (whatever that
                    // is)
                    bottom = this.GetRightRegion(leftBoundary);

                    // create a new edge that bisects
                    bisectionEdge = this.BisectEdge(bottom, newSite);

                    // create a new HalfEdge, setting its ELpm field to 0
                    bisector = this.CreateHalfEdge(bisectionEdge, LEdge);

                    // insert this new bisector edge between the left and right
                    // vectors in a linked list
                    this.InsertHalfEdge(leftBoundary, bisector);

                    // if the new bisector intersects with the left edge,
                    // remove the left edge's vertex, and put in the new one
                    if ((point = this.Intersect(leftBoundary, bisector)) != null)
                    {
                        this.DeleteHalfEdgeFromPriorityQueue(leftBoundary);
                        double distance = this.CalculateDistance(point, newSite);
                        this.InsertHalfEdgeInPriorityQueue(leftBoundary, point, distance);
                    }

                    leftBoundary = bisector;

                    // create a new HalfEdge, setting its ELpm field to 1
                    bisector = this.CreateHalfEdge(bisectionEdge, REdge);

                    // insert the new HE to the right of the original bisector
                    // earlier in the IF stmt
                    this.InsertHalfEdge(leftBoundary, bisector);

                    // if this new bisector intersects with the new HalfEdge
                    if ((point = this.Intersect(bisector, rightBoundary)) != null)
                    {
                        // push the HE into the ordered linked list of vertices
                        double distance = this.CalculateDistance(point, newSite);
                        this.InsertHalfEdgeInPriorityQueue(bisector, point, distance);
                    }

                    newSite = this.GoToNextSite();
                }
                else if (!this.IsPriorityQueueEmpty())
                {
                    /// intersection is smallest - this is a vector event; pop the HalfEdge with the lowest vector 
                    /// off the ordered list of vectors
                    leftBoundary = this.ExtractMinHalfEdgeFromPriorityQueue();

                    // get the HalfEdge to the left of the above HE
                    aboveHalfEdgeLeftBoundary = this.LeftHalfEdge(leftBoundary);

                    // get the HalfEdge to the right of the above HE
                    rightBoundary = this.RightHalfEdge(leftBoundary);

                    // get the HalfEdge to the right of the HE to the right of the
                    // lowest HE
                    lowestHalfEdgeRightBoundary = this.RightHalfEdge(rightBoundary);

                    // get the Site to the left of the left HE which it bisects
                    bottom = this.GetLeftRegion(leftBoundary);

                    // get the Site to the right of the right HE which it bisects
                    top = this.GetRightRegion(rightBoundary);

                    // get the vertex that caused this event
                    vertex = leftBoundary.Vertex;

                    // set the vertex number
                    this.CreateVertex(vertex);

                    // set the endpoint of the left HalfEdge to be this vector
                    this.SetEndPoint(leftBoundary.Edge, leftBoundary.ELpm, vertex);

                    // set the endpoint of the right HalfEdge to be this vector
                    this.SetEndPoint(rightBoundary.Edge, rightBoundary.ELpm, vertex);

                    // mark the lowest HE for deletion
                    this.DeleteHalfEdge(leftBoundary);

                    // remove all vertex events to do with the right HE
                    this.DeleteHalfEdgeFromPriorityQueue(rightBoundary);

                    // mark the right HE for deletion 
                    this.DeleteHalfEdge(rightBoundary); 
                    index = LEdge; 

                   /// if the site to the left of the event is higher than the site  
                   /// to the right of it, then swap them and set the index variable to 1
                    if (bottom.Coordinates.Y > top.Coordinates.Y)
                    { 
                        temp = bottom;
                        bottom = top;
                        top = temp;
                        index = REdge;
                    }

                    /// create an Edge (or line) that is between the two Sites. This creates the formula of
                    /// the line, and assigns a line number to it
                    bisectionEdge = this.BisectEdge(bottom, top);

                    // create a HE from the edge and make it point to that edge with its ELedge field
                    bisector = this.CreateHalfEdge(bisectionEdge, index); 

                    // insert the new bisector to the right of the left HE
                    this.InsertHalfEdge(aboveHalfEdgeLeftBoundary, bisector);

                    /// Set one endpoint to the new edge to be the vector point 'v'; If the site to the left of this 
                    /// bisector is higher than the right site, then this endpoint is put in position 0; 
                    /// otherwise - in position 1
                    this.SetEndPoint(bisectionEdge, REdge - index, vertex); 

                    /// if left half edge and the new bisector intersect, then delete
                    /// the left half edge, and reinsert it
                    if ((point = this.Intersect(aboveHalfEdgeLeftBoundary, bisector)) != null)
                    {
                        double distance = this.CalculateDistance(point, bottom);
                        this.DeleteHalfEdgeFromPriorityQueue(aboveHalfEdgeLeftBoundary);
                        this.InsertHalfEdgeInPriorityQueue(aboveHalfEdgeLeftBoundary, point, distance);
                    }

                    // if right half edge and the new bisector intersect, then reinsert it
                    if ((point = this.Intersect(bisector, lowestHalfEdgeRightBoundary)) != null)
                    {
                        double distance = this.CalculateDistance(point, bottom);
                        this.InsertHalfEdgeInPriorityQueue(bisector, point, distance);
                    }
                }
                else
                {
                    break;
                }
            }

            for (leftBoundary = this.RightHalfEdge(this.edgeListLeftEnd); leftBoundary != this.edgeListRightEnd; leftBoundary = this.RightHalfEdge(leftBoundary))
            {
                bisectionEdge = leftBoundary.Edge;
                this.ClipLine(bisectionEdge);
            }

            return true;
        }
    } 
} 
