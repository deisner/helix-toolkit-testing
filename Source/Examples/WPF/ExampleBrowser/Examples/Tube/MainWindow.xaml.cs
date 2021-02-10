// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   Interaction logic for MainWindow.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TubeDemo
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media.Media3D;
    using System.Windows.Media;
    using ExampleBrowser;
    using HelixToolkit.Wpf;
    using System.Collections.Generic;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [Example(null, "Shows Borromean rings using the TubeVisual3D.")]
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // http://en.wikipedia.org/wiki/Borromean_rings
            // http://paulbourke.net/geometry/borromean/


            // If mergeMeshes is true, create one mesh for all 50 rings, and use only one GeometryModel3D 
            // and one ModelVisual3D for the entire scene.
            // 
            // Otherwise, create 50 separate Visual3Ds and GeometryModel3Ds.

            const bool mergeMeshes = false;

            int n = 180;
            double r = Math.Sqrt(3) / 3;
            //Ring1 = this.CreatePath(0, Math.PI * 2, n, u => Math.Cos(u), u => Math.Sin(u) + r, u => Math.Cos(3 * u) / 3, 0, 0, 0);
            //Ring2 = this.CreatePath(0, Math.PI * 2, n, u => Math.Cos(u) + 0.5, u => Math.Sin(u) - r / 2, u => Math.Cos(3 * u) / 3, 0, 0, 0);
            //Ring3 = this.CreatePath(0, Math.PI * 2, n, u => Math.Cos(u) - 0.5, u => Math.Sin(u) - r / 2, u => Math.Cos(3 * u) / 3, 0, 0, 0);


            var meshes = new List<MeshGeometry3D>();
            var redMaterial = MaterialHelper.CreateMaterial(Colors.Red);
            double min = 3, max = 30;
            int m = 10, p = 5;
            for (int i = 0; i < m; i++)
            {
                double dx = min + (max - min) * i / (m - 1);
                for (int j = 0; j < p; j++)
                {
                    double dy = min + (max - min) * j / (p - 1);
                    Point3DCollection ring = CreatePath(0, Math.PI * 2, n, u => Math.Cos(u), u => Math.Sin(u) + r, u => Math.Cos(3 * u) / 3, dx, dy, 0);
                    TubeVisual3D tube = new TubeVisual3D() { Path = ring, Diameter = 0.8, ThetaDiv = 20, IsPathClosed = true, Material = redMaterial };
                    
                    if (mergeMeshes)
                        meshes.Add(tube.Model.Geometry as MeshGeometry3D);
                    else
                        hvp3D.Children.Add(tube);
                }
            }

            if (mergeMeshes)
            {
                MeshGeometry3D mergedMesh = MergeMeshes(meshes);
                var mergedModel = new GeometryModel3D
                {
                    Geometry = mergedMesh,
                    Material = redMaterial
                };

                ModelVisual3D model = new ModelVisual3D() { Content = mergedModel };
                hvp3D.Children.Add(model);
            }

            PrintRenderingTier();

            DataContext = this;
        }

        private void PrintRenderingTier()
        {
            int renderingTier = (RenderCapability.Tier >> 16);
            Console.WriteLine($"Rendering Tier: {renderingTier}");
        }

        private Point3DCollection CreatePath(double min, double max, int n, Func<double, double> fx, Func<double, double> fy, Func<double, double> fz, 
            double dx, double dy, double dz)
        {
            var list = new Point3DCollection(n);
            for (int i = 0; i < n; i++)
            {
                double u = min + (max - min) * i / n;
                list.Add(new Point3D(fx(u) + dx, fy(u) + dy, fz(u) + dz));
            }
            return list;
        }

        private static MeshGeometry3D MergeMeshes(IList<MeshGeometry3D> meshes)
        {
            if (meshes.Count == 0)
                throw new ArgumentException("At least one mesh is required");

            Console.WriteLine($"Merging {meshes.Count} meshes");

            int triangleIndexOffset = 0;
            var mergedPositions = new List<Point3D>();
            var mergedNormals = new List<Vector3D>();
            var mergedTriangleIndices = new Int32Collection();
            foreach (var m in meshes)
            {
                Console.WriteLine($"triangleIndexOffset = {triangleIndexOffset}");
                Console.WriteLine($"Adding {m.Positions.Count} Positions");
                mergedPositions = mergedPositions.Concat(m.Positions).ToList();
                mergedNormals = mergedNormals.Concat(m.Normals).ToList();
                var offsetTriangleIndices = m.TriangleIndices.Select(i => i + triangleIndexOffset);
                mergedTriangleIndices = new Int32Collection(mergedTriangleIndices.Concat(offsetTriangleIndices));
                triangleIndexOffset += m.Positions.Count;
            }

            MeshGeometry3D mergedMeshes = new MeshGeometry3D()
            {
                Positions = new Point3DCollection(mergedPositions),
                Normals = new Vector3DCollection(mergedNormals),
                TriangleIndices = new Int32Collection(mergedTriangleIndices)
            };

            return mergedMeshes;
        }


        //public Point3DCollection Ring1 { get; set; }
        //public Point3DCollection Ring2 { get; set; }
        //public Point3DCollection Ring3 { get; set; }
    }
}