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
        }
        public static class Heart
        {
            public delegate void HeartBeatEventHandler(double secs);
            public static event HeartBeatEventHandler Beat;
            public static void MakeBeat(double secs)
            {
                Beat?.Invoke(secs);
                Kernel.Camera.Transform =
                    MyLib.Transform(CameraProperties.BaseTransform)
                    .Translate(CameraProperties.position - new Point3D())
                    .Value;
            }
        }
    }
}
