using System.Windows.Media.Media3D;
using System.Windows.Media;

namespace Digging_Game_3.Models
{
    partial class Pod
    {
        class Body : My3DObject
        {
            public Body():base()
            {
            }
            protected override Model3D CreateModel(params object[] vs)
            {
                double r = 1.5;
                ///6 7
                ///4 5
                ///
                ///2 3
                ///0 1
                return My3DGraphics.NewModel().AddTriangles(
                    new[] { new Point3D(-r,-r,-r),new Point3D(r,-r,-r),new Point3D(-r,r,-r),new Point3D(r,r,-r),new Point3D(-r,-r,r),new Point3D(r,-r,r),new Point3D(-r,r,r),new Point3D(r,r,r) },
                    new[]
                    {
                        0,2,3,0,3,1,
                        4,5,7,4,7,6,
                        0,1,5,0,5,4,
                        2,6,7,2,7,3,
                        0,4,6,0,6,2,
                        1,3,7,1,7,5
                    }).CreateModel(new SolidColorBrush(Colors.PeachPuff));
            }
        }
    }
}
