using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Collections.Generic;

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
                //var vertices = new List<Point3D>{, new Point3D(r, -r, -r), new Point3D(-r, r, -r), new Point3D(r, r, -r), new Point3D(-r, -r, r), new Point3D(r, -r, r), new Point3D(-r, r, r), new Point3D(r, r, r) };
                My3DGraphics.Cuboid.AddFaces(new Point3D(r, r, r),out List<Point3D>vertices, out List<int> triangleIndices, out List<Vector3D> normals);
                return My3DGraphics.NewModel().AddTriangles(vertices,triangleIndices,normals).CreateModel(new SolidColorBrush(Colors.PeachPuff));
            }
        }
    }
}
