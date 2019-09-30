using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System;

namespace Digging_Game_3.Models
{
    partial class Pod
    {
        partial class Propeller
        {
            partial class BladeSet : My3DObject
            {
                const int TransformIndexTheta = 0;
                public enum Types { Basic }
                static readonly Dictionary<Types, Description> Descriptions = new Dictionary<Types, Description>
                {
                    {Types.Basic,new Description( Blade.Types.BasicTriangle,3) }
                };
                class Description
                {
                    public Blade.Types BladeType { get; private set; }
                    public int Count { get; private set; }
                    public Description(Blade.Types bladeType, int count)
                    {
                        BladeType = bladeType;
                        Count = count;
                    }
                }
                List<Blade> Blades = new List<Blade>();
                public bool Reversed { get; private set; }
                public double SpeedRatio { get; private set; }
                public double AngleOffset { get; private set; }
                //double _Height=0;
                //public double Height
                //{
                //    get { return _Height; }
                //    set
                //    {
                //        this.Transform = MyLib.Transform(OriginTransform).Translate(new Vector3D(0,0,Height)).Value;
                //        _Height = value;
                //    }
                //}
                double _Folding = 0;
                public double Folding
                {
                    get { return _Folding; }
                    set
                    {
                        foreach(var v in Blades)
                        {
                            v.Transform = MyLib.Transform(v.OriginTransform).RotatePrepend(new Vector3D(0, 1, 0), Math.PI/2 * value).Value;
                        }
                        _Folding = value;
                    }
                }
                public Propeller Parent { get; private set; }
                public BladeSet(Propeller parent, Types type, double radius, bool reversed, double speedRatio, double angleOffset) : base(parent, type, radius, reversed, speedRatio, angleOffset)
                {
                    Kernel.Heart.Beat1 += (secs) =>
                    {
                        MyLib.Set(SubTransforms, TransformIndexTheta, false).RotatePrepend(new Vector3D(0, 0, 1), (Reversed ? -1 : 1) * (secs * SpeedRatio * 5 * Parent.omega)).Done();
                        UpdateTransform();
                    };
                }
                Model3DGroup CreateModel(Description description,double radius)
                {
                    Model3DGroup ans = new Model3DGroup();
                    Blades = new List<Blade>();
                    for(int i=0;i<description.Count;i++)
                    {
                        var blade = new Blade(description.BladeType, radius,MyLib.ToRad(20), Reversed);
                        blade.Transform = blade.OriginTransform = MyLib.Transform(blade).Rotate(new Vector3D(0, 0, 1), (Reversed ? -1 : 1) * (AngleOffset + i * 2.0 * Math.PI / description.Count)).Value;
                        Blades.Add(blade);
                        ans.Children.Add(blade.Model);
                    }
                    SubTransforms.Add(new MatrixTransform3D());//theta
                    return ans;
                }
                protected override Model3D CreateModel(params object[] vs)
                {
                    MyLib.AssertTypes(vs,typeof(Propeller), typeof(Types), typeof(double), typeof(bool), typeof(double), typeof(double));
                    Parent = vs[0] as Propeller;
                    Reversed = (bool)vs[3];
                    SpeedRatio = (double)vs[4];
                    AngleOffset = (double)vs[5];
                    return CreateModel(Descriptions[(Types)vs[1]], (double)vs[2]);
                }
            }
        }
    }
}
