﻿using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Windows.Media;

namespace Digging_Game_3.Models
{
    partial class Pod
    {
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
            List<BladeSet> BladeSets = new List<BladeSet>();
            double _Folding = 0;
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
            public Propeller(Types propellerType) : base(propellerType) { }
            double Height, MaxHeight;
            Model3DGroup CreateModel(Description description)
            {
                Model3DGroup ans = new Model3DGroup();
                BladeSets = new List<BladeSet>();
                foreach(var bladeSetDescription in description.BladeSets)
                {
                    var bladeSet = new BladeSet(
                        bladeSetDescription.Type,
                        bladeSetDescription.Radius,
                        bladeSetDescription.Reversed,
                        bladeSetDescription.SpeedRatio,
                        bladeSetDescription.AngleOffset);
                    bladeSet.Transform = MyLib.Transform(bladeSet).Translate(new Vector3D(0, 0, bladeSetDescription.Height)).Value;
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