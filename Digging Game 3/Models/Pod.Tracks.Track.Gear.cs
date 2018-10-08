using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System;

namespace Digging_Game_3.Models
{
    partial class Pod
    {
        public partial class Tracks
        {
            public partial class Track
            {
                public class Gear : My3DObject
                {
                    const int TransformIndexPosition = 0;
                    const int TransformIndexSpin = 1;
                    public Point3D Position{get { return RB.position; }}
                    public Point3D DesiredPosition { get; private set; }
                    public double Radius { get; private set; }
                    public RigidBody RB { get; private set; }
                    public double SuspensionHardness { get; private set; }
                    public Gear(Point3D desiredPosition,double radius,double suspensionHardness, IsCollidableDelegate isCollidable) : base(desiredPosition,radius, suspensionHardness, isCollidable)
                    {
                        Kernel.Heart.Beat += secs =>
                        {
                            MyLib.Set(SubTransforms, TransformIndexPosition, true).TranslatePrepend(Position - new Point3D()).Done();
                            UpdateTransform();
                        };
                    }
                    public delegate bool IsCollidableDelegate(Point3D p, out int x, out int y);
                    IsCollidableDelegate IsCollidable;
                    public bool UpdateRigitBody(double secs, Func<bool>needRestore, Vector3D velocityChange,Action<Func< Vector3D>> reportReactionForce)
                    {
                        RB.force = new Vector3D(0, -RB.mass * Constants.Gravity, 0);
                        RB.force += SuspensionHardness * (DesiredPosition - Position);
                        {
                            var friction = -1 * RB.velocity;
                            RB.force += friction;
                        }
                        if (RB.Update(secs, rb =>
                        {
                            RB.velocity += velocityChange;
                            var dif = (rb.position - rb._position).Length;
                            if (dif > Radius / 5) return false;
                            int cur_x, cur_y, x = 0, y = 0;
                            IsCollidable(rb.position, out cur_x, out cur_y);
                            {
                                const double bounce = 0.5;
                                double t;
                                Point3D cp;
                                cp = rb.position + new Vector3D(-Radius, 0, 0);
                                if (IsCollidable(cp, out x, out y))
                                {
                                    t = cp.X * -Math.Sin(rb.theta) + cp.Y * -Math.Cos(rb.theta);
                                    if (x == cur_x - 1 && !Blocks.IsCollidable(cur_x, y) && rb.velocity.X + t * rb.omega < 0)//collide left, +x force. t=(cp.x*-Sin(theta)+cp.y*-Cos(theta)), (v.x+f/m) + (omega+f*t/I)*t = -b*(v.x + omega*t), f*(1/m+t^2/I)=(-b-1)*(v.x+omega*t)
                                    {
                                        System.Diagnostics.Trace.WriteLine("L");
                                        var f = (-bounce - 1) * (rb.velocity.X + rb.omega * t) / (1.0 / rb.mass + t * t / rb.momentOfInertia);
                                        //f += -rb.force.X * secs;
                                        rb.velocity.X += f / rb.mass;
                                        rb.omega += f * t / rb.momentOfInertia;
                                    }
                                }
                                cp = rb.position + new Vector3D(Radius, 0, 0);
                                if (IsCollidable(cp, out x, out y))
                                {
                                    t = cp.X * -Math.Sin(rb.theta) + cp.Y * -Math.Cos(rb.theta);
                                    if (x == cur_x + 1 && !Blocks.IsCollidable(cur_x, y) && rb.velocity.X + t * rb.omega > 0)//collide right, -x force. t=(cp.x*-Sin(theta)+cp.y*-Cos(theta)), (v.x-f/m) + (omega-f*t/I)*t = -b*(v.x + omega*t), f*(-1/m-t^2/I)=(-b-1)*(v.x+omega*t)
                                    {
                                        System.Diagnostics.Trace.WriteLine("R");
                                        var f = (-bounce - 1) * (rb.velocity.X + rb.omega * t) / (-1.0 / rb.mass - t * t / rb.momentOfInertia);
                                        //f += rb.force.X * secs;
                                        rb.velocity.X -= f / rb.mass;
                                        rb.omega -= f * t / rb.momentOfInertia;
                                    }
                                }
                                cp = rb.position + new Vector3D(0, -Radius, 0);
                                if (IsCollidable(cp, out x, out y))
                                {
                                    t = cp.Y * -Math.Sin(rb.theta) + cp.X * Math.Cos(rb.theta);
                                    if (y == cur_y - 1 && !Blocks.IsCollidable(x, cur_y) && rb.velocity.Y + t * rb.omega < 0)//collide down, +y force. t=(cp.y*-Sin(theta)+cp.x*Cos(theta)), (v.y+f/m) + (omega+f*t/I)*t = -b*(v.y + omega*t), f*(1/m+t^2/I)=(-b-1)*(v.y+omega*t)
                                    {
                                        System.Diagnostics.Trace.WriteLine("D");
                                        var f = (-bounce - 1) * (rb.velocity.Y + rb.omega * t) / (1.0 / rb.mass + t * t / rb.momentOfInertia);
                                        //f -= rb.force.Y * secs;
                                        rb.velocity.Y += f / rb.mass;
                                        rb.omega += f * t / rb.momentOfInertia;
                                    }
                                }
                                cp = rb.position + new Vector3D(0, Radius, 0);
                                if (IsCollidable(cp, out x, out y))
                                {
                                    t = cp.Y * -Math.Sin(rb.theta) + cp.X * Math.Cos(rb.theta);
                                    if (y == cur_y + 1 && !Blocks.IsCollidable(x, cur_y) && rb.velocity.Y + t * rb.omega > 0)//collide up, -y force. t=(cp.y*-Sin(theta)+cp.x*Cos(theta)), (v.y-f/m) + (omega-f*t/I)*t = -b*(v.y + omega*t), f*(-1/m-t^2/I)=(-b-1)*(v.y+omega*t)
                                    {
                                        System.Diagnostics.Trace.WriteLine("U");
                                        var f = (-bounce - 1) * (rb.velocity.Y + rb.omega * t) / (-1.0 / rb.mass - t * t / rb.momentOfInertia);
                                        //f += rb.force.Y * secs;
                                        rb.velocity.Y -= f / rb.mass;
                                        rb.omega -= f * t / rb.momentOfInertia;
                                    }
                                }
                                //System.Diagnostics.Trace.WriteLine($"position: {rb.position}, \tvelocity: {rb.velocity}, \ttheta: {rb.theta}, \tomega: {rb.omega}");
                            }
                            if (needRestore()) return false;
                            else
                            {
                                reportReactionForce(() => SuspensionHardness * (Position - DesiredPosition));
                                return true;
                            }
                        })) return true;
                        else
                        {
                            RB.velocity -= velocityChange;
                            return false;
                        }
                    }
                    protected override Model3D CreateModel(params object[] vs)
                    {
                        SubTransforms.Add(new MatrixTransform3D());
                        SubTransforms.Add(new MatrixTransform3D());
                        MyLib.AssertTypes(vs, typeof(Point3D),typeof(double),typeof(double),typeof(IsCollidableDelegate));
                        RB = new RigidBody();
                        RB.position = DesiredPosition = (Point3D)vs[0];
                        Radius = (double)vs[1];
                        SuspensionHardness = (double)vs[2];
                        IsCollidable = (IsCollidableDelegate)vs[3];
                        const int n = 10;
                        List<Point3D> positions = new List<Point3D>();
                        for (int i = 0; i < n; i++) positions.Add(new Point3D(Radius * Math.Cos(2 * Math.PI * i / n), Radius * Math.Sin(2 * Math.PI * i / n), 0));
                        List<int> triangleIndices = new List<int>();
                        for (int i = 2; i < n; i++) triangleIndices.AddRange(new[] { 0, i - 1, i });
                        triangleIndices = triangleIndices.Concat(triangleIndices.Reverse<int>()).ToList();
                        return My3DGraphics.NewModel().AddTriangles(positions, triangleIndices).CreateModel(new SolidColorBrush(Colors.Gray));
                    }
                }
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
