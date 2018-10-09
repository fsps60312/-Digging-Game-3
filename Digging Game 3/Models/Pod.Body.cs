using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Digging_Game_3.Models
{
    partial class Pod
    {
        public class Body : My3DObject
        {
            const double BodyRadius = 1.5;
            const int TransformIndexRotateAroundZ = 0;
            const int TransformIndexRotateAroundY = 1;
            Propeller propeller;
            Drill drill;
            public List<Vector3D> CollidePoints { get; private set; } = new List<Vector3D>
            {new Vector3D(-BodyRadius,-BodyRadius,0),new Vector3D(-BodyRadius,BodyRadius,0),new Vector3D(BodyRadius,-BodyRadius,0),new Vector3D(BodyRadius,BodyRadius,0) };
            public Func<bool> IsOnGround = () => false;
            public double TrackSpeed = 0;
            public double MaxTrackAcceleration = 10;
            public double MaxTrackPower = 50;
            public double TrackFriction = 2;
            public Body():base()
            {
                propeller.IsOnGround = () => IsOnGround();
                Kernel.Heart.Beat += (secs) =>
                {
                    //if (IsOnGround()) System.Diagnostics.Trace.WriteLine("A");
                    MaintainRotationY(secs);
                    MaintainRotationZ(secs);
                    //drill.Folding = System.Math.Abs(System.DateTime.Now.Ticks % 100000000 - 50000000) / 50000000.0;
                    {
                        RB.position.Z = RB.velocity.Z = 0;
                        RB.force = new Vector3D();
                        RigidBodyUpdating?.Invoke(secs, RB);
                        RB.force += new Vector3D(0, -RB.mass * Constants.Gravity, 0);
                        RB.force += new Vector3D(-Math.Sin(RB.theta) * propeller.LiftForce(), Math.Cos(RB.theta) * propeller.LiftForce(), 0);
                        MaintainRigidBody(secs);
                    }
                    {
                        double frictionAcceleration = (TrackSpeed > 0 ? -1 : 1) * (TrackFriction /*+ 0.5 * Math.Abs(TrackSpeed)*/);
                        double acceleration = 0;
                        if (IsOnGround() && Keyboard.IsDown(System.Windows.Input.Key.A, System.Windows.Input.Key.D))
                        {
                            if (!Keyboard.IsDown(RotationY > Math.PI / 2 ? System.Windows.Input.Key.D : System.Windows.Input.Key.A))
                            {
                                acceleration += Math.Min(MaxTrackAcceleration, MaxTrackPower / Math.Max(Math.Abs(TrackSpeed), double.MinValue));
                            }
                            else
                            {
                                frictionAcceleration += (TrackSpeed > 0 ? -1 : 1) * MaxTrackAcceleration;
                            }
                        }
                        TrackSpeed += acceleration * secs;
                        if ((TrackSpeed > 0) != (TrackSpeed + frictionAcceleration * secs > 0)) TrackSpeed = 0;
                        else TrackSpeed += frictionAcceleration * secs;
                    }
                    OriginTransform = MyLib.Transform(new MatrixTransform3D()).Translate(RB.position - new Point3D()).Value;
                    const double lookOffset = 1.3;
                    MyLib.SmoothTo(ref Kernel.CameraProperties.position, RB.position + new Vector3D(0, 0, 30 / Math.Pow(0.4 + SetZ(RB.position - Kernel.CameraProperties.position, 0).Length * 0.1, 0.5)), secs, 0.2);
                    var target = RB.position + 0.1 * RB.velocity - Kernel.CameraProperties.position;
                    target /= Math.Abs(target.Z);
                    var len = Math.Sqrt(Math.Pow(target.X, 2) + Math.Pow(target.Y, 2));
                    var targetLen = Math.Min(len, lookOffset);
                    target.X *= targetLen / len;
                    target.Y *= targetLen / len;
                    MyLib.SmoothTo(ref Kernel.CameraProperties.lookDirection, target, secs, 0.2);
                    UpdateTransform();
                    //this.Folding = 1;
                    //for(int i=0;i<1;i++)
                    {
                        var color = (byte)MyLib.Rand.Next(200, 210);
                        var position = RB.position + new Vector3D(-Math.Cos(RotationY) * 1, 1, MyLib.Rand.NextDouble() / 2 + 0.5);
                        var speed = new Vector3D(2 * -Math.Cos(RotationY) + MyLib.Rand.NextDouble() - 0.5, 5 + MyLib.Rand.NextDouble() - 0.5, 0);
                        Fumes.Instance.AddFums(position, speed
                            , Color.FromArgb(50, color, color, color)
                            , 0.1, 2, 2, speed.Length * 1.5);
                    }
                };
            }
            protected override Model3D CreateModel(params object[] vs)
            {
                propeller = new Propeller(Propeller.Types.Basic);
                propeller.Transform = propeller.OriginTransform = MyLib.Transform(propeller).TranslatePrepend(new Vector3D(0, 1.5, 0)).RotatePrepend(new Vector3D(1, 0, 0), MyLib.ToRad(-90)).Value;
                drill = new Drill(1.5, 50);
                drill.Transform = drill.OriginTransform = MyLib.Transform(drill).TranslatePrepend(new Vector3D(1.5, 0, 0)).RotatePrepend(new Vector3D(0, 1, 0), MyLib.ToRad(90)).Value;
                var ans = new Model3DGroup();
                ans.Children.Add(CreateBodyModel());
                ans.Children.Add(propeller.Model);
                ans.Children.Add(drill.Model);
                propeller.DownwardWindSpeed = () => { return Vector3D.DotProduct(RB.velocity, new Vector3D(-Math.Sin(RB.theta), Math.Cos(RB.theta), 0)); };
                this.SubTransforms.Add(new MatrixTransform3D());
                this.SubTransforms.Add(new MatrixTransform3D());
                //AddSphere(ans);
                return ans;
            }
            double RotationY = 0;
            double DesiredRotationY = 0;
            void MaintainRotationZ(double secs)
            {
                var t = -10 * RB.theta - RB.omega;
                RB.alpha = Math.Pow(Math.Abs(t), 1) * t;
                if (Keyboard.IsDown(System.Windows.Input.Key.A, System.Windows.Input.Key.D))
                {
                    if (Keyboard.IsDown(System.Windows.Input.Key.A))
                    {
                        RB.alpha += 5;
                    }
                    if (Keyboard.IsDown(System.Windows.Input.Key.D))
                    {
                        RB.alpha -= 5;
                    }
                    //if (Keyboard.IsDown(System.Windows.Input.Key.K)) RB.omega += RB.alpha * 0.1;
                }
                //else RB.alpha = -1 * (RB.theta - 1 * RB.omega);
                RB.alpha += -1 * Math.Abs(RB.omega) * RB.omega - 1 * RB.omega;
                MyLib.Set(SubTransforms, TransformIndexRotateAroundZ, true).RotatePrepend(new Vector3D(0, 0, 1), RB.theta).Done();
            }
            void MaintainRotationY(double secs)
            {
                if ((!IsOnGround() || TrackSpeed == 0) && Keyboard.IsDown(System.Windows.Input.Key.A, System.Windows.Input.Key.D))
                {
                    //DesiredRotationY = Math.PI / 2;
                    if (Keyboard.IsDown(System.Windows.Input.Key.A)&&!Keyboard.IsDown(System.Windows.Input.Key.D)) DesiredRotationY = Math.PI;
                    if (Keyboard.IsDown(System.Windows.Input.Key.D)&&!Keyboard.IsDown(System.Windows.Input.Key.A)) DesiredRotationY = 0;
                }
                MyLib.SmoothTo(ref RotationY, DesiredRotationY, secs, Math.Max(Math.Abs(DesiredRotationY - RotationY) / Math.PI, 0.2) * 0.2);
                MyLib.Set(SubTransforms, TransformIndexRotateAroundY, true).RotatePrepend(new Vector3D(0, -1, 0), RotationY).Done();
            }
            public delegate void RigidBodyUpdatingEventHandler(double secs,RigidBody rb);
            public RigidBodyUpdatingEventHandler RigidBodyUpdating;
            void MaintainRigidBody(double secs)
            {
                if (!RB.Update(secs, rb =>
                {
                    if (secs > 1e-3) return false;
                    var dif = (rb.position - rb._position).Length;
                    if (dif > 0.5) return false;
                    int cur_x, cur_y, x = 0, y = 0;
                    Blocks.IsCollidable(rb.position, out cur_x, out cur_y);
                    if (secs < 1e-4)//accurate enough
                    {
                        for (int cpi = -1; ;)
                        {
                            cpi = this.CollidePoints.FindIndex(cpi + 1, p => Blocks.IsCollidable(rb.position + new Vector3D(Math.Cos(rb.theta) * p.X - Math.Sin(rb.theta) * p.Y, Math.Cos(rb.theta) * p.Y + Math.Sin(rb.theta) * p.X, p.Z), out x, out y));
                            if (cpi == -1)
                            {
                                //{
                                //    if (!tracks.UpdateRigidBody(secs, Math.Cos(RotationY), RB.velocity - RB._velocity, RB.theta, out Vector3D reactionForce, out double reactionTorque)) return false;
                                //    //RB.force += reactionForce;
                                //    //RB.alpha += reactionTorque;
                                //}
                                return true;
                            }
                            var cp = this.CollidePoints[cpi];
                            const double bounce = 0.5;
                            double t;
                            t = cp.X * -Math.Sin(rb.theta) + cp.Y * -Math.Cos(rb.theta);
                            if (x == cur_x - 1 && !Blocks.IsCollidable(cur_x, y) && rb.velocity.X + t * rb.omega < 0)//collide left, +x force. t=(cp.x*-Sin(theta)+cp.y*-Cos(theta)), (v.x+f/m) + (omega+f*t/I)*t = -b*(v.x + omega*t), f*(1/m+t^2/I)=(-b-1)*(v.x+omega*t)
                            {
                                var f = (-bounce - 1) * (rb.velocity.X + rb.omega * t) / (1.0 / rb.mass + t * t / rb.momentOfInertia);
                                //f += -rb.force.X * secs;
                                rb.velocity.X += f / rb.mass;
                                rb.omega += f * t / rb.momentOfInertia;
                            }
                            t = cp.X * -Math.Sin(rb.theta) + cp.Y * -Math.Cos(rb.theta);
                            if (x == cur_x + 1 && !Blocks.IsCollidable(cur_x, y) && rb.velocity.X + t * rb.omega > 0)//collide right, -x force. t=(cp.x*-Sin(theta)+cp.y*-Cos(theta)), (v.x-f/m) + (omega-f*t/I)*t = -b*(v.x + omega*t), f*(-1/m-t^2/I)=(-b-1)*(v.x+omega*t)
                            {
                                var f = (-bounce - 1) * (rb.velocity.X + rb.omega * t) / (-1.0 / rb.mass - t * t / rb.momentOfInertia);
                                //f += rb.force.X * secs;
                                rb.velocity.X -= f / rb.mass;
                                rb.omega -= f * t / rb.momentOfInertia;
                            }
                            t = cp.Y * -Math.Sin(rb.theta) + cp.X * Math.Cos(rb.theta);
                            if (y == cur_y - 1 && !Blocks.IsCollidable(x, cur_y) && rb.velocity.Y + t * rb.omega < 0)//collide down, +y force. t=(cp.y*-Sin(theta)+cp.x*Cos(theta)), (v.y+f/m) + (omega+f*t/I)*t = -b*(v.y + omega*t), f*(1/m+t^2/I)=(-b-1)*(v.y+omega*t)
                            {
                                var f = (-bounce - 1) * (rb.velocity.Y + rb.omega * t) / (1.0 / rb.mass + t * t / rb.momentOfInertia);
                                //f -= rb.force.Y * secs;
                                rb.velocity.Y += f / rb.mass;
                                rb.omega += f * t / rb.momentOfInertia;
                            }
                            t = cp.Y * -Math.Sin(rb.theta) + cp.X * Math.Cos(rb.theta);
                            if (y == cur_y + 1 && !Blocks.IsCollidable(x, cur_y) && rb.velocity.Y + t * rb.omega > 0)//collide up, -y force. t=(cp.y*-Sin(theta)+cp.x*Cos(theta)), (v.y-f/m) + (omega-f*t/I)*t = -b*(v.y + omega*t), f*(-1/m-t^2/I)=(-b-1)*(v.y+omega*t)
                            {
                                var f = (-bounce - 1) * (rb.velocity.Y + rb.omega * t) / (-1.0 / rb.mass - t * t / rb.momentOfInertia);
                                //f += rb.force.Y * secs;
                                rb.velocity.Y -= f / rb.mass;
                                rb.omega -= f * t / rb.momentOfInertia;
                            }
                            //System.Diagnostics.Trace.WriteLine($"position: {rb.position}, \tvelocity: {rb.velocity}, \ttheta: {rb.theta}, \tomega: {rb.omega}");
                        }
                    }
                    if (this.CollidePoints.Any(p => Blocks.IsCollidable((new Point3D() + p) * MatrixTZ, out x, out y))) return false;
                    //{
                    //    if (!tracks.UpdateRigidBody(secs, Math.Cos(RotationY), RB.velocity - RB._velocity, RB.theta, out Vector3D reactionForce, out double reactionTorque)) return false;
                    //    //RB.force += reactionForce;
                    //    //RB.alpha += reactionTorque;
                    //}
                    return true;
                }))
                {
                    MaintainRigidBody(0.5 * secs);
                    MaintainRigidBody(0.5 * secs);
                }
            }
            RigidBody RB = new RigidBody { mass = 0.8 };
            public Matrix3D MatrixT
            {
                get
                {
                    var ans = Matrix3D.Identity;
                    ans.TranslatePrepend(RB.position - new Point3D());
                    return ans;
                }
            }
            public Matrix3D MatrixY
            {
                get
                {
                    var ans = Matrix3D.Identity;
                    ans.RotatePrepend(new Quaternion(new Vector3D(0, -1, 0), RotationY / Math.PI * 180));
                    return ans;
                }
            }
            public Matrix3D MatrixZ
            {
                get
                {
                    var ans = Matrix3D.Identity;
                    ans.RotatePrepend(new Quaternion(new Vector3D(0, 0, 1), RB.theta / Math.PI * 180));
                    return ans;
                }
            }
            Matrix3D MatrixTZ { get { return MatrixZ * MatrixT; } }
            Model3D CreateBodyModel()
            {
                //double r = 1.5;
                ///6 7
                ///4 5
                ///
                ///2 3
                ///0 1
                //var vertices = new List<Point3D>{, new Point3D(r, -r, -r), new Point3D(-r, r, -r), new Point3D(r, r, -r), new Point3D(-r, -r, r), new Point3D(r, -r, r), new Point3D(-r, r, r), new Point3D(r, r, r) };
                My3DGraphics.Cuboid.AddFaces(new Point3D(BodyRadius, BodyRadius, BodyRadius),out List<Point3D>vertices, out List<int> triangleIndices, out List<Vector3D> normals);
                return My3DGraphics.NewModel().AddTriangles(vertices,triangleIndices,normals).CreateModel(new SolidColorBrush(Colors.PeachPuff));
            }
            Vector3D RestrictX(Vector3D v, double minX, double maxX) { if (v.X < minX) v.X = minX; if (v.X > maxX) v.X = maxX; return v; }
            Vector3D RestrictY(Vector3D v, double minY, double maxY) { if (v.Y < minY) v.Y = minY; if (v.Y > maxY) v.Y = maxY; return v; }
            Vector3D SetZ(Vector3D v, double z) { v.Z = z; return v; }
            Vector3D RestrictXY(Vector3D v, double minX, double maxX, double minY, double maxY) { return RestrictY(RestrictX(v, minX, maxX), minY, maxY); }
            Vector3D NormalizedByZ(Vector3D v) { return v / Math.Abs(v.Z); }
        }
    }
}
