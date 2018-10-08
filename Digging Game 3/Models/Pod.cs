using System.Windows.Media.Media3D;
using System.Diagnostics;
using System.IO;
using System;
using System.Windows.Media;
using System.Linq;

namespace Digging_Game_3.Models
{
    partial class Pod :My3DObject
    {
        Body body;
        Tracks tracks;
        protected override Model3D CreateModel(params object[] vs)
        {
            body = new Body();
            tracks = new Tracks(body, new Vector3D(0, -1.5, 0));
            var ans = new Model3DGroup();
            ans.Children.Add(body.Model);
            ans.Children.Add(tracks.Model);
            return ans;
        }
        //Point3D ToWorldPoint3D(Point3D localPoint)
        //{
        //    var ans = localPoint;
        //    ans = new Point3D() + new Vector3D(Math.Cos(RotationY) * ans.X - Math.Sin(RotationY) * ans.Z, ans.Y, Math.Cos(RotationY) * ans.Z + Math.Sin(RotationY) * ans.X);
        //    ans = ToWorldPoint2D(ans);
        //    if (Keyboard.IsDown(System.Windows.Input.Key.Z)) Trace.WriteLine($"local: {localPoint}, \tglobal: {ans}");
        //    return ans;
        //}
        public Pod():base()
        {
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
