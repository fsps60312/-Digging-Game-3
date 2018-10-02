using System.Windows.Media.Media3D;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Digging_Game_3.Models
{
    partial class Pod
    {
        public class Tracks : My3DObject//point +x, upward +z
        {
            public class Track : My3DObject
            {
                protected override Model3D CreateModel(params object[] vs)
                {
                    return TemporaryModel();
                }
                static Model3D TemporaryModel()
                {
                    const double depth = 1, height = 1, lengthUp = 4, lengthDown = 2.5;
                    List<Point3D> positions = new List<Point3D> { new Point3D(-lengthUp / 2, height / 2, 0), new Point3D(lengthUp / 2, height / 2, 0), new Point3D(lengthDown / 2, -height / 2, 0), new Point3D(-lengthDown / 2, -height / 2, 0) };
                    List<int> indices = new List<int> { 0, 3, 2, 0, 2, 1 };
                    List<Point3D> trackPositions = positions.SelectConcat(v => v + new Vector3D(0, 0, depth / 2), v => v + new Vector3D(0, 0, -depth / 2)).ToList();
                    ///4----5
                    /// 7--6
                    ///
                    ///0----1
                    /// 3--2
                    List<int> trackIndices = indices.Concat(indices.Select(v => positions.Count + v).Reverse())
                        .Concat(new[] { 0, 1, 5, 0, 5, 4 }.SelectAdd(v => v / 4 * 4 + (v + 1) % 4, v => v / 4 * 4 + (v + 2) % 4, v => v / 4 * 4 + (v + 3) % 4)).ToList();
                    return My3DGraphics.NewModel().AddTriangles(trackPositions, trackIndices).CreateModel(new SolidColorBrush(Colors.SlateGray));
                    //List<Point3D> tracksPositions = trackPositions.SelectConcat(v => v + new Vector3D(0, 0, gap / 2), v => v + new Vector3D(0, 0, -gap / 2)).ToList();
                    //List<int> tracksIndices = trackIndices.Concat(trackIndices.Select(v => trackPositions.Count + v)).ToList();
                }
            }
            Track leftTrack, rigtTrack;
            protected override Model3D CreateModel(params object[] vs)
            {
                const double gap = 2.5;
                leftTrack = new Track();
                rigtTrack = new Track();
                leftTrack.Transform = leftTrack.OriginTransform = MyLib.Transform(leftTrack).TranslatePrepend(new Vector3D(0, 0, -gap / 2)).Value;
                rigtTrack.Transform = rigtTrack.OriginTransform = MyLib.Transform(rigtTrack).TranslatePrepend(new Vector3D(0, 0, gap / 2)).Value;
                var ans = new Model3DGroup();
                ans.Children.Add(leftTrack.Model);
                ans.Children.Add(rigtTrack.Model);
                return ans;
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
