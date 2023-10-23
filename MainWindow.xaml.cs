using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Nts.Extensions;
using Mapsui.Projections;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Utilities;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using Color = Mapsui.Styles.Color;
using Point = NetTopologySuite.Geometries.Point;

namespace MapsuiTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            var mapControl = new Mapsui.UI.Wpf.MapControl();

            mapControl.Map?.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer("mapsui-test"));


            var polygonLayer = new Layer("Polygons")
            {
                Style = new VectorStyle
                {
                    Fill = new Brush(new Color(150, 150, 30, 128)),
                    Outline = new Pen
                    {
                        Color = Color.Orange,
                        Width = 2,
                        PenStyle = PenStyle.Solid,
                        PenStrokeCap = PenStrokeCap.Round

                    }
                }
            };

            var p1 = new MPoint(33.1563657, 39.7996025);
            var p2 = new MPoint(38.68248887661493, 40.58426879860461);
            var p3 = new MPoint(37.7541441214137, 36.59296331010281);
            var p4 = new MPoint(28.877190722566567, 36.079662914349406);
            var p5 = new MPoint(26.471184788968152, 40.50913455516448);

            var p1Spherical = SphericalMercator.FromLonLat(p1.X, p1.Y).ToMPoint();
            var p2Spherical = SphericalMercator.FromLonLat(p2.X, p2.Y).ToMPoint();
            var p3Spherical = SphericalMercator.FromLonLat(p3.X, p3.Y).ToMPoint();
            var p4Spherical = SphericalMercator.FromLonLat(p4.X, p4.Y).ToMPoint();
            var p5Spherical = SphericalMercator.FromLonLat(p5.X, p5.Y).ToMPoint();

            var polygon1 = new Polygon(
                    new LinearRing(new[]
                    {
                        new Coordinate(p1Spherical.X, p1Spherical.Y),
                        new Coordinate(p2Spherical.X, p2Spherical.Y),
                        new Coordinate(p3Spherical.X, p3Spherical.Y),
                        new Coordinate(p4Spherical.X, p4Spherical.Y),
                        new Coordinate(p5Spherical.X, p5Spherical.Y),
                        new Coordinate(p1Spherical.X, p1Spherical.Y)
                    }));
            polygon1.UserData = "a fluffy polygon";
            

            var polygonList = new List<Polygon> { polygon1 };

            // available for single polygon, check source code of Mapsui.Nts.Extensions.ToFeatures()
            polygonLayer.DataSource = new MemoryProvider(polygonList.ToFeatures());

            mapControl.Map.Layers.Add(polygonLayer);

            var pinLayer = new GenericCollectionLayer<List<IFeature>>()
            {
                Style = CreatePinStyle()
            };
            mapControl.Map.Layers.Add(pinLayer);

            // putting a pin on the map:
            //mapControl.Map.Info += (sender, args) =>
            //{
            //    if (args.MapInfo?.WorldPosition == null) return;

            //    pinLayer.Features.Add(new GeometryFeature
            //    {
            //        Geometry = new Point(args.MapInfo.WorldPosition.X, args.MapInfo.WorldPosition.Y)
            //    });
            //};

            // testing if a polygon is clicked:
            mapControl.Map.Info += (sender, args) =>
            {
                if (args.MapInfo?.WorldPosition == null) return;

                var clickedPoint = new Point(args.MapInfo.WorldPosition.X, args.MapInfo.WorldPosition.Y);

                foreach (var polygon in polygonList)
                {
                    if (polygon.Contains(clickedPoint))
                    {
                        MessageBox.Show($"The clicked polygon is {polygon.UserData}");
                        break;
                    }
                }
            };


            mapControl.Map.Home = n => n.NavigateTo(polygonLayer.Extent);
            // there is also map.Home = n => n.CenterOnAndZoomTo(polygonLayer.Extent!.Centroid, n.Resolutions[15]);

            Content = mapControl;
        }

        public static SymbolStyle CreatePinStyle(Color? pinColor = null, double symbolScale = 1.0)
        {
            // This method is in Mapsui.Rendering.Skia because it has a dependency on Skia
            // because the resource is converted to an image using Skia. I think
            // It should be possible to create a style with just a reference to the platform
            // independent resource. The conversion to an image should happen in a render phase that
            // precedes a paint phase. https://github.com/Mapsui/Mapsui/issues/1448
            var pinId = typeof(Map).LoadSvgId("Resources.Images.Pin.svg");
            return new SymbolStyle
            {
                BitmapId = pinId,
                SymbolOffset = new RelativeOffset(0.0, 0.5),
                SymbolScale = symbolScale,
                BlendModeColor = pinColor ?? Color.FromArgb(255, 57, 115, 199) // Determines color of the pin
            };
        }
    }
}
