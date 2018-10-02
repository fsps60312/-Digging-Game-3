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

namespace Digging_Game_3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Viewport3D mainViewPort;
        //Light EnvironmentLight = new DirectionalLight(Colors.White, new Vector3D(2, 3, 1));
        
        Model3D model;
        async void StartAnimation()
        {
            //MessageBox.Show((RenderCapability.Tier>>16).ToString());
            RenderOptions.SetEdgeMode(mainViewPort, EdgeMode.Aliased);
            //mainViewPort.Effect = new System.Windows.Media.Effects.BlurEffect { /*Radius = 5*/ };
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }
        DateTime lastUpdateTime = DateTime.Now,statisticTime=DateTime.Now;
        int cnt = 0;
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            var time = DateTime.Now;
            Kernel.Heart.MakeBeat((time - lastUpdateTime).TotalSeconds);
            lastUpdateTime = time;
            if (++cnt == 100)
            {
                this.Title = $"{cnt/(time-statisticTime).TotalSeconds} fps";
                cnt = 0;
                statisticTime = time;
            }
        }
        //UIElement SetPosition(UIElement uIElement,int row,int column)
        //{
        //    Grid.SetRow(uIElement, row);
        //    Grid.SetColumn(uIElement, column);
        //    return uIElement;
        //}
        public const double HeightRatio = 1.0 / 4 * 3;
        void InitializeViews()
        {
            mainViewPort = new Viewport3D { ClipToBounds = true, IsHitTestVisible = false, MinHeight = HeightRatio, MinWidth = 1 };
            this.Content = new Viewbox
            {
                StretchDirection = StretchDirection.Both,
                Stretch = Stretch.Uniform,
                Child = mainViewPort
            };
            //this.Content = new Grid
            //{
            //    //RowDefinitions=
            //    //{
            //    //    new RowDefinition{Height=new GridLength(1,GridUnitType.Star)},
            //    //    new RowDefinition{Height=new GridLength(1,GridUnitType.Auto)},
            //    //    new RowDefinition{Height=new GridLength(1,GridUnitType.Star)}
            //    //},
            //    //ColumnDefinitions=
            //    //{
            //    //    new ColumnDefinition{Width=new GridLength(1,GridUnitType.Star)},
            //    //    new ColumnDefinition{Width=new GridLength(1,GridUnitType.Auto)},
            //    //    new ColumnDefinition{Width=new GridLength(1,GridUnitType.Star)}
            //    //},
            //    Children =
            //    {
            //        SetPosition(,0,0)
            //    }
            //};
            this.KeyDown += MainWindow_KeyDown;
            this.KeyUp += MainWindow_KeyUp;
            {
                mainViewPort.Children.Add(new ModelVisual3D { Content = new AmbientLight(Colors.DarkGray) });
                //this.mainViewPort.Children.Add(new ModelVisual3D { Content = new DirectionalLight(Colors.Black, new Vector3D(-1, -4, -1.2)*-1) });
                mainViewPort.Children.Add(new ModelVisual3D { Content = new DirectionalLight(Colors.White, new Vector3D(-0.9, -5, -1)) });//Antique, Floral
                //this.mainViewPort.Children.Add(new ModelVisual3D { Content = new DirectionalLight(Colors.White, new Vector3D(0,-1,0)) });
                //this.mainViewPort.Children.Add(new ModelVisual3D { Content = new DirectionalLight(Colors.White, new Vector3D(-1,0,0)) });
                Kernel.Camera = new PerspectiveCamera();
                Kernel.Camera.FarPlaneDistance = 500;
                Kernel.Camera.NearPlaneDistance = 1;
                mainViewPort.Camera = Kernel.Camera;
                Kernel.CameraProperties.BaseTransform= Kernel.Camera.Transform;
            }
            {
                //double ratio = 1.5, r = 1;
                //var endPoints = new List<double>();
                //endPoints.Add(0);
                //endPoints.Add(Math.PI / (2 + ratio));
                //endPoints.Add(Math.PI * (1 + ratio) / (2 + ratio));
                //endPoints = endPoints.Concat(endPoints.Select(a => a + Math.PI)).ToList();
                //model = My3DGraphics.CreateHex(endPoints, r, r / 2);
                model = new Models.Pod().Model;
                mainViewPort.Children.Add(new ModelVisual3D { Content = model });
                //this.mainViewPort.Children.Add(new ModelVisual3D { Content = new Models.Pod().Model });
            }
            {
                model = new Models.Blocks().Model;
                mainViewPort.Children.Add(new ModelVisual3D { Content = model });
            }
        }
        void ShowKeyStates()
        {
            this.Title = string.Join(", ", Keyboard.keyPressed) + $" pos: {Kernel.Camera.Position} look: {Kernel.Camera.LookDirection} up: {Kernel.Camera.UpDirection}";
        }
        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            lock(Keyboard.keyPressed)
            {
                if (e.IsDown)
                {
                    if (!Keyboard.keyPressed.Contains(e.Key)) Keyboard.keyPressed.Add(e.Key);
                }
                else
                {
                    if (Keyboard.keyPressed.Contains(e.Key)) Keyboard.keyPressed.Remove(e.Key);
                }
            }
            ShowKeyStates();
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            lock (Keyboard.keyPressed)
            {
                if (e.IsDown)
                {
                    if (!Keyboard.keyPressed.Contains(e.Key)) Keyboard.keyPressed.Add(e.Key);
                }
                else
                {
                    if (Keyboard.keyPressed.Contains(e.Key)) Keyboard.keyPressed.Remove(e.Key);
                }
            }
            ShowKeyStates();
        }
        void MonitorKeys()
        {
            Kernel.Heart.Beat += (secs) =>
            {
                var trans = MyLib.Transform(Kernel.CameraProperties.BaseTransform);
                {
                    const double disDelta = 0.1, angleDelta = 2 * Math.PI / 180;
                    Vector3D
                        z = -MyLib.Norm(Kernel.Camera.LookDirection),
                        x = MyLib.Norm(Vector3D.CrossProduct(Kernel.Camera.LookDirection, Kernel.Camera.UpDirection)),
                        y = MyLib.Norm(Vector3D.CrossProduct(z, x));
                    //Camera.UpDirection = z;
                    //Camera.LookDirection = x;
                    if (Keyboard.IsDown(Key.Up)) trans.RotatePrepend(x, angleDelta);
                    if (Keyboard.IsDown(Key.Down)) trans.RotatePrepend(x, -angleDelta);
                    if (Keyboard.IsDown(Key.Left)) trans.RotatePrepend(y, angleDelta);
                    if (Keyboard.IsDown(Key.Right)) trans.RotatePrepend(y, -angleDelta);
                    if (Keyboard.IsDown(Key.OemOpenBrackets)) trans.RotatePrepend(z, -angleDelta);
                    if (Keyboard.IsDown(Key.OemCloseBrackets)) trans.RotatePrepend(z, angleDelta);
                    if (Keyboard.IsDown(Key.F)) trans.TranslatePrepend(-x * disDelta);
                    if (Keyboard.IsDown(Key.H)) trans.TranslatePrepend(x * disDelta);
                    if (Keyboard.IsDown(Key.T)) trans.TranslatePrepend(y * disDelta);
                    if (Keyboard.IsDown(Key.G)) trans.TranslatePrepend(-y * disDelta);
                    if (Keyboard.IsDown(Key.R)) trans.TranslatePrepend(-z * disDelta);
                    if (Keyboard.IsDown(Key.Y)) trans.TranslatePrepend(z * disDelta);
                }
                Kernel.CameraProperties.BaseTransform = trans.Value;
            };
        }
        public MainWindow()
        {
            InitializeComponent();
            InitializeViews();
            StartAnimation();
            MonitorKeys();
            //LaunchOldForm();
        }
        async void LaunchOldForm()
        {
            await Task.Delay(100000);
        }
    }
}
