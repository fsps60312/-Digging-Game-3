using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace Digging_Game_3.Models
{
    class Block : My3DObject
    {
        public const double Width = 5, Height = 4, Depth = 1;
        public static readonly Point3D Anchor = new Point3D(0, 0, 1);
        protected override Model3D CreateModel(params object[] vs)
        {
            My3DGraphics.NewModel().AddTriangles(new[] { new Point3D() }.Select(v=>v+(Anchor-new Point3D())), new[] { 0 });
            throw new NotImplementedException();
        }
    }
}
