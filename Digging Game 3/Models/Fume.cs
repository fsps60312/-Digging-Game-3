using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Digging_Game_3.Models
{
    class Fumes : My3DObject
    {
        public static Fumes Instance;
        public Fumes():base()
        {
            Kernel.Heart.Beat += secs =>
              {
                  List<Fume> trash = new List<Fume>();
                  foreach (var fume in fumes)
                  {
                      fume.Update(secs);
                      if (fume.Disappeared)
                      {
                          (Model as Model3DGroup).Children.Remove(fume.Model);
                          trash.Add(fume);
                      }
                  }
                  foreach (var fume in trash) fumes.Remove(fume);
              };
        }
        SortedSet<Fume> fumes = new SortedSet<Fume>(Comparer<Fume>.Create( (a, b) => a.Position.Z.CompareTo(b.Position.Z)));
        public void AddFums(Point3D position,Vector3D speed, Color color, double radiusStart,double radiusEnd,double lifeTime,double speedRandomness)
        {
            Fume fume = null;
            fume = new Fume(position, speed, color, radiusStart, radiusEnd, lifeTime, speedRandomness);
            fumes.Add(fume);
            (Model as Model3DGroup).Children.Clear();
            foreach (var f in fumes)(Model as Model3DGroup).Children.Add(f.Model);
        }
        protected override Model3D CreateModel(params object[] vs)
        {
            return new Model3DGroup();
        }
    }
    class Fume : My3DObject
    {
        public Point3D Position;
        public Vector3D Speed;
        public double RadiusStart, RadiusEnd;
        public Color Color;
        public double LifeTime;
        public double SpeedRandomness;
        double timePassed = 0;
        public bool Disappeared { get; private set; } = false;
        public Fume(Point3D position, Vector3D speed, Color color, double radiusStart, double radiusEnd, double lifeTime,double speedRandomness)
            : base(position, speed, color, radiusStart, radiusEnd, lifeTime, speedRandomness) { }
        public void Update(double secs)
        {
            timePassed += secs;
            var ratio = timePassed / LifeTime;
            if (ratio >=1) { Disappeared=true; return; }
            (Model as GeometryModel3D).Material = new DiffuseMaterial { Brush = new SolidColorBrush(Color.FromArgb((byte)(Color.A * (1 - ratio)), Color.R, Color.G, Color.B))};
            Position += secs * Speed;
            var angle = MyLib.Rand.NextDouble() * 2 * Math.PI;
            Speed += secs * SpeedRandomness * new Vector3D(Math.Cos(angle), Math.Sin(angle), 0);
            Transform = MyLib.Transform(MatrixTransform3D.Identity).TranslatePrepend(Position - new Point3D()).ScalePrepend(new Vector3D(1,1,1)*(1+(RadiusEnd-RadiusStart)/RadiusStart*ratio)).Value;
        }
        protected override Model3D CreateModel(params object[] vs)
        {
            MyLib.AssertTypes(vs, typeof(Point3D), typeof(Vector3D), typeof(Color), typeof(double), typeof(double), typeof(double), typeof(double));
            Position = (Point3D)vs[0];
            Speed = (Vector3D)vs[1];
            Color = (Color)vs[2];
            RadiusStart = (double)vs[3];
            RadiusEnd = (double)vs[4];
            LifeTime = (double)vs[5];
            SpeedRandomness = (double)vs[6];
            const int n = 10;
            List<Point3D> positions = new List<Point3D>();
            for (int i = 0; i < n; i++) positions.Add(new Point3D(RadiusStart * Math.Cos(2 * Math.PI * i / n), RadiusStart* Math.Sin(2 * Math.PI * i / n), 0));
            List<int> triangleIndices = new List<int>();
            for (int i = 2; i < n; i++) triangleIndices.AddRange(new[] { 0, i - 1, i });
            return My3DGraphics.NewModel().AddTriangles(positions, triangleIndices).CreateModel(new SolidColorBrush(Color));
        }
    }
}
