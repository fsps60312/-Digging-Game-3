using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Digging_Game_3.Models
{
    partial class Pod
    {
        partial class Propeller
        {
            partial class BladeSet
            {
                class Blade : My3DObject//↺
                {
                    public enum Types { BasicTriangle }
                    static readonly Dictionary<Types, Description> Descriptions = new Dictionary<Types, Description>
                    {
                        {Types.BasicTriangle,new Description(MyLib.FromAngular(0,0), MyLib.FromAngular(1,0),MyLib.FromAngular(0.9,MyLib.ToRad(-7)),MyLib.FromAngular(0.2,MyLib.ToRad(-40))) }
                    };
                    class Description
                    {
                        public Point3D[] Vertexes { get; private set; }
                        public Description(params Point3D[] vertexes)
                        {
                            Vertexes = vertexes;
                        }
                    }
                    public Blade(Types type, double radius,double angle, bool reversed) : base(type, radius,angle, reversed) { }
                    Model3DGroup CreateModel(Description description, double scale, double angle, bool reversed)
                    {
                        var vs = description.Vertexes.Select(v => v.Multiply(scale));
                        if (reversed)
                        {
                            vs = vs.Select(v => new Point3D(v.X, -v.Y, v.Z)).Reverse();
                            angle = -angle;
                        }
                        var front = My3DGraphics.NewModel().AddTriangles(vs, new[] { 3, 2, 0, 2, 1, 0 }).CreateModel(Brushes.Violet);
                        var back = My3DGraphics.NewModel().AddTriangles(vs, new[] { 0, 1, 2, 0, 2, 3 }).CreateModel(Brushes.DarkViolet);
                        front.Transform = back.Transform = MyLib.Transform(front).Rotate(new Vector3D(1, 0, 0), angle).Value;
                        return new Model3DGroup() { Children = { front, back } };
                    }
                    protected override Model3D CreateModel(params object[] vs)
                    {
                        MyLib.AssertTypes(vs, typeof(Types), typeof(double),typeof(double), typeof(bool));
                        return CreateModel(Descriptions[(Types)vs[0]], (double)vs[1],(double)vs[2], (bool)vs[3]);
                    }
                }
            }
        }
    }
}
