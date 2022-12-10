using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Windows.Media;
using System.Windows.Input;

namespace Digging_Game_3.Models
{
    partial class Pod
    {
        partial class Propeller : My3DObject
        {
            private double omega = 0;
            public class PropertyDescription
            {
                public double friction=1;
                public Func<double, double> air_friction = o => 0.5 * o * o + 0.01 * Math.Abs(o);
                public double momentOfInertia=1;
                public double maxPower=50;//最大功率
                public double maxTorque=20;//最大扭力
                public double foldingTime=0.5;//用時
            }
            public PropertyDescription Properties = new PropertyDescription();
        }
        partial class Propeller:My3DObject
        {
            public enum Types { Basic }
            readonly static Dictionary<Types, Description> Descriptions = new Dictionary<Types, Description>
            {
                {
                    Types.Basic,new Description(new List<BladeSetDescription>
                    {
                        new BladeSetDescription(BladeSet.Types.Basic,2.0,false,1),
                        new BladeSetDescription(BladeSet.Types.Basic,2.0,true,0.5)
                    })
                }
            };
            private List<BladeSet> BladeSets = new List<BladeSet>();
            private double _Folding = 0;
            public double Folding
            {
                get { return _Folding; }
                set
                {
                    if(value<0.7)
                    {
                        double f = value / 0.7;
                        foreach (var v in BladeSets) v.Folding = f;
                        this.Transform = MyLib.Transform(OriginTransform).TranslatePrepend(new Vector3D(0, 0, Math.Sin(f * Math.PI / 2) * (MaxHeight - Height))).Value;
                    }
                    else
                    {
                        double h = (1-value) / 0.3;
                        foreach (var v in BladeSets) v.Folding = 1;
                        this.Transform = MyLib.Transform(OriginTransform).TranslatePrepend(new Vector3D(0, 0, -Height + h * MaxHeight)).Value;
                    }
                    _Folding = value;
                }
            }
            class BladeSetDescription
            {
                public BladeSet.Types Type;
                public double Radius;
                public bool Reversed;
                public double Height;
                public double SpeedRatio;
                public double AngleOffset;
                public BladeSetDescription(BladeSet.Types type,double radius,bool reversed,double height,double speedRatio=1,double angleOffset=0)
                {
                    Type = type;
                    Radius = radius;
                    Reversed = reversed;
                    Height = height;
                    SpeedRatio = speedRatio;
                    AngleOffset = angleOffset;
                }
            }
            class Description
            {
                public List<BladeSetDescription> BladeSets { get; private set; }
                public Description(List<BladeSetDescription> bladeSets)
                {
                    BladeSets = bladeSets;
                }
            }
            public Func<bool> IsOnGround = new Func<bool>(() => false);
            public Func<double> DownwardWindSpeed = new Func<double>(() => 0);
            public double LiftForce()
            {
                return (omega - DownwardWindSpeed() * 0.05) * (1 - Folding) * 5;
            }
            public Propeller(Types propellerType) : base(propellerType)
            {
                bool foldingTarget = false;
                Kernel.Heart.Beat1 += (secs) =>
                {
                    double torque = 0, frictionTorque = 0;
                    //keyboard control
                    if (Keyboard.IsDown(Key.S))
                    {
                        frictionTorque += (omega > 0 ? -1 : 1) * Math.Min(Properties.maxTorque, omega != 0 ? Properties.maxPower / Math.Abs(omega) : double.MaxValue);
                    }
                    else if (Keyboard.IsDown(Key.W))
                    {
                        torque += Math.Min(Properties.maxTorque, omega > 0 ? Properties.maxPower / omega : double.MaxValue);
                    }
                    if (Keyboard.IsDown(Key.W)) foldingTarget = false;
                    else if(Keyboard.IsDown(Key.S)&& Math.Abs(omega) <= Math.PI / 10) foldingTarget = true;
                    else if (!Keyboard.IsDown(Key.W) && !Keyboard.IsDown(Key.S)) foldingTarget = IsOnGround();

                    torque -= LiftForce() / 5 * 2;//風導致轉動
                    //friction
                    frictionTorque += (omega > 0 ? -1 : 1) * (Properties.friction + Properties.air_friction(omega));
                    if ((torque > 0) != (torque + frictionTorque > 0))
                    {
                        if (omega > 0) frictionTorque = Math.Max(frictionTorque, -torque - omega / secs);
                        else frictionTorque = Math.Min(frictionTorque, -torque - omega / secs);
                    }
                    torque += frictionTorque;
                    // # omega
                    omega += secs * torque;

                    //folding
                    if (foldingTarget) Folding = Math.Min(1, Folding + secs / Properties.foldingTime);
                    else Folding = Math.Max(0, Folding - secs / Properties.foldingTime);
                };
            }
            private double Height, MaxHeight;
            Model3DGroup CreateModel(Description description)
            {
                Model3DGroup ans = new Model3DGroup();
                BladeSets = new List<BladeSet>();
                foreach(var bladeSetDescription in description.BladeSets)
                {
                    var bladeSet = new BladeSet(
                        this,
                        bladeSetDescription.Type,
                        bladeSetDescription.Radius,
                        bladeSetDescription.Reversed,
                        bladeSetDescription.SpeedRatio,
                        bladeSetDescription.AngleOffset);
                    bladeSet.Transform = bladeSet.OriginTransform = MyLib.Transform(bladeSet).Translate(new Vector3D(0, 0, bladeSetDescription.Height)).Value;
                    BladeSets.Add(bladeSet);
                    ans.Children.Add(bladeSet.Model);
                }
                Height = description.BladeSets.Max(v => v.Height);
                MaxHeight = description.BladeSets.Max(v => Math.Max(v.Height, v.Radius) - v.Height + Height);
                //System.Windows.Forms.MessageBox.Show($"Height:{Height}, MaxHeight:{MaxHeight}");
                const double poleRadius = 0.05;
                ans.Children.Add(My3DGraphics.NewModel().AddTriangles(new[]
                {
                    new Point3D(-poleRadius, -poleRadius,Height-MaxHeight),
                    new Point3D(poleRadius, -poleRadius, Height-MaxHeight),
                    new Point3D(-poleRadius, poleRadius, Height-MaxHeight),
                    new Point3D(poleRadius, poleRadius, Height-MaxHeight),
                    new Point3D(-poleRadius, -poleRadius,Height),
                    new Point3D(poleRadius, -poleRadius, Height),
                    new Point3D(-poleRadius, poleRadius, Height),
                    new Point3D(poleRadius, poleRadius, Height)
                }, new[]
                /// 6 7
                /// 4 5
                /// 
                /// 2 3
                /// 0 1
                {
                    //0,2,3,0,3,1,
                    4,5,7,4,7,6,
                    0,1,5,0,5,4,
                    2,6,7,2,7,3,
                    0,4,6,0,6,2,
                    1,3,7,1,7,5
                }).CreateModel(new SolidColorBrush(Colors.SlateGray)));
                this.OriginTransform = ans.Transform;
                return ans;
            }
            protected override Model3D CreateModel(params object[] vs)
            {
                MyLib.AssertTypes(vs, typeof(Types));
                return CreateModel(Descriptions[(Types)vs[0]]);
            }
        }
    }
}
