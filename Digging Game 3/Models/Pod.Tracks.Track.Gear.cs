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
                    const int TransformIndexRotateY = 1;
                    const int TransformIndexSpin = 2;
                    Body Parent;
                    public Point3D Position{get { return RB.position; }}
                    public Point3D RelativePosition { get; private set; }
                    public Point3D DesiredPosition { get { return RelativePosition * Parent.MatrixY * Parent.MatrixZ * Parent.MatrixT; } }
                    public double Radius { get; private set; }
                    public RigidBody RB { get; private set; }
                    public double SuspensionHardness { get; private set; }
                    public double Mass { get; private set; }
                    public Gear(Point3D desiredPosition,double radius,double suspensionHardness,double mass, Body parent) : base(desiredPosition,radius, suspensionHardness,mass,parent)
                    {
                        var preMatrixY = Parent.MatrixY;
                        var preMatrixZ = Parent.MatrixZ;
                        Parent.RigidBodyUpdating += (secs,rb) =>
                        {
                            var parentVelocity = rb.GetVelocityAt(RelativePosition * Parent.MatrixY);
                            var f1 = ReactForce;
                            var f2 = -0.5 * (parentVelocity - RB.velocity);
                            //if (Math.Abs((new Vector3D(1, 0, 0) * Parent.MatrixY).Z) > 0.01)
                            //{
                            //    //RB.velocity = parentVelocity;
                            //    f1 *= 2;
                            //    f2 *= 2;
                            //}
                            var f = f1 + f2;
                            rb.force += f;
                            onGround -= secs;
                            if (onGround < 0) onGround = 0;
                            RB.force = new Vector3D(0, -RB.mass * Constants.Gravity, 0);
                            RB.force -= f;
                            {
                                var friction = (new Vector3D(Parent.TrackSpeed, 0, 0) * Parent.MatrixY-RB.velocity).X * 0.5 * Math.Max(0, ReactForce.Y);
                                double maxFriction = SuspensionHardness * 0.1;
                                if (friction > maxFriction) friction = maxFriction;
                                if (friction < -maxFriction) friction = -maxFriction;
                                RB.force.X += friction;
                            }
                            RB.theta += secs * Parent.TrackSpeed / Radius;
                            {
                                var newPosition = RB.position * MyLib.Inverse(preMatrixY * Parent.MatrixZ * Parent.MatrixT) * Parent.MatrixY * Parent.MatrixZ * Parent.MatrixT;
                                var newVelocity = RB.velocity * MyLib.Inverse(preMatrixY * Parent.MatrixZ * Parent.MatrixT) * Parent.MatrixY * Parent.MatrixZ * Parent.MatrixT;
                                RB.position.X = newPosition.X; RB.position.Z = newPosition.Z;
                                //RB.velocity.X = newVelocity.X; RB.velocity.Y = newVelocity.Y;
                                preMatrixY = Parent.MatrixY;
                                preMatrixZ = Parent.MatrixZ;
                            }
                            UpdateRigitBody(secs, parentVelocity);
                            ///minimize: (px+a*fx)^2+(py+a*fy)^2
                            ///2(px+a*fx)*fx+2(py+a*fy)*fy=0
                            ///px*fx+a*fx*fx+py*fy+a*fy*fy=0
                            ///a=-(px*fx+py*fy)/(fx*fx+fy*fy)
                            if (f.Length > 0)
                            {
                                var p = DesiredPosition - rb.position;
                                var a = -(p.X * f.X + p.Y * f.Y) / (f.X * f.X + f.Y * f.Y);
                                var forceArm = p + f * a;
                                var torque = Vector3D.CrossProduct(forceArm, f).Z;
                                //System.Diagnostics.Trace.WriteLine($"torque: {torque}");
                                rb.alpha += torque;
                            }
                            MyLib.Set(SubTransforms, TransformIndexPosition, true).TranslatePrepend(Position - new Point3D()).Done();
                            SubTransforms[TransformIndexRotateY] = new MatrixTransform3D(Parent.MatrixY);
                            MyLib.Set(SubTransforms, TransformIndexSpin, true).RotatePrepend(new Vector3D(0,0,-1), RB.theta).Done();
                            UpdateTransform();
                        };
                    }
                    Vector3D ReactForce { get { return SuspensionHardness * (Position - DesiredPosition); } }
                    double onGround = 0;
                    public bool IsOnGround { get { return onGround > 0; } }
                    void UpdateRigitBody(double secs,Vector3D parentVelocity)
                    {
                        if (!RB.Update(secs, rb =>
                        {
                            if (secs > 1e-3) return false;
                            var dif = (rb.position - rb._position).Length;
                            if (dif > Radius / 5) return false;
                            if(!Blocks.IsCollidable(rb.position, out int cur_x, out int cur_y))
                            {
                                //bool rollback = false;
                                const double rollbackSpeed = 0.1;
                                const double bounce = 0.5;
                                {
                                    int x = 0, y = 0;
                                    Point3D cp;
                                    cp = rb.position + new Vector3D(-Radius, 0, 0);
                                    if (Blocks.IsCollidable(cp, out x, out y))
                                    {
                                        if (x == cur_x - 1 && !Blocks.IsCollidable(cur_x, y) && rb.velocity.X < 0)//collide left, +x force. t=(cp.x*-Sin(theta)+cp.y*-Cos(theta)), (v.x+f/m) + (omega+f*t/I)*t = -b*(v.x + omega*t), f*(1/m+t^2/I)=(-b-1)*(v.x+omega*t)
                                        {
                                            var f = (-bounce - 1) * (rb.velocity.X) / (1.0 / rb.mass);
                                            rb.velocity.X += f / rb.mass;
                                            //rollback = true;
                                            rb.position.X += secs * rollbackSpeed;
                                        }
                                    }
                                    cp = rb.position + new Vector3D(Radius, 0, 0);
                                    if (Blocks.IsCollidable(cp, out x, out y))
                                    {
                                        if (x == cur_x + 1 && !Blocks.IsCollidable(cur_x, y) && rb.velocity.X > 0)//collide right, -x force. t=(cp.x*-Sin(theta)+cp.y*-Cos(theta)), (v.x-f/m) + (omega-f*t/I)*t = -b*(v.x + omega*t), f*(-1/m-t^2/I)=(-b-1)*(v.x+omega*t)
                                        {
                                            var f = (-bounce - 1) * (rb.velocity.X) / (-1.0 / rb.mass);
                                            rb.velocity.X -= f / rb.mass;
                                            //rollback = true;
                                            rb.position.X -= secs * rollbackSpeed;
                                        }
                                    }
                                    cp = rb.position + new Vector3D(0, -Radius, 0);
                                    if (Blocks.IsCollidable(cp, out x, out y))
                                    {
                                        if (y == cur_y - 1 && !Blocks.IsCollidable(x, cur_y) && rb.velocity.Y < 0)//collide down, +y force. t=(cp.y*-Sin(theta)+cp.x*Cos(theta)), (v.y+f/m) + (omega+f*t/I)*t = -b*(v.y + omega*t), f*(1/m+t^2/I)=(-b-1)*(v.y+omega*t)
                                        {
                                            var f = (-bounce - 1) * (rb.velocity.Y) / (1.0 / rb.mass);
                                            rb.velocity.Y += f / rb.mass;
                                            //rollback = true;
                                            rb.position.Y += secs * rollbackSpeed;
                                            onGround = 0.1;
                                        }
                                    }
                                    cp = rb.position + new Vector3D(0, Radius, 0);
                                    if (Blocks.IsCollidable(cp, out x, out y))
                                    {
                                        if (y == cur_y + 1 && !Blocks.IsCollidable(x, cur_y) && rb.velocity.Y > 0)//collide up, -y force. t=(cp.y*-Sin(theta)+cp.x*Cos(theta)), (v.y-f/m) + (omega-f*t/I)*t = -b*(v.y + omega*t), f*(-1/m-t^2/I)=(-b-1)*(v.y+omega*t)
                                        {
                                            var f = (-bounce - 1) * (rb.velocity.Y) / (-1.0 / rb.mass);
                                            rb.velocity.Y -= f / rb.mass;
                                            //rollback = true;
                                            rb.position.Y -= secs * rollbackSpeed;
                                        }
                                    }
                                }
                                //if(false)
                                {
                                    Blocks.IsCollidable(rb.position + new Vector3D(Blocks.Width / 2, Blocks.Height/2, 0), out int cross_x, out int cross_y);
                                    var vectorToCross = new Vector3D((Blocks.Anchor.X + Blocks.Width * cross_x) - rb.position.X, (Blocks.Anchor.Y + Blocks.Height * cross_y) - rb.position.Y, 0);
                                    if (0<vectorToCross.Length&&vectorToCross.Length < Radius && Vector3D.DotProduct(rb.velocity, vectorToCross) > 0)
                                    {
                                        int x = vectorToCross.X < 0 ? cross_x - 1 : cross_x;
                                        int y = vectorToCross.Y < 0 ? cross_y - 1 : cross_y;
                                        if (Blocks.IsCollidable(x, y))
                                        {
                                            ///(v+a*c)。c=-b*(v。c)
                                            ///(vx+a*cx)*cx+(vy+a*cy)*cy=-b*(vx*cx+vy*cy)
                                            ///a(cx*cx+cy*cy)+vx*cx+vy*cy=-b*(vx*cx+vy*cy)
                                            ///a=(-b*(vx*cx+vy*cy)-(vx*cx+vy*cy))/(cx*cx+cy*cy)
                                            double a = ((-bounce - 1) * Vector3D.DotProduct(rb.velocity, vectorToCross)) / Vector3D.DotProduct(vectorToCross, vectorToCross);
                                            rb.velocity += a * vectorToCross;
                                            //rollback = true;
                                            rb.position += -vectorToCross / vectorToCross.Length * rollbackSpeed;
                                        }
                                    }
                                }
                                //if (rollback) rb.position = rb._position;
                                //System.Diagnostics.Trace.WriteLine($"position: {rb.position}, \tvelocity: {rb.velocity}, \ttheta: {rb.theta}, \tomega: {rb.omega}");
                            }
                            return true;
                        }))
                        {
                            UpdateRigitBody(0.5 * secs,parentVelocity);
                            UpdateRigitBody(0.5 * secs,parentVelocity);
                        };
                    }
                    protected override Model3D CreateModel(params object[] vs)
                    {
                        SubTransforms.Add(new MatrixTransform3D());
                        SubTransforms.Add(new MatrixTransform3D());
                        SubTransforms.Add(new MatrixTransform3D());
                        MyLib.AssertTypes(vs, typeof(Point3D),typeof(double),typeof(double),typeof(double),typeof(Body));
                        RelativePosition = (Point3D)vs[0];
                        Radius = (double)vs[1];
                        SuspensionHardness = (double)vs[2];
                        Mass = (double)vs[3];
                        Parent = vs[4] as Body;
                        RB = new RigidBody { mass = Mass };
                        RB.position = DesiredPosition;
                        const int n = 10;
                        List<Point3D> positions = new List<Point3D>();
                        for (int i = 0; i < n; i++) positions.Add(new Point3D(Radius * Math.Cos(2 * Math.PI * i / n), Radius * Math.Sin(2 * Math.PI * i / n), 0));
                        List<int> triangleIndices = new List<int>();
                        var ans = new Model3DGroup();
                        for (int i = 2; i < n; i++)
                        {
                            var color = Color.FromArgb(255, (byte)MyLib.Rand.Next(110, 150), (byte)MyLib.Rand.Next(110, 150), (byte)MyLib.Rand.Next(110, 150));
                            ans.Children.Add(My3DGraphics.NewModel().AddTriangles(new[] { positions[0], positions[i - 1], positions[i] }, new[] { 0, 1, 2, 2, 1, 0 }).CreateModel(new SolidColorBrush(color)));
                            triangleIndices.AddRange(new[] { 0, i - 1, i, i, i - 1, 0 });
                        }
                        return ans;
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
