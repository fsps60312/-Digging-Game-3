using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;

namespace ModelEditor
{
    public static class UIelement_Extensions
    {
        public static UIElement Set(this UIElement uIElement,int row,int column)
        {
            Grid.SetRow(uIElement, row);
            Grid.SetColumn(uIElement, column);
            return uIElement;
        }
        public static UIElement SetSpan(this UIElement uIElement,int rowSpan,int columnSpan)
        {
            Grid.SetRowSpan(uIElement, rowSpan);
            Grid.SetColumnSpan(uIElement, columnSpan);
            return uIElement;
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance;
        FileManager FM = new FileManager();
        MainViewport3D mainViewport;
        Label labelInfo;
        TextBox textBox;
        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
            InitializeViews();
            RegisterEvents();
            InitializeAsync();
        }
        async void InitializeAsync()
        {
            var camera = new PerspectiveCamera();
            camera.FarPlaneDistance = 500;
            camera.NearPlaneDistance = 1;
            camera.Position = new Point3D(-5, -10, 5);
            camera.LookDirection = new Vector3D(1, 2, -1);
            camera.UpDirection = new Vector3D(0, 0, 1);
            mainViewport.Camera = camera;
            mainViewport.BuildScene = viewPort =>
            {
                //viewPort.Children.Add(new ModelVisual3D { Content = new DirectionalLight(Colors.White, new Vector3D(-1, -2, -4)) });
                //viewPort.Children.Add(new ModelVisual3D { Content = new DirectionalLight(Colors.White, new Vector3D(1, 2, 4)) });
                viewPort.Children.Add(new ModelVisual3D { Content = new AmbientLight ( Colors.White ) });
                viewPort.Children.Add(new ModelVisual3D
                {
                    Content = new Func<GeometryModel3D>(() =>
                    {
                        var mesh = new MeshGeometry3D();
                        foreach (var p in new[] { new Point3D(-1, -1, 0), new Point3D(1, -1, 0), new Point3D(-1, 1, 0), new Point3D(1, 1, 0) }) mesh.Positions.Add(p);
                        foreach (var i in new[] { 0, 1, 3, 0, 3, 2, 0, 2, 3, 0, 3, 1 }) mesh.TriangleIndices.Add(i);
                        for (int i = 0; i < mesh.TriangleIndices.Count; i++) mesh.Normals.Add(new Vector3D(0, 0, i * 2 < mesh.TriangleIndices.Count ? 1 : -1));
                        mesh.Freeze();
                        var brush = new SolidColorBrush(Colors.LightYellow) { Opacity = 0.5 };
                        var model = new GeometryModel3D(mesh, new DiffuseMaterial(brush));
                        model.Freeze();
                        return model;
                    })()
                });
            };
            mainViewport.OnMessage = s => labelInfo.Content = s;
            await mainViewport.Load("");
        }
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (!keyPressed.ContainsKey(e.Key)) keyPressed.Add(e.Key, e.KeyStates);
            else keyPressed[e.Key] = e.KeyStates;
            ShowKeyStates();
        }
        void RegisterEvents()
        {
            textBox.KeyDown += TextBox_KeyDown;
            FM.NewData += async s => await mainViewport.Load(s);
            labelInfo.MouseDoubleClick += async delegate { await FM.UserOpenFile(); };
            this.KeyDown += MainWindow_KeyDown;
            this.KeyUp += MainWindow_KeyDown;
        }

        private async void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && (KeyStatesOf(Key.LeftCtrl, Key.RightCtrl) & KeyStates.Down) != 0)
            {
                await mainViewport.Load((sender as TextBox).Text);
            }
        }

        void InitializeViews()
        {
            this.Content = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition{Height=new GridLength(1,GridUnitType.Star)},
                    new RowDefinition{Height=new GridLength(1,GridUnitType.Star)}
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition{Width=new GridLength(1,GridUnitType.Star)},
                    new ColumnDefinition{Width=new GridLength(1,GridUnitType.Star)},
                },
                Children =
                {
                    //new Viewbox
                    //{
                    //    StretchDirection = StretchDirection.Both,
                    //    Stretch = Stretch.Uniform,
                    //    Child = (mainViewport=new MainViewport3D()),
                    //    ClipToBounds=true
                    //}.Set(0,0).SetSpan(2,1),
                    (mainViewport=new MainViewport3D{ClipToBounds=true }).Set(0,0).SetSpan(2,1),
                    new ScrollViewer
                    {
                        HorizontalScrollBarVisibility =ScrollBarVisibility.Auto,
                        VerticalScrollBarVisibility=ScrollBarVisibility.Auto,
                        Content = (labelInfo=new Label{Content="Info" })
                    }.Set(0,1),
                    (textBox=new TextBox{AcceptsReturn=true }).Set(1,1)
                }
            };
        }
        KeyStates KeyStatesOf(params Key[] keys)
        {
            return keys.Select(k => keyPressed.ContainsKey(k) ? keyPressed[k] : KeyStates.None).Aggregate((a, b) => a | b);
        }
        Dictionary<Key, KeyStates> keyPressed = new Dictionary<Key, KeyStates>();
        void ShowKeyStates()
        {
            this.Title = string.Join(", ", keyPressed.Where(p => p.Value != KeyStates.None));
        }
    }
}
