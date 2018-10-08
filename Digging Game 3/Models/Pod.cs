﻿using System.Windows.Media.Media3D;
using System.Diagnostics;
using System.IO;
using System;
using System.Windows.Media;
using System.Linq;

namespace Digging_Game_3.Models
{
    partial class Pod :My3DObject
    {
        const int TransformIndexRotateAroundZ = 0;
        const int TransformIndexRotateAroundY = 1;
        Propeller propeller;
        Body body;
        Drill drill;
        Tracks tracks;
        protected override Model3D CreateModel(params object[] vs)
        {
            propeller = new Propeller(Propeller.Types.Basic);
            propeller.Transform=propeller.OriginTransform = MyLib.Transform(propeller).TranslatePrepend(new Vector3D(0,1.5,0)).RotatePrepend(new Vector3D(1, 0, 0), MyLib.ToRad(-90)).Value;
            body = new Body();
            drill = new Drill(1.5, 50);
            drill.Transform = drill.OriginTransform = MyLib.Transform(drill).TranslatePrepend(new Vector3D(1.5, 0, 0)).RotatePrepend(new Vector3D(0, 1, 0), MyLib.ToRad(90)).Value;
            tracks = new Tracks(this, new Vector3D(0, -1.5, 0));
            var ans = new Model3DGroup();
            ans.Children.Add(propeller.Model);
            ans.Children.Add(body.Model);
            ans.Children.Add(drill.Model);
            ans.Children.Add(tracks.Model);
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
            RB.alpha = Math.Pow(Math.Abs(t),1)* t;
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
            if(Keyboard.IsDown(System.Windows.Input.Key.A, System.Windows.Input.Key.D))
            {
                DesiredRotationY = Math.PI / 2;
                if (Keyboard.IsDown(System.Windows.Input.Key.A)) DesiredRotationY += Math.PI / 2;
                if (Keyboard.IsDown(System.Windows.Input.Key.D)) DesiredRotationY -= Math.PI / 2;
            }
            MyLib.SmoothTo(ref RotationY, DesiredRotationY, secs, Math.Max(Math.Abs(DesiredRotationY - RotationY) / Math.PI, 0.2) * 0.2);
            MyLib.Set(SubTransforms, TransformIndexRotateAroundY, true).RotatePrepend(new Vector3D(0, -1, 0), RotationY).Done();
        }
        bool _IsCollideDown = false;
        void MaintainRigitBody(double secs)
        {
            RB.force = new Vector3D(0, -RB.mass * Constants.Gravity, 0);
            RB.force += new Vector3D(-Math.Sin(RB.theta) * propeller.LiftForce(), Math.Cos(RB.theta) * propeller.LiftForce(), 0);
            if(!RB.Update(secs,rb=>
            {
                var dif = (rb.position - rb._position).Length;
                if (dif > 0.5) return false;
                int cur_x, cur_y, x = 0, y = 0;
                Blocks.IsCollidable(rb.position, out cur_x, out cur_y);
                if (secs < 1e-4)//accurate enough
                {
                    for (int cpi = -1; ;)
                    {
                        cpi = body.CollidePoints.FindIndex(cpi + 1, p => Blocks.IsCollidable(rb.position + new Vector3D(Math.Cos(rb.theta) * p.X - Math.Sin(rb.theta) * p.Y, Math.Cos(rb.theta) * p.Y + Math.Sin(rb.theta) * p.X, p.Z), out x, out y));
                        if (cpi == -1)
                        {
                            {
                                if (!tracks.UpdateRigidBody(secs,Math.Cos( RotationY),  RB.velocity - RB._velocity,RB.theta, out Vector3D reactionForce, out double reactionTorque)) return false;
                                //RB.force += reactionForce;
                                //RB.alpha += reactionTorque;
                            }
                            return true;
                        }
                        var cp = body.CollidePoints[cpi];
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
                            _IsCollideDown = true;
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
                if (body.CollidePoints.Any(p => Blocks.IsCollidable(ToWorldPoint2D(new Point3D() + p), out x, out y))) return false;
                {
                    if (!tracks.UpdateRigidBody(secs, Math.Cos(RotationY), RB.velocity - RB._velocity, RB.theta, out Vector3D reactionForce, out double reactionTorque)) return false;
                    //RB.force += reactionForce;
                    //RB.alpha += reactionTorque;
                }
                return true;
            }))
            {
                MaintainRigitBody(0.5 * secs);
                MaintainRigitBody(0.5 * secs);
            }
        }
        RigidBody RB = new RigidBody();
        Point3D ToWorldPoint2D(Point3D localPoint) { return RB.position + new Vector3D(Math.Cos(RB.theta) * localPoint.X - Math.Sin(RB.theta) * localPoint.Y, Math.Cos(RB.theta) * localPoint.Y + Math.Sin(RB.theta) * localPoint.X, localPoint.Z); }
        Point3D ToWorldPoint3D(Point3D localPoint)
        {
            var ans = localPoint;
            ans = new Point3D() + new Vector3D(Math.Cos(RotationY) * ans.X - Math.Sin(RotationY) * ans.Z, ans.Y, Math.Cos(RotationY) * ans.Z + Math.Sin(RotationY) * ans.X);
            ans = ToWorldPoint2D(ans);
            if (Keyboard.IsDown(System.Windows.Input.Key.Z)) Trace.WriteLine($"local: {localPoint}, \tglobal: {ans}");
            return ans;
        }
        public Pod():base()
        {
            int isOnGround = 0;
            propeller.IsOnGround = () => isOnGround > 0 && Math.Abs(RB.velocity.Y) < 0.01;
            Kernel.Heart.Beat += (secs) =>
            {
                MaintainRotationY(secs);
                MaintainRotationZ(secs);
                //drill.Folding = System.Math.Abs(System.DateTime.Now.Ticks % 100000000 - 50000000) / 50000000.0;
                _IsCollideDown = false;
                MaintainRigitBody(secs);
                {
                    if (isOnGround > 0) isOnGround--;
                    if (_IsCollideDown) isOnGround = 5;
                }
                OriginTransform = MyLib.Transform(new MatrixTransform3D()).Translate(RB.position - new Point3D()).Value;
                const double lookOffset = 1.3;
                MyLib.SmoothTo(ref Kernel.CameraProperties.position, RB.position + new Vector3D(0, 0, 30 / Math.Pow(0.4+SetZ(RB.position-Kernel.CameraProperties.position,0).Length*0.1,0.5)), secs, 0.2);
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
                    var speed = new Vector3D(2 * -Math.Cos(RotationY)+MyLib.Rand.NextDouble()-0.5, 5 + MyLib.Rand.NextDouble() - 0.5, 0);
                    Fumes.Instance.AddFums(position, speed
                        , Color.FromArgb(50, color, color, color)
                        , 0.1, 2, 2, speed.Length*1.5);
                }
            };
        }
        Vector3D RestrictX(Vector3D v, double minX, double maxX) { if (v.X < minX) v.X = minX; if (v.X > maxX) v.X = maxX; return v; }
        Vector3D RestrictY(Vector3D v, double minY, double maxY) { if (v.Y < minY) v.Y = minY; if (v.Y > maxY) v.Y = maxY; return v; }
        Vector3D SetZ(Vector3D v,double z) { v.Z = z;return v; }
        Vector3D RestrictXY(Vector3D v, double minX, double maxX, double minY, double maxY) { return RestrictY(RestrictX(v, minX, maxX), minY, maxY); }
        Vector3D NormalizedByZ(Vector3D v) { return v / Math.Abs(v.Z); }
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
