using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.IO;
using System.Diagnostics;
using System.Windows.Media;

namespace ModelEditor
{
    class MainViewport3D : Viewport3D
    {
        public Action<string> OnMessage = s => { };
        public Action<Viewport3D> BuildScene = p => { };
        /// https://en.wikipedia.org/wiki/Wavefront_.obj_file
        Task Load(List<Point3D> vertexes, List<Vector3D> normals, List<List<Tuple<int, int, int>>> faces)
        {
            var visual3d = new Func<ModelVisual3D>(() =>
              {
                  var mesh = new MeshGeometry3D();
                  foreach (var face in faces)
                  {
                      var v = face.Select(p => p.Item1 >= 1 ? vertexes[p.Item1 - 1] : new Point3D()).ToList();
                      var vt = face.Select(p => new Point()).ToList();
                      var vn = face.Select(p => p.Item3 >= 1 ? normals[p.Item3 - 1] : new Vector3D()).ToList();
                      int n = face.Count;
                      Trace.Assert(v.Count == n && vt.Count == n && vn.Count == n);
                      int offset = mesh.Positions.Count;
                      for (int i = 0; i < n; i++) mesh.Positions.Add(v[i]);
                      for (int i = 2; i < n; i++)
                      {
                          foreach (int _ in new[] { 0, i - 1, i })
                          {
                              mesh.TriangleIndices.Add(offset + _);
                              mesh.TextureCoordinates.Add(vt[_]);
                              mesh.Normals.Add(vn[_]);
                          }
                      }
                  }
                  mesh.Freeze();
                  var brush = new SolidColorBrush(Colors.Purple){ Opacity=0.7};
                  var backBrush = new SolidColorBrush(Colors.LightGreen) { Opacity = 0.7 };
                  var model = new GeometryModel3D(mesh, new DiffuseMaterial(brush)){ BackMaterial = new DiffuseMaterial(backBrush) };
                  model.Freeze();
                  var visual= new ModelVisual3D { Content = model };
                  return visual;
              })();
            this.Children.Clear();
            //this.Children.Add(new ModelVisual3D { Content = new DirectionalLight(Colors.White, new Vector3D(-4, -2, -1)) });
            //this.Children.Add(new ModelVisual3D { Content = new AmbientLight(Colors.White) });
            this.Children.Add(visual3d);
            BuildScene(this);
            //this.Children.Add(new ModelVisual3D
            //{
            //    Content = new Func<GeometryModel3D>(() =>
            //    {
            //        var mesh = new MeshGeometry3D();
            //        foreach (var p in new[] { new Point3D(-5, -5, 0), new Point3D(5, -5, 0), new Point3D(-5, 5, 0), new Point3D(5, 5, 0) }) mesh.Positions.Add(p);
            //        foreach (var i in new[] { 0, 1, 3, 0, 3, 2, 0, 2, 3, 0, 3, 1 }) mesh.TriangleIndices.Add(i);
            //        mesh.Freeze();
            //        var brush = new SolidColorBrush(Colors.LightYellow);
            //        return new GeometryModel3D(mesh, new DiffuseMaterial(brush));
            //    })()
            //});
            OnMessage($"{vertexes.Count} vertexes\r\n" +
                string.Join("\r\n", vertexes) + $"\r\n{new string('=',20)}\r\n" +
                $"{normals.Count} normals\r\n" +
                string.Join("\r\n", normals) + $"\r\n{new string('=', 20)}\r\n" +
                $"{faces.Count} faces\r\n" +
                string.Join("\r\n", faces.Select(vs => string.Join(" ", vs.Select(v => $"{v.Item1}/{v.Item2}/{v.Item3}")))));
            return Task.CompletedTask;
        }
        string DeComment(string v)
        {
            int i = v.IndexOf('#');
            return i == -1 ? v : v.Remove(i);
        }
        async Task Load(List<string> contentLines)
        {
            var vertexLines = contentLines.Where(v => v.StartsWith("v ")).Select(v => DeComment(v).Trim().Substring(2).Split(' ')).ToList();
            var normalLines = contentLines.Where(v => v.StartsWith("vn ")).Select(v => DeComment(v).Trim().Substring(3).Split(' ')).ToList();
            var faces = contentLines.Where(v => v.StartsWith("f ")).Select(v => DeComment(v).Trim().Substring(2).Split(' ').Select(u => u.Split('/'))).ToList();
            await Load(
                vertexLines.Select(v => new Point3D(double.Parse(v[0]), double.Parse(v[1]), double.Parse(v[2]))).ToList(),
                normalLines.Select(v => new Vector3D(double.Parse(v[0]), double.Parse(v[1]), double.Parse(v[2]))).ToList(),
                faces.Select(v => v.Select(u => u.Select(i => i == "" ? 0 : int.Parse(i)).ToArray()).Select(u => new Tuple<int, int, int>(u[0], u[1], u[2])).ToList()).ToList()
            );
        }
        static long counter = 0;
        public async Task Load(string content)
        {
            try
            {
                await Load(content.Split('\n').Select(s => s.Trim()).ToList());
            }
            catch (Exception error) { OnMessage($"#{++counter}\r\n" + error.ToString()); }
        }
        public MainViewport3D() { }
    }
}
