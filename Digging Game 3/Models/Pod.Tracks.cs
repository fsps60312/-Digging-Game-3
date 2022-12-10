using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System;

namespace Digging_Game_3.Models
{
    partial class Pod
    {
        public partial class Tracks : My3DObject//point +x, upward +z
        {
            public partial class Track : My3DObject
            {
                public class Tooth : My3DObject
                {
                    const int TransformIndexPosition = 0;
                    public Tooth(Point3D position) : base(position)
                    {
                        Kernel.Heart.Beat2 += () => UpdateTransform();
                    }
                    private Point3D _Position;
                    public Point3D Position
                    {
                        get { return _Position; }
                        set { MyLib.Set(SubTransforms, TransformIndexPosition, true).TranslatePrepend((_Position = value) - new Point3D()).Done(); }
                    }
                    protected override Model3D CreateModel(params object[] vs)
                    {
                        SubTransforms.Add(new MatrixTransform3D());
                        MyLib.AssertTypes(vs, typeof(Point3D));
                        Position = (Point3D)vs[0];
                        My3DGraphics.Cuboid.AddFaces(new Point3D(0.1, 0.1, 0.1), out List<Point3D> vertices, out List<int> triangleIndices, out List<Vector3D> normals);
                        return My3DGraphics.NewModel().AddTriangles(vertices, triangleIndices, normals).CreateModel(new SolidColorBrush(Colors.White));
                    }
                }
                //double BodyRotationY = 0;
                //double BodyRotationZ = 0;
                //Point3D BodyPosition = new Point3D();
                Body Parent;
                public bool IsOnGround { get { return groundGears.All(g => g.IsOnGround); } }
                public Track(Body parent,Vector3D offset):base(parent,offset)
                {
                    {
                        double r = 0;
                        var initialChainLength = GetChainLength(gears);
                        Kernel.Heart.Beat1+= (secs) =>
                          {
                              var chainLength = GetChainLength(gears);
                              if (double.IsNaN(chainLength)) chainLength = initialChainLength;
                              r -= secs * Parent.TrackSpeed / chainLength;
                              r = (r % 1 + 1) % 1;
                              for (int i = 0; i < Teeth.Count; i++)
                              {
                                  Teeth[i].Position = GetToothPosition(gears, ((double)i / Teeth.Count + r) % 1);
                              }
                          };
                    }
                }
                List<Gear> gears = new List<Gear>(), groundGears = new List<Gear>();
                private Vector3D GetChainTouchPoint(Gear a, Gear b)
                {
                    var angle = Math.PI / 2 - Math.Acos((a.Radius - b.Radius) / (a.Position - b.Position).Length);
                    var mat = Matrix3D.Identity;
                    mat.Rotate(new Quaternion(new Vector3D(0, 0, 1) * Parent.MatrixY, angle / Math.PI * 180));
                    var ans = Vector3D.CrossProduct(b.Position - a.Position, new Vector3D(0, 0, 1) * Parent.MatrixY) * mat;
                    ans.Normalize();
                    return ans;
                }
                private double GetChainLength(List<Gear>chain)
                {
                    double ans = 0;
                    for(int i=0;i<chain.Count;i++)
                    {
                        var a = chain[i];
                        var b = chain[(i + 1) % chain.Count];
                        var c = chain[(i + 2) % chain.Count];
                        ans += Math.Sqrt(Math.Pow((b.Position - a.Position).Length, 2) + Math.Pow(b.Radius - a.Radius, 2));
                        ans += Vector3D.AngleBetween(GetChainTouchPoint(a, b), GetChainTouchPoint(b, c)) / 180 * Math.PI * b.Radius;
                    }
                    return ans;
                }
                private Point3D GetToothPosition(List<Gear> chain,double ratio)
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
                        l = Math.Sqrt(Math.Pow((b.Position - a.Position).Length, 2) + Math.Pow(b.Radius - a.Radius, 2));
                        if (ans + l <= target) ans += l;
                        else
                        {
                            var r = (target - ans) / l;
                            return new Point3D() + (1-r) * (a.Position + t1 * a.Radius - new Point3D()) + r * (b.Position + t1 * b.Radius - new Point3D());
                        }
                        var angleDegrees = Vector3D.AngleBetween(t1, t2);
                        l = angleDegrees / 180 * Math.PI * b.Radius;
                        if (ans + l <= target) ans += l;
                        else
                        {
                            var r = (target - ans) / l;
                            var mat = Matrix3D.Identity;
                            mat.Rotate(new Quaternion(new Vector3D(0, 0, 1)*Parent.MatrixY, angleDegrees * r));
                            return b.Position + t1 * mat * b.Radius;
                        }
                    }
                    throw new Exception($"ratio: {ratio}");
                }
                List<Tooth> Teeth = new List<Tooth>();
                protected override Model3D CreateModel(params object[] vs)
                {
                    //const double suspensionHardness = 10;
                    MyLib.AssertTypes(vs,typeof(Body),typeof(Vector3D));
                    Parent = vs[0] as Body;
                    var offset = (Vector3D)vs[1];
                    //return TemporaryModel();
                    const double depth = 1, height = 1, lengthUp = 4, lengthDown = 2.5;
                    var chain = new List<Tuple<Point3D, double,double, double>>
                    {                       //      relative position,                                  radius, mass,   suspension hardness
                        Tuple.Create(offset+ new Point3D(-lengthUp / 2, -height / 2+0.4, 0),                   0.4,    0.02,   7.0   ),
                        Tuple.Create(offset+ new Point3D((lengthDown*1-lengthUp*3) / 4/2, -height / 2+0.2, 0), 0.2,    0.025,  10.0  ),
                        Tuple.Create(offset+ new Point3D((lengthDown*2-lengthUp*2) / 4/2, -height / 2+0.2, 0), 0.2,    0.03,   10.0  ),
                        Tuple.Create(offset+ new Point3D((lengthDown*3-lengthUp*1) / 4/2, -height / 2+0.2, 0), 0.2,    0.03,   20.0  ),
                        Tuple.Create(offset+ new Point3D(lengthDown / 2, -height / 2+0.5, 0),                  0.5,    0.045,  15.0  ),
                        Tuple.Create(offset+ new Point3D(lengthUp / 2, height / 2, 0),                         0.2,    0.01,   10.0  ),
                        Tuple.Create(offset+ new Point3D(-lengthUp/10, height / 2, 0),                         0.3,    0.01,   10.0  ),
                    };
                    Model3DGroup ans = new Model3DGroup();
                    foreach (var gv in chain)
                    {
                        var gear = new Gear(gv.Item1, gv.Item2, gv.Item4, gv.Item3, Parent);
                        ans.Children.Add(gear.Model);
                        gears.Add(gear);
                    }
                    foreach (var i in new[] { 3, 4 }) groundGears.Add(gears[i]);
                    double chainLength = GetChainLength(gears);
                    int count =(int)( chainLength / 0.5);
                    for (int i = 0; i < count; i++)
                    {
                        var t = new Tooth(GetToothPosition(gears, (double)i / count));
                        ans.Children.Add(t.Model);
                        Teeth.Add(t);
                    }
                    return ans;
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
            public Tracks(Body parent,Vector3D offset) : base(parent,offset)
            {
                parent.IsOnGround = () => leftTrack.IsOnGround && rigtTrack.IsOnGround;
            }
            Track leftTrack, rigtTrack;
            protected override Model3D CreateModel(params object[] vs)
            {
                MyLib.AssertTypes(vs, typeof(Body),typeof(Vector3D));
                var parent = vs[0] as Body;
                var offset = (Vector3D)vs[1];
                const double gap = 2.5 + 1;
                leftTrack = new Track(parent,offset+new Vector3D(0,0, -gap / 2));
                rigtTrack = new Track(parent,offset+new Vector3D(0,0, gap / 2));
                //leftTrack.Transform = leftTrack.OriginTransform = MyLib.Transform(leftTrack).TranslatePrepend(new Vector3D(0, 0, -gap / 2)).Value;
                //rigtTrack.Transform = rigtTrack.OriginTransform = MyLib.Transform(rigtTrack).TranslatePrepend(new Vector3D(0, 0, gap / 2)).Value;
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
