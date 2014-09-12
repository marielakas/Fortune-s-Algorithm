namespace Voronoi.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using Microsoft.Expression.Drawing;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using Voronoi.UI.Resources;
    using CSPoint = System.Windows.Point;
    using System.Windows.Media.Imaging;
    using Windows.Storage;
    using System.IO;

    public partial class MainPage : PhoneApplicationPage
    {
        private static int sitesCount;
        private VoronoiObject voronoiObject;
        private Random seeder;
        private Canvas currentDiagram;
        private bool isTextBoxTextChanged;
        
        public MainPage()
        {
            this.InitializeComponent();
            this.seeder = new Random();
            this.isTextBoxTextChanged = false;
            this.voronoiObject = new VoronoiObject(0.1);
        }

        private void CreateVoronoiDiagramClick(object sender, RoutedEventArgs e)
        {
            if (this.isTextBoxTextChanged)
            {
                this.currentDiagram = null;
                bool result = Int32.TryParse(this.PointsCount.Text.ToString(), out sitesCount);
                if (result)
                {
                    if (sitesCount >= 2 && sitesCount <= 999)
                    {
                        this.DrawDiagram();
                    }
                    else
                    {
                        MessageBox.Show("You must enter a number between 2 and 999. " +
                                                             "No characters and negative numbers are allowed");
                    }
                }
                else
                {
                    MessageBox.Show("You must enter a number between 2 and 999. " +
                                                              "No characters and negative numbers are allowed");
                }
            }
        }

        private void DrawDiagram()
        {
            SolidColorBrush blackBrush = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            SolidColorBrush redBrush = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
            SolidColorBrush purpleBrush = new SolidColorBrush(Color.FromArgb(255, 174, 129, 255));

            this.currentDiagram = new Canvas();
            this.currentDiagram.Width = 430;
            this.currentDiagram.Height = 500;
            currentDiagram.Background = purpleBrush;

            double canvasWidth = double.Parse(MainCanvas.Width.ToString());
            double canvasHeight = double.Parse(MainCanvas.Height.ToString());

            List<CSPoint> sites = new List<CSPoint>();
            int seed = this.seeder.Next();
            Random randomPointGenerator = new Random(seed);

            for (int i = 0; i < sitesCount; i++)
            {
                sites.Add(new CSPoint((randomPointGenerator.NextDouble() * 430), (randomPointGenerator.NextDouble() * 500)));
            }

            for (int i = 0; i < sites.Count; i++)
            {
                CSPoint center = new CSPoint(sites[i].X, sites[i].Y);
                System.Windows.Shapes.Path path = new System.Windows.Shapes.Path
                    ();
                path.Fill = blackBrush;
                path.Stroke = blackBrush;
                path.StrokeThickness = 1;
                EllipseGeometry ellipseGeometry = new EllipseGeometry();
             
                ellipseGeometry.Center = center;
                ellipseGeometry.RadiusX = 2;
                ellipseGeometry.RadiusY = 2;
                path.Data = ellipseGeometry;
                currentDiagram.Children.Add(path);
            }

            List<GraphEdge> graphEdges = this.CreateVoronoiGraph(sites, canvasWidth, canvasHeight);

            for (int i = 0; i < graphEdges.Count; i++)
            {
                CSPoint p1 = new CSPoint(graphEdges[i].X1, graphEdges[i].Y1);
                CSPoint p2 = new CSPoint(graphEdges[i].X2, graphEdges[i].Y2);
                Line line = new Line();
                line.Stroke = redBrush;
                line.StrokeThickness = 2;
                line.X1 = p1.X;
                line.Y1 = p1.Y;
                line.X2 = p2.X;
                line.Y2 = p2.Y;

                currentDiagram.Children.Add(line);
            }
            this.MainCanvas.Children.Add(currentDiagram);
        }

        private List<GraphEdge> CreateVoronoiGraph(List<CSPoint> sites, double width, double height)
        {
            double[] valuesX = new double[sites.Count];
            double[] valuesY = new double[sites.Count];

            for (int i = 0; i < sites.Count; i++)
            {
                valuesX[i] = sites[i].X;
                valuesY[i] = sites[i].Y;
            }

            List<GraphEdge> graphEdges = this.voronoiObject.GenerateVoronoiDiagram(valuesX, valuesY, 0.0d, width, 0.0d, height);
            return graphEdges;
        }

        private void PointsCountTextChanged(object sender, TextChangedEventArgs e)
        {
            this.isTextBoxTextChanged = true;
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            WriteableBitmap wb = new WriteableBitmap(currentDiagram, null);
            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            int a = 4;
            //FileStream stream = new FileStream(path, FileMode.CreateNew);
            //wb.WritePNG(stream);
            ///TODO
        }
    }
}