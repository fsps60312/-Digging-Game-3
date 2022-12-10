using System;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Collections.Generic;

namespace Digging_Game_3.Models
{
    partial class Pod
    {
        /// <summary>
        /// A Drill point to Vector3D(0,0,1)
        /// </summary>
        partial class Drill : My3DObject
        {
            const int TransformIndexWithdraw = 0;
            const int TransformIndexRotate = 1;
            public double Radius { get; private set; }
            public double ConeAngle { get; private set; }
            public Drill(double radius, double bladeCount) : base(radius, bladeCount)
            {
                SubTransforms.Add(new MatrixTransform3D());//Withdraw
                SubTransforms.Add(new MatrixTransform3D());//Rotate
                double accu_sec = 0;
                Kernel.Heart.Beat1 += (secs) =>
                {
                    MyLib.Set(SubTransforms, TransformIndexRotate).RotatePrepend(new Vector3D(0, 0, 1), secs * Math.PI).Done();
                    accu_sec += secs;
                    accu_sec %= 1;
                    Folding = 1-accu_sec;
                    UpdateTransform();
                };
            }
            double _Folding;
            public double Folding
            {
                get { return _Folding; }
                set
                {
                    const double ratio = 0.3;
                    if (value < ratio)
                    {
                        double f = value / ratio;
                        foreach (var v in blades) v.Folding = f;
                        MyLib.Set(SubTransforms, TransformIndexWithdraw, true).Done();
                    }
                    else
                    {
                        double f = (value - ratio) / (1 - ratio);
                        foreach (var v in blades) v.Folding = 1;
                        MyLib.Set(SubTransforms, TransformIndexWithdraw, true).TranslatePrepend(new Vector3D(0, 0, -f * Radius / Math.Cos(ConeAngle))).Done();
                    }
                    _Folding = value;
                    UpdateTransform();
                }
            }
            List<Blade> blades;
            protected override Model3D CreateModel(params object[] vs)
            {
                MyLib.AssertTypes(vs, typeof(double), typeof(double));
                double r = Radius = (double)vs[0];
                double n = (double)vs[1];
                double coneAngle = ConeAngle = MyLib.ToRad(50);
                blades = new List<Blade>();
                Model3DGroup ans = new Model3DGroup();
                for (int i = 0; i < n; i++)
                {
                    double a = Math.PI * 2 * i / n;
                    var blade = new Blade(r, coneAngle, MyLib.ToRad(40), new[] { Colors.DarkGray, Colors.DarkSlateGray, Colors.DimGray, Colors.Gray, Colors.LightGray, Colors.LightSlateGray, Colors.SlateGray }[MyLib.Rand.Next(7)]);
                    blade.Transform = MyLib.Transform(new MatrixTransform3D()).Rotate(new Vector3D(0, 0, 1), a)/*.Translate(new Vector3D(0, 0, r * Math.Tan(coneAngle)))*/.Value;
                    blades.Add(blade);
                    ans.Children.Add(blade.Model);
                }
                return ans;
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
