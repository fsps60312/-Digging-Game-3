using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System;

namespace Digging_Game_3.Models
{
    partial class Pod
    {
        public class Tracks : My3DObject//point +x, upward +z
        {
            public class Track : My3DObject
            {
                public class Tooth : My3DObject
                {
                    public Tooth(Point3D position) : base(position) { }
                    public Point3D Position;
                    protected override Model3D CreateModel(params object[] vs)
                    {
                        MyLib.AssertTypes(vs, typeof(Point3D));
                        Position = (Point3D)vs[0];
                        My3DGraphics.Cuboid.AddFaces(new Point3D(0.1, 0.1, 0.1), out List<Point3D> vertices, out List<int> triangleIndices, out List<Vector3D> normals);
                        return My3DGraphics.NewModel().AddTriangles(vertices.Select(p=>Position+(p-new Point3D())), triangleIndices, normals).CreateModel(new SolidColorBrush(Colors.White));
                    }
                }
                public class Gear : My3DObject
                {
                    const int TransformIndexPosition = 0;
                    const int TransformIndexSpin = 1;
                    public Point3D DesiredPosition;
                    public double Radius;
                    public Gear(Point3D desiredPosition,double radius) : base(desiredPosition,radius)
                    {
                        SubTransforms.Add(new MatrixTransform3D());
                        Kernel.Heart.Beat += secs =>
                          {
                              MyLib.Set(SubTransforms, TransformIndexPosition, true).TranslatePrepend(DesiredPosition - new Point3D()).Done();
                              UpdateTransform();
                          };
                    }
                    protected override Model3D CreateModel(params object[] vs)
                    {
                        MyLib.AssertTypes(vs, typeof(Point3D),typeof(double));
                        DesiredPosition = (Point3D)vs[0];
                        Radius = (double)vs[1];
                        const int n = 10;
                        List<Point3D> positions = new List<Point3D>();
                        for (int i = 0; i < n; i++) positions.Add(new Point3D(Radius * Math.Cos(2 * Math.PI * i / n), Radius * Math.Sin(2 * Math.PI * i / n), 0));
                        List<int> triangleIndices = new List<int>();
                        for (int i = 2; i < n; i++) triangleIndices.AddRange(new[] { 0, i - 1, i });
                        triangleIndices = triangleIndices.Concat(triangleIndices.Reverse<int>()).ToList();
                        return My3DGraphics.NewModel().AddTriangles(positions, triangleIndices).CreateModel(new SolidColorBrush(Colors.Gray));
                    }
                }
                public Track():base()
                {
                    double r = 0;
                    Kernel.Heart.Beat += secs =>
                      {
                          r -= secs * 0.1;
                          r = (r % 1 + 1) % 1;
                          for (int i = 0; i < Teeth.Count; i++)
                          {
                              Teeth[i].Transform = MyLib.Transform(new MatrixTransform3D()).Translate(GetToothPosition(chain, ((double)i / Teeth.Count + r) % 1) - Teeth[i].Position).Value;
                          }
                      };
                }
                const double depth = 1, height = 1, lengthUp = 4, lengthDown = 2.5;
                List<Tuple<Point3D, double>> chain = new List<Tuple<Point3D, double>>
                    {
                        Tuple.Create( new Point3D(-lengthUp / 2, -height / 2+0.4, 0),0.4 ),
                        Tuple.Create( new Point3D(-lengthUp/10, height / 2, 0),0.3 ),
                        Tuple.Create( new Point3D(lengthUp / 2, height / 2, 0),0.2 ),
                        Tuple.Create( new Point3D(lengthDown / 2, -height / 2+0.5, 0),0.5 ),
                        Tuple.Create( new Point3D((lengthDown*3-lengthUp*1) / 4/2, -height / 2+0.2, 0),0.2 ),
                        Tuple.Create( new Point3D((lengthDown*2-lengthUp*2) / 4/2, -height / 2+0.2, 0),0.2 ),
                        Tuple.Create( new Point3D((lengthDown*1-lengthUp*3) / 4/2, -height / 2+0.2, 0),0.2 )
                    }.Reverse<Tuple<Point3D, double>>().ToList();
                List<Gear> gears = new List<Gear>();
                private Vector3D GetChainTouchPoint(Tuple<Point3D, double>a, Tuple<Point3D, double>b)
                {
                    var angle = Math.PI / 2 - Math.Acos((a.Item2 - b.Item2) / (a.Item1 - b.Item1).Length);
                    var mat = Matrix3D.Identity;
                    mat.Rotate(new Quaternion(new Vector3D(0, 0, 1), angle / Math.PI * 180));
                    var ans = Vector3D.CrossProduct(b.Item1 - a.Item1, new Vector3D(0, 0, 1)) * mat;
                    ans.Normalize();
                    return ans;
                }
                private double GetChainLength(List<Tuple<Point3D,double>>chain)
                {
                    double ans = 0;
                    for(int i=0;i<chain.Count;i++)
                    {
                        var a = chain[i];
                        var b = chain[(i + 1) % chain.Count];
                        var c = chain[(i + 2) % chain.Count];
                        ans += Math.Sqrt(Math.Pow((b.Item1 - a.Item1).Length, 2) + Math.Pow(b.Item2 - a.Item2, 2));
                        ans += Vector3D.AngleBetween(GetChainTouchPoint(a, b), GetChainTouchPoint(b, c)) / 180 * Math.PI * b.Item2;
                    }
                    return ans;
                }
                private Point3D GetToothPosition(List<Tuple<Point3D, double>> chain,double ratio)
                {
                    double target = GetChainLength(chain) * ratio;
                    double ans = 0;
                    for (int i = 0; i < chain.Count; i++)
                    {
                        var a = chain[i];
                        var b = chain[(i + 1) % chain.Count];
                        var c = chain[(i + 2) % chain.Count];
                        var t1 = GetChainTouchPoint(a, b);
                        var t2 = GetChainTouchPoint(b, c);
                        double l;
                        l = Math.Sqrt(Math.Pow((b.Item1 - a.Item1).Length, 2) + Math.Pow(b.Item2 - a.Item2, 2));
                        if (ans + l <= target) ans += l;
                        else
                        {
                            var r = (target - ans) / l;
                            return new Point3D() + (1-r) * (a.Item1 + t1 * a.Item2 - new Point3D()) + r * (b.Item1 + t1 * b.Item2 - new Point3D());
                        }
                        var angleDegrees = Vector3D.AngleBetween(t1, t2);
                        l = angleDegrees / 180 * Math.PI * b.Item2;
                        if (ans + l <= target) ans += l;
                        else
                        {
                            var r = (target - ans) / l;
                            var mat = Matrix3D.Identity;
                            mat.Rotate(new Quaternion(new Vector3D(0, 0, 1), angleDegrees * r));
                            return b.Item1 + t1 * mat * b.Item2;
                        }
                    }
                    throw new Exception($"ratio: {ratio}");
                }
                List<Tooth> Teeth = new List<Tooth>();
                protected override Model3D CreateModel(params object[] vs)
                {
                    MyLib.AssertTypes(vs);
                    Model3DGroup ans = new Model3DGroup();
                    foreach (var p in chain)
                    {
                        var gear = new Gear(p.Item1,p.Item2);
                        ans.Children.Add(gear.Model);
                        gears.Add(gear);
                    }
                    double chainLength = GetChainLength(chain);
                    int count =(int)( chainLength / 0.5);
                    for(int i=0;i<count;i++)
                    {
                        var t = new Tooth(GetToothPosition(chain, (double)i / count));
                        ans.Children.Add(t.Model);
                        Teeth.Add(t);
                    }
                    return ans;
                    //return TemporaryModel();
                }
                static Model3D TemporaryModel()
                {
                    const double depth = 1, height = 1, lengthUp = 4, lengthDown = 2.5;
                    List<Point3D> positions = new List<Point3D> { new Point3D(-lengthUp / 2, height / 2, 0), new Point3D(lengthUp / 2, height / 2, 0), new Point3D(lengthDown / 2, -height / 2, 0), new Point3D(-lengthDown / 2, -height / 2, 0) };
                    List<int> indices = new List<int> { 0, 3, 2, 0, 2, 1 };
                    List<Point3D> trackPositions = positions.SelectConcat(v => v + new Vector3D(0, 0, depth / 2), v => v + new Vector3D(0, 0, -depth / 2)).ToList();
                    ///4----5
                    /// 7--6
                    ///
                    ///0----1
                    /// 3--2
                    List<int> trackIndices = indices.Concat(indices.Select(v => positions.Count + v).Reverse())
                        .Concat(new[] { 0, 1, 5, 0, 5, 4 }.SelectAdd(v => v / 4 * 4 + (v + 1) % 4, v => v / 4 * 4 + (v + 2) % 4, v => v / 4 * 4 + (v + 3) % 4)).ToList();
                    return My3DGraphics.NewModel().AddTriangles(trackPositions, trackIndices).CreateModel(new SolidColorBrush(Colors.SlateGray));
                    //List<Point3D> tracksPositions = trackPositions.SelectConcat(v => v + new Vector3D(0, 0, gap / 2), v => v + new Vector3D(0, 0, -gap / 2)).ToList();
                    //List<int> tracksIndices = trackIndices.Concat(trackIndices.Select(v => trackPositions.Count + v)).ToList();
                }
            }
            Track leftTrack, rigtTrack;
            protected override Model3D CreateModel(params object[] vs)
            {
                const double gap = 2.5+1;
                leftTrack = new Track();
                rigtTrack = new Track();
                leftTrack.Transform = leftTrack.OriginTransform = MyLib.Transform(leftTrack).TranslatePrepend(new Vector3D(0, 0, -gap / 2)).Value;
                rigtTrack.Transform = rigtTrack.OriginTransform = MyLib.Transform(rigtTrack).TranslatePrepend(new Vector3D(0, 0, gap / 2)).Value;
                var ans = new Model3DGroup();
                ans.Children.Add(leftTrack.Model);
                ans.Children.Add(rigtTrack.Model);
                return ans;
            }
        }
        //void AddSphere(Model3DGroup ans)
        //{
        //    var mercury = My3DGraphics.Sphere.Create(1).CreateModel(new ImageBrush(new System.Windows.Media.Imaging.BitmapImage(new Uri(new FileInfo(@"..\Mercury.jpg").FullName))));
        //    Kernel.Heart.Beat += (secs) =>
        //    {
        //        mercury.Transform = MyLib.Transform(mercury).RotatePrepend(new Vector3D(0, 0, 1), secs * Math.PI / 5).Value;
        //    };
        //    ans.Children.Add(mercury);//@"C:\O\OneDrive - csie.ntu.edu.tw\Downloads\Mercury.jpg" 
        //}
    }
}
