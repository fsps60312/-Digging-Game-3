using System.Windows.Media.Media3D;
using System.Diagnostics;
using System.IO;

namespace Digging_Game_3.Models
{
    partial class Pod :My3DObject
    {
        Propeller propeller;
        Body body;
        Drill drill;
        protected override Model3D CreateModel(params object[] vs)
        {
            propeller = new Propeller(Propeller.Types.Basic);
            propeller.Transform=propeller.OriginTransform = MyLib.Transform(propeller).TranslatePrepend(new Vector3D(0,1.5,0)).RotatePrepend(new Vector3D(1, 0, 0), MyLib.ToRad(-90)).Value;
            body = new Body();
            drill = new Drill(1.5, 50);
            drill.Transform = drill.OriginTransform = MyLib.Transform(drill).TranslatePrepend(new Vector3D(1.5, 0, 0)).RotatePrepend(new Vector3D(0, 1, 0), MyLib.ToRad(90)).Value;
            var ans = new Model3DGroup();
            ans.Children.Add(propeller.Model);
            ans.Children.Add(body.Model);
            ans.Children.Add(drill.Model);
            //AddSphere(ans);
            return ans;
        }
        RigidBody RB = new RigidBody { fraction = 5 };
        public Pod():base()
        {
            Kernel.Heart.Beat += (secs) =>
            {
                propeller.Folding = drill.Folding = System.Math.Abs(System.DateTime.Now.Ticks % 100000000 - 50000000) / 50000000.0;
                RB.force = new Vector3D();
                const double f = 50;
                if (Keyboard.IsDown(System.Windows.Input.Key.A)) RB.force -= new Vector3D(f, 0, 0);
                if (Keyboard.IsDown(System.Windows.Input.Key.D)) RB.force += new Vector3D(f, 0, 0);
                if (Keyboard.IsDown(System.Windows.Input.Key.W)) RB.force += new Vector3D(0, f, 0);
                if (Keyboard.IsDown(System.Windows.Input.Key.S)) RB.force -= new Vector3D(0, f, 0);
                RB.Update(secs);
                Model.Transform = MyLib.Transform(new MatrixTransform3D()).Translate(RB.position - new Point3D()).Value;
                MyLib.SmoothTo(ref Kernel.CameraProperties.position, RB.position + new Vector3D(0, 0, 20),secs,0.5);
                MyLib.SmoothTo(ref Kernel.CameraProperties.lookDirection, RB.position - Kernel.CameraProperties.position, secs, 0.5);
                //this.Folding = 1;
            };
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
