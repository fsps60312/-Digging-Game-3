using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Diagnostics;
using System.Windows;

namespace Digging_Game_3
{
    public static class My3DGraphics
    {
        public static class Sphere
        {
            static Random rand = new Random();
            public static MeshGeometry3D Create(double radius, int zoneCount = 20)
            {
                try
                {
                    var positions = new List<Point3D>();
                    var triangleIndices = new List<int>();
                    var textureCoordinates = new List<System.Windows.Point>();
                    List<List<double>> ps = new List<List<double>>();
                    ps.Add(new List<double> { 0 });
                    positions.Add(new Point3D(0, 0, radius));
                    textureCoordinates.Add(new Point(0.5, 0));
                    for (int i = 1; i < zoneCount; i++)
                    {
                        var latitude = Math.PI * i / zoneCount;//0~Pi
                        int cnt = (int)Math.Ceiling(Math.Sin(latitude) * 2 * zoneCount);
                        //double offset = rand.NextDouble() / cnt;
                        List<double> pps = new List<double>();
                        double r =radius* Math.Sin(latitude), z =radius* Math.Cos(latitude);
                        //pps.Add(0);
                        for (int j = 0; j <= cnt; j++)
                        {
                            double a = (double)j / cnt;
                            textureCoordinates.Add(new Point(a, (double)i / zoneCount));
                            //Trace.Assert(0 <= a && a <= 1);
                            pps.Add(a);
                            a *= 2 * Math.PI;
                            positions.Add(new Point3D(r * Math.Cos(a), r * Math.Sin(a), z));
                        }
                        //pps.Add(1);
                        ps.Add(pps);
                    }
                    ps.Add(new List<double> { 0 });
                    positions.Add(new Point3D(0, 0, -radius));
                    textureCoordinates.Add(new Point(0.5, 1));
                    //for (int i = 0; i < ps.Count; i++) ps[i].Add(ps[i][0] + 1);
                    for (int i = 1, os1 = 0, os2 = os1 + ps[0].Count; i <= zoneCount; os1 = os2, os2 += ps[i].Count, i++)
                    {
                        int n1 = ps[i - 1].Count - 1, n2 = ps[i].Count - 1;
                        int i1 = 0, i2 = 0;
                        int a = os1, b = os2;
                        //Trace.WriteLine($"i={i},n1={n1},n2={n2},ps.Count={ps.Count},a={a},b={b},os1={os1},os2={os2},positions.Count={positions.Count}");
                        Action incrementA = new Action(() =>
                        {
                            int c = os1 + (++i1);
                            if (a != c)
                            {
                                //Trace.Write($"A{c}");
                                //Trace.Assert(c <= positions.Count && b <= positions.Count && c <= positions.Count, $"i={i},n1={n1},n2={n2},ps.Count={ps.Count},a={a},b={b},c={c},positions.Count={positions.Count}");
                                var indices = new[] { a, b, c };
                                triangleIndices.AddRange(indices);
                                a = c;
                            }
                        });
                        Action incrementB = new Action(() =>
                        {
                            int c = os2 + (++i2);
                            if (b != c)
                            {
                                //Trace.Write($"B{c}");
                                //Trace.Assert(c <= positions.Count, $"n1={n1},n2={n2},ps.Count={ps.Count},c={c},positions.Count={positions.Count}");
                                var indices = new[] { a, b, c };
                                triangleIndices.AddRange(indices);
                                b = c;
                            }
                        });
                        while (i1 < n1 && i2 < n2)
                        {
                            if (ps[i][i2 + 1] - ps[i - 1][i1] <= ps[i - 1][i1 + 1] - ps[i][i2])
                                incrementB();//↖
                            else
                                incrementA();//↗
                        }
                        while (i1 < n1) incrementA();
                        while (i2 < n2) incrementB();
                        //Trace.WriteLine("");
                        //Trace.Assert(a == os1 && b == os2, $"i={i},n1={n1},n2={n2},ps.Count={ps.Count},a={a},b={b},os1={os1},os2={os2},positions.Count={positions.Count}");
                    }
                    //Trace.Assert(triangleIndices.Count == normals.Count && triangleIndices.Count == textureCoordinates.Count);
                    //MessageBox.Show(triangleIndices.Count.ToString());
                    return NewModel().AddTriangles(positions, triangleIndices, positions.Select(p => new Vector3D(p.X, p.Y, p.Z)), textureCoordinates);
                }
                catch (Exception error) { MessageBox.Show(error.ToString()); throw; }
            }
        }
        public static MeshGeometry3D NewModel() { return new MeshGeometry3D(); }
        public static MeshGeometry3D AddTriangles(this MeshGeometry3D mesh,IEnumerable<Point3D>positions,IEnumerable<int> triangleIndices,IEnumerable<Vector3D>normals=null,IEnumerable<System.Windows.Point>textureCoordinates=null)
        {
            int offset = mesh.Positions.Count;
            foreach (var p in positions) mesh.Positions.Add(p);
            foreach (var i in triangleIndices) mesh.TriangleIndices.Add(i);
            if (normals != null) foreach (var v in normals) mesh.Normals.Add(v);
            if (textureCoordinates != null) foreach (var p in textureCoordinates) mesh.TextureCoordinates.Add(p);
            return mesh;
        }
        public static GeometryModel3D CreateModel(this MeshGeometry3D mesh,Brush brush)
        {
            mesh.Freeze();
            var model = new GeometryModel3D(mesh, new DiffuseMaterial(brush));
            //model.BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Red));
            return model;
        }
        public static Model3D CreateModel(Brush brush, IEnumerable<Point3D> ps)
        {
            double n = ps.Count();
            Trace.Assert(n >= 3);
            if (n == 3)
            {
                return NewModel().AddTriangles(ps, new int[] { 0, 1, 2 }).CreateModel(brush);
            }
            else
            {
                var indices = new List<int>();
                for (int i = 2; i < n; i++)
                {
                    indices.Add(0);
                    indices.Add(i - 1);
                    indices.Add(i);
                }
                return NewModel().AddTriangles(ps, indices).CreateModel(brush);
            }
        }
        public static Model3D CreateModel(Brush brush, Point3D p0, Point3D p1, Point3D p2)
        {
            return CreateModel(brush, new Point3D[] { p0, p1, p2 });
        }
        public static Model3D CreateModel(Brush brush, Point3D p0, Point3D p1, Point3D p2, params Point3D[] p3)
        {
            return CreateModel(brush, new Point3D[] { p0, p1, p2 }.Concat(p3));
        }
        #region model
        public static Model3DGroup CreateHex(List<double> angles, double r, double depth)
        {
            //System.Windows.MessageBox.Show(string.Join(", ", angles.Select(a => a / Math.PI * 180)));
            Model3DGroup hex = new Model3DGroup();
            var p = angles.Select(a => new Point3D(r * Math.Cos(a), 0, r * Math.Sin(a))).ToArray();
            p = p.Concat(p.Select(v => new Point3D(v.X, depth, v.Z))).ToArray();
            var b = new SolidColorBrush(Colors.SlateGray);
            System.Diagnostics.Trace.Assert(angles.Count == 6);
            hex.Children.Add(CreateModel(b, p[0], p[1], p[2], p[3], p[4], p[5]));
            hex.Children.Add(CreateModel(b, p[11], p[10], p[9], p[8], p[7], p[6]));
            int n = angles.Count();
            for (int i = 0; i < n; i++) hex.Children.Add(CreateModel(b, p[i], p[n + i], p[n + (i + 1) % n], p[(i + 1) % n]));
            return hex;
        }
        public static ModelVisual3D CreatePlane()
        {
            Model3DGroup triangle = new Model3DGroup();
            Point3D p1 = new Point3D(-5, -5, 0);
            Point3D p2 = new Point3D(5, -5, 0);
            Point3D p3 = new Point3D(-5, 5, 0);
            Point3D p4 = new Point3D(5, 5, 0);
            triangle.Children.Add(CreateModel(new SolidColorBrush(Colors.BlueViolet), p1, p2, p4, p3));
            triangle.Children.Add(CreateModel(new SolidColorBrush(Colors.SlateBlue), p1, p3, p4, p2));
            return new ModelVisual3D { Content = triangle };
        }
        #endregion
    }
}
