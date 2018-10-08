using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Windows.Media;

namespace Digging_Game_3.Models
{
    partial class Blocks : My3DObject
    {
        public static bool IsCollidable(Point3D p, out int x, out int y)
        {
            x = (int)Math.Floor((p.X - Blocks.Anchor.X) / Blocks.Width);
            y = (int)Math.Floor((p.Y - Blocks.Anchor.Y) / Blocks.Height);
            return Blocks.IsCollidable(x, y);
        }
        public static bool IsCollidable(int x, int y) { return y <= -5 || y >= 9 || x <= -5 || x >= 5 || (x >= -1 && x <= 1 && y == 2); }
    }
    partial class Blocks : My3DObject
    {
        public const double Width = 5, Height = 4, Depth = 2.9;
        public static readonly Vector3D Anchor = new Vector3D(0, 0, -1.5);
        LinkedList<Tuple<int, LinkedList<Tuple<int, Block>>>> blocks = new LinkedList<Tuple<int, LinkedList<Tuple<int, Block>>>>();
        Dictionary<int, HashSet<Block>> blockRecycle = new Dictionary<int, HashSet<Block>> { { 0, new HashSet<Block>() }, { 1, new HashSet<Block>() } };
        void RecycleBlock(Block b)
        {
            if (b == null) return;
            (Model as Model3DGroup).Children.Remove(b.Model);
            var a = b.Anchor - Anchor;
            blockRecycle[((int)Math.Round(a.X / Width + a.Y / Height) % 2 + 2) % 2].Add(b);
        }
        void RemoveXMin()
        {
            foreach (var b in blocks.First.Value.Item2) RecycleBlock(b.Item2);
            blocks.RemoveFirst();
        }
        void RemoveXMax()
        {
            foreach (var b in blocks.Last.Value.Item2) RecycleBlock(b.Item2);
            blocks.RemoveLast();
        }
        void RemoveYMin()
        {
            foreach (var b in blocks)
            {
                RecycleBlock(b.Item2.First.Value.Item2);
                b.Item2.RemoveFirst();
            }
            if (blocks.First.Value.Item2.Count == 0) blocks.Clear();
        }
        void RemoveYMax()
        {
            foreach (var b in blocks)
            {
                RecycleBlock(b.Item2.Last.Value.Item2);
                b.Item2.RemoveLast();
            }
            if (blocks.First.Value.Item2.Count == 0) blocks.Clear();
        }
        Block NewBlock(int xi, int yi)
        {
            //if (yi > 0) return null;
            if (!IsCollidable(xi, yi)) return null;
            var anchor = Anchor + new Vector3D(Width * xi, Height * yi, 0);
            if (blockRecycle[0].Count > 0)
            {
                var b = blockRecycle[0].First();
                blockRecycle[0].Remove(b);
                b.Reset(anchor);
                (Model as Model3DGroup).Children.Add(b.Model);
                return b;
            }
            else
            {
                Block b = new Block(anchor, new Size3D(Width, Height, Depth), new SolidColorBrush(Color.FromArgb(128, (byte)MyLib.Rand.Next(128 - 20, 128 + 20), (byte)MyLib.Rand.Next(72 - 10, 72 + 10), 0)));
                (Model as Model3DGroup).Children.Add(b.Model);
                return b;
            }
        }
        void AppendYMin()
        {
            foreach (var b in blocks)
            {
                int yI = b.Item2.First.Value.Item1 - 1;
                b.Item2.AddFirst(new Tuple<int, Block>(yI, NewBlock(b.Item1, yI)));
            }
        }
        void AppendYMax()
        {
            foreach (var b in blocks)
            {
                int yI = b.Item2.Last.Value.Item1 + 1;
                b.Item2.AddLast(new Tuple<int, Block>(yI, NewBlock(b.Item1, yI)));
            }
        }
        void AppendXMin()
        {
            int xI = blocks.First.Value.Item1 - 1;
            var newList = blocks.First.Value.Item2.Select(b => new Tuple<int, Block>(b.Item1, NewBlock(xI, b.Item1)));
            blocks.AddFirst(new Tuple<int, LinkedList<Tuple<int, Block>>>(xI, new LinkedList<Tuple<int, Block>>(newList)));
        }
        void AppendXMax()
        {
            int xI = blocks.Last.Value.Item1 + 1;
            var newList = blocks.Last.Value.Item2.Select(b => new Tuple<int, Block>(b.Item1, NewBlock(xI, b.Item1)));
            blocks.AddLast(new Tuple<int, LinkedList<Tuple<int, Block>>>(xI, new LinkedList<Tuple<int, Block>>(newList)));
        }
        void RegoinOnXYPlane(out double xMin, out double xMax, out double yMin, out double yMax)
        {
            var a = Kernel.Camera.FieldOfView / 180 * Math.PI / 2;
            Point3D o = new Point3D();
            Point3D[] ps = new[]
            {
                o + new Vector3D(-Math.Sin(a), -Math.Sin(a)*MainWindow.HeightRatio, -Math.Cos(a)),
                o + new Vector3D(-Math.Sin(a), Math.Sin(a)*MainWindow.HeightRatio, -Math.Cos(a)),
                o + new Vector3D(Math.Sin(a), -Math.Sin(a)*MainWindow.HeightRatio, -Math.Cos(a)),
                o + new Vector3D(Math.Sin(a), Math.Sin(a)*MainWindow.HeightRatio, -Math.Cos(a))
            };
            var mat = Kernel.Camera.Transform.Value;
            o *= mat;
            ps = ps.Select(p => p * mat).ToArray();
            ps = ps.Select(p => o - (p - o) / (p - o).Z * o.Z).ToArray();
            //System.Diagnostics.Trace.WriteLine(string.Join(", ", ps.Select(p => p.Z)));
            xMin = ps.Min(p => p.X);
            xMax = ps.Max(p => p.X);
            yMin = ps.Min(p => p.Y);
            yMax = ps.Max(p => p.Y);
        }
        public Blocks()
        {
            Kernel.Heart.Beat += (secs) =>
            {
                //int xI = (int)Math.Round((Kernel.CameraProperties.position.X - Anchor.X) / Width);
                //int yI = (int)Math.Round((Kernel.CameraProperties.position.Y - Anchor.Y) / Height);
                RegoinOnXYPlane(out double xMind, out double xMaxd, out double yMind, out double yMaxd);
                //System.Diagnostics.Trace.WriteLine($"{xMind}, {xMaxd}, {yMind}, {yMaxd}");
                //int xMin = xI - 10, xMax = xI + 10, yMin = yI - 10, yMax = yI + 10;
                int
                xMin = (int)Math.Floor((xMind - Anchor.X) / Width) - 1, xMax = (int)Math.Ceiling((xMaxd - Anchor.X) / Width) + 1,
                yMin = (int)Math.Floor((yMind - Anchor.Y) / Height) - 1, yMax = (int)Math.Ceiling((yMaxd - Anchor.Y) / Height) + 1;
                while (blocks.Count > 0 && blocks.First.Value.Item1 < xMin) RemoveXMin();
                while (blocks.Count > 0 && blocks.Last.Value.Item1 > xMax) RemoveXMax();
                while (blocks.Count > 0 && blocks.First.Value.Item2.First.Value.Item1 < yMin) RemoveYMin();
                while (blocks.Count > 0 && blocks.First.Value.Item2.Last.Value.Item1 > yMax) RemoveYMax();
                if (blocks.Count == 0) blocks.AddFirst(new Tuple<int, LinkedList<Tuple<int, Block>>>(xMin, new LinkedList<Tuple<int, Block>>(new[] { new Tuple<int, Block>(yMin, NewBlock(xMin, yMin)) })));
                while (blocks.First.Value.Item2.First.Value.Item1 > yMin) AppendYMin();
                while (blocks.First.Value.Item2.Last.Value.Item1 < yMax) AppendYMax();
                while (blocks.First.Value.Item1 > xMin) AppendXMin();
                while (blocks.Last.Value.Item1 < xMax) AppendXMax();
            };
        }
        protected override Model3D CreateModel(params object[] vs)
        {
            return new Model3DGroup();
        }
    }
    class Block : My3DObject
    {
        public Vector3D Anchor { get; private set; }
        public Block(Vector3D anchor,Size3D size,Brush brush) : base(anchor, size,brush) { }
        protected override Model3D CreateModel(params object[] vs)
        {
            MyLib.AssertTypes(vs, typeof(Vector3D), typeof(Size3D), typeof(Brush));
            var anchor = Anchor = (Vector3D)vs[0];
            var size = (Size3D)vs[1];
            var brush = (Brush)vs[2];
            My3DGraphics.Cuboid.AddFaces(new Point3D(size.X / 2, size.Y / 2, size.Z / 2), out List<Point3D> vertices, out List<int> triangleIndices, out List<Vector3D> normals, "xy-z");
            vertices = vertices.Select(p => p + new Vector3D(size.X, size.Y, size.Z) / 2 + anchor).ToList();
            System.Diagnostics.Trace.Assert(vertices.Count == 5 * 4 && triangleIndices.Count == 5 * 6 && normals.Count == 5 * 4);
            for (int k = 0; k <= 0; k++)
            {
                for (int i = k * 6, j = (k + 1) * 6 - 1; i < j; i++, j--) { var t = triangleIndices[i]; triangleIndices[i] = triangleIndices[j]; triangleIndices[j] = t; }
                for (int i = k * 4; i < (k + 1) * 4; i++) normals[i] *= -1;
            }
            return My3DGraphics.NewModel().AddTriangles(vertices, triangleIndices, normals).CreateModel(brush);
        }
        public void Reset(Vector3D anchor)
        {
            this.Transform = MyLib.Transform(OriginTransform).TranslatePrepend(anchor - Anchor).Value;
        }
    }
}
