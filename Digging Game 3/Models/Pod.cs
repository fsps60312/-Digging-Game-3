using System.Windows.Media.Media3D;
using System.Diagnostics;
using System.IO;
using System;
using System.Windows.Media;

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
            tracks = new Tracks();
            tracks.Transform = tracks.OriginTransform = MyLib.Transform(tracks).TranslatePrepend(new Vector3D(0, -1.5, 0)).Value;
            var ans = new Model3DGroup();
            ans.Children.Add(propeller.Model);
            ans.Children.Add(body.Model);
            ans.Children.Add(drill.Model);
            ans.Children.Add(tracks.Model);
            propeller.DownwardWindSpeed = () => { return Vector3D.DotProduct(RB.velocity, new Vector3D(Math.Sin(RotationZ), Math.Cos(RotationZ), 0)); };
            this.SubTransforms.Add(new MatrixTransform3D());
            this.SubTransforms.Add(new MatrixTransform3D());
            //AddSphere(ans);
            return ans;
        }
        double RotationY = 0, RotationZ = 0;
        double DesiredRotationY = 0;
        void MaintainRotationZ(double secs)
        {
            if (!Keyboard.IsDown(System.Windows.Input.Key.A, System.Windows.Input.Key.D))
            {
                MyLib.SmoothTo(ref RotationZ, 0, secs, 0.5);
            }
            if (Keyboard.IsDown(System.Windows.Input.Key.A))
            {
                RotationZ -= secs * Math.PI * 0.3;
            }
            if (Keyboard.IsDown(System.Windows.Input.Key.D))
            {
                RotationZ += secs * Math.PI * 0.3;
            }
            MyLib.Set(SubTransforms, TransformIndexRotateAroundZ, true).RotatePrepend(new Vector3D(0, 0, -1), RotationZ).Done();
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
        RigidBody RB = new RigidBody();
        public Pod():base()
        {
            Kernel.Heart.Beat += (secs) =>
            {
                MaintainRotationY(secs);
                MaintainRotationZ(secs);
                //drill.Folding = System.Math.Abs(System.DateTime.Now.Ticks % 100000000 - 50000000) / 50000000.0;
                RB.force = new Vector3D(0, -RB.mass * Constants.Gravity, 0);
                RB.force += new Vector3D(Math.Sin(RotationZ) * propeller.LiftForce(), Math.Cos(RotationZ) * propeller.LiftForce(), 0);
                RB.Update(secs);
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
