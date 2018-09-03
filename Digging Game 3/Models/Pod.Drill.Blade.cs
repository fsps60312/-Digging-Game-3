using System;
using System.Windows.Media.Media3D;
using System.Windows.Media;

namespace Digging_Game_3.Models
{
    partial class Pod
    {
        partial class Drill
        {
            class Blade : My3DObject
            {
                public Blade(double radius,double coneAngle,double attackAngle, Color color) : base(radius,coneAngle, attackAngle, color) { }
                double _Folding;
                public double Folding
                {
                    get { return _Folding; }
                    set
                    {
                        setFoldingAction(value);
                        _Folding = value;
                    }
                }
                Action<double> setFoldingAction = null;
                protected override Model3D CreateModel(params object[] vs)
                {
                    MyLib.AssertTypes(vs, typeof(double),typeof(double),typeof(double), typeof(Color));
                    double r = (double)vs[0];
                    double coneAngle = (double)vs[1];
                    double attackAngle = (double)vs[2];
                    var color = (Color)vs[3];
                    double R = r / Math.Cos(coneAngle);
                    var blade = My3DGraphics.NewModel().AddTriangles(new[] { new Point3D(0, 0, 0), new Point3D(R, 0, 0), new Point3D(R, R / 7, 0), new Point3D(0, R / 30, 0) }, new[] { 0, 1, 2, 0, 2, 3, 3, 2, 0, 2, 1, 0 }).CreateModel(new SolidColorBrush(color));
                    var bladeOriginTransform = blade.Transform;
                    double supportThickness = R / 100;
                    var support = My3DGraphics.NewModel().AddTriangles(
                        new[] { new Point3D(0, 0, -supportThickness), new Point3D(R / 2, 0, -supportThickness), new Point3D(0, supportThickness, -supportThickness), new Point3D(R / 2, supportThickness, -supportThickness), new Point3D(0, 0, 0), new Point3D(R / 2, 0, 0), new Point3D(0, supportThickness, 0), new Point3D(R / 2, supportThickness, 0) },
                        ///6 7
                        ///4 5
                        ///
                        ///2 3
                        ///0 1
                        new[]
                        {
                            0,2,3,0,3,1,
                            4,5,7,4,7,6,
                            0,1,5,0,5,4,
                            2,6,7,2,7,3,
                            0,4,6,0,6,2,
                            1,3,7,1,7,5
                        }).CreateModel(new SolidColorBrush(Colors.Gray));
                    var supportOriginTransform = support.Transform;
                    var ans = new Model3DGroup();
                    ans.Children.Add(blade);
                    ans.Children.Add(support);
                    System.Diagnostics.Trace.WriteLine($"coneangle={coneAngle},{coneAngle / Math.PI * 180}");
                    setFoldingAction = new Action<double>(folding =>
                      {
                          double angle = coneAngle + folding * (Math.PI / 2 - coneAngle);
                          blade.Transform = MyLib.Transform(bladeOriginTransform).TranslatePrepend(new Vector3D(0.5 * R*Math.Cos(angle),0, 0.5 * R * Math.Sin(angle))).RotatePrepend(new Vector3D(0, 1, 0), angle).TranslatePrepend(new Vector3D(-R / 2, 0, 0)).RotatePrepend(new Vector3D(1, 0, 0), attackAngle).Value;
                          support.Transform = MyLib.Transform(supportOriginTransform).Rotate(new Vector3D(0, 1, 0), -angle).Value;
                      });
                    setFoldingAction(0);
                    return ans;
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
