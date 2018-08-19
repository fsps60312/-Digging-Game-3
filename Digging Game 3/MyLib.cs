﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Digging_Game_3
{
    #region Planets
    /// <summary>
    /// https://astrogeology.usgs.gov/search/planetary-index
    /// https://bjj.mmedia.is/data/
    /// Mercury:    https://astrogeology.usgs.gov/search/map/Mercury/Messenger/Global/Mercury_MESSENGER_MDIS_Basemap_BDR_Mosaic_Global_166m
    /// Venus:      https://astrogeology.usgs.gov/search/map/Venus/Magellan/Colorized/Venus_Magellan_C3-MDIR_Colorized_Global_Mosaic_4641m
    /// Earth:      
    /// Mars:       https://astrogeology.usgs.gov/search/map/Mars/Viking/MDIM21/Mars_Viking_MDIM21_ClrMosaic_global_232m
    ///             https://astrogeology.usgs.gov/search/map/Mars/Viking/Color/Mars_Viking_ClrMosaic_global_925m
    /// 
    /// </summary>
    #endregion
    public static class MyExtensions
    {
        public static Point3D Multiply(this Point3D a, double scale) { return new Point3D(a.X * scale, a.Y * scale, a.Z * scale); }
    }
    public static class MyLib
    {
        public static Random Rand = new Random();
        public class MyTrans
        {
            Matrix3D v;
            public MyTrans(Transform3D a) { v = a.Value; }
            MyTrans(Matrix3D v) { this.v = v; }
            public MyTrans RotatePrepend(Vector3D axis, double angleRad) { v.RotatePrepend(new Quaternion(axis, angleRad / Math.PI * 180)); return this; }
            public MyTrans Rotate(Vector3D axis, double angleRad) { v.Rotate(new Quaternion(axis, angleRad / Math.PI * 180)); return this; }
            public MyTrans TranslatePrepend(Vector3D a) { v.TranslatePrepend(a); return this; }
            public MyTrans Translate(Vector3D a) { v.Translate(a); return this; }
            public MatrixTransform3D Value { get { return new MatrixTransform3D(v); } }
            public MyTrans Copy() { return new MyTrans(v); }
        }
        public static MyTrans Transform(IMy3DObject a) { return new MyTrans(a.Model.Transform); }
        public static MyTrans Transform(Model3D a) { return new MyTrans(a.Transform); }
        public static MyTrans Transform(Transform3D a) { return new MyTrans(a); }
        public static MyTrans Transform(PerspectiveCamera a) { return new MyTrans(a.Transform); }
        public static Vector3D Norm(Vector3D a) { var d = a.Length; return new Vector3D(a.X / d, a.Y / d, a.Z / d); }
        public static Point3D FromAngular(double radius, double angle,double z = 0) { return new Point3D(radius * Math.Cos(angle), radius * Math.Sin(angle), z); }
        public static double ToRad(double degrees) { return degrees / 180 * Math.PI; }
        //public static void SmoothTo<T>(T _target,Expression<Func<T, double>> vExpr, double t, double secs, double timeToHalf)
        //{
        //    ///f = a x      ### f: location-velocity
        //    ///F = S(1/f) = S(1/a x^-1) = ln(x)/a: time
        //    ///secs/timeToHalf = (F(1)-F(x)) / (F(1)-F(0.5)) = ln(x) / ln(0.5)
        //    ///ln(x) = secs / timeToHalf * ln(0.5)
        //    ///x = Exp(secs / timeToHalf * ln(0.5))
        //    double x = Math.Exp(secs / timeToHalf * Math.Log(0.5));
        //    var expr = (MemberExpression)vExpr.Body;
        //    var prop = (System.Reflection.PropertyInfo)expr.Member;
        //    object target = _target;
        //    double v = (double)prop.GetValue(target);
        //    v = v * x + t * (1 - x);
        //    prop.SetValue(target, t);
        //    Trace.WriteLine($"{prop.GetValue(target).GetType()}: {prop.GetValue(target)}");
        //    Trace.Assert((double)prop.GetValue(target) == t);
        //}
        public static void CopyTo(Point3D p, out double x, out double y, out double z) { x = p.X; y = p.Y; z = p.Z; }
        public static void SmoothTo(ref double v,double t,double secs,double timeToHalf)
        {
            ///f = a x      ### f: location-velocity
            ///F = S(1/f) = S(1/a x^-1) = ln(x)/a: time
            ///secs/timeToHalf = (F(1)-F(x)) / (F(1)-F(0.5)) = ln(x) / ln(0.5)
            ///ln(x) = secs / timeToHalf * ln(0.5)
            ///x = Exp(secs / timeToHalf * ln(0.5))
            double x = Math.Exp(secs / timeToHalf * Math.Log(0.5));
            v = v * x + t * (1 - x);
        }
        public static bool All<T1>(this T1[]a,Func<int,T1,bool>predicate)
        {
            for (int i = 0; i < a.Length; i++) if (!predicate(i,a[i])) return false;
            return true;
        }
        public static void AssertTypes(object[]vs,params Type[]types)
        {
            Trace.Assert(vs.All((i, v) => v.GetType() == types[i]),null, DebugInfo(vs, types));
        }
        public static string DebugInfo(object a,params object[] b)
        {
            return DebugInfo(a)+"\r\n--------------------\r\n" + DebugInfo(b);
        }
        public static string DebugInfo(object o)
        {
            if(o.GetType().IsSubclassOf(typeof(IEnumerable))||o.GetType().IsSubclassOf(typeof(Array)))
            {
                return $"{{type: {o.GetType()}, value: {o}, children:\r\n" +
                    $"\r\n  {string.Join("\r\n", ((IEnumerable)o).Cast<object>().Select(v => DebugInfo(v))).Replace("\r\n", "\r\n  ")}"+"\r\n" +
                    "}";
            }
            else return $"{{type: {o.GetType()}, value: {o}}}";
        }
    }
}