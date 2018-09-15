using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Digging_Game_3
{
    public static class Kernel
    {
        public static PerspectiveCamera Camera;
        public static class CameraProperties
        {
            public static Transform3D BaseTransform=new MatrixTransform3D();
            public static Point3D position=new Point3D(1,0,0);
            public static Vector3D lookDirection = new Vector3D(0,0,-1);
        }
        public static class Heart
        {
            public delegate void HeartBeatEventHandler(double secs);
            public static event HeartBeatEventHandler Beat;
            public static void MakeBeat(double secs)
            {
                Beat?.Invoke(secs);
                var trans = MyLib.Transform(CameraProperties.BaseTransform);
                trans = trans.TranslatePrepend(CameraProperties.position - new Point3D());
                var cross = Vector3D.CrossProduct(Kernel.Camera.LookDirection, CameraProperties.lookDirection);
                if (cross.Length != 0)
                {
                    var dot = Vector3D.DotProduct(Kernel.Camera.LookDirection, CameraProperties.lookDirection);
                    var angle = Math.Acos(dot / Kernel.Camera.LookDirection.Length / CameraProperties.lookDirection.Length);
                    trans = trans.RotatePrepend(cross, angle);
                }
                //if (Keyboard.IsDown(System.Windows.Input.Key.Z)) System.Diagnostics.Trace.WriteLine(CameraProperties.position-new Point3D());
                Kernel.Camera.Transform = trans.Value;
            }
        }
    }
}
