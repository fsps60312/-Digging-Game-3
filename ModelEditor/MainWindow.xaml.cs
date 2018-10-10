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
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance;
        MainViewport3D mainViewport;
        Label labelInfo;
        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
            InitializeViews();
            mainViewport.SetFileManager(new FileManager());
            this.KeyDown += MainWindow_KeyDown;
            this.KeyUp += MainWindow_KeyDown;
        }

        private async void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsDown) keyPressed.Add(e.Key);
            else keyPressed.Remove(e.Key);
            ShowKeyStates();
            if(e.IsDown)
            {
                if(e.Key==Key.O)
                {
                    await mainViewport.UserLoad();
                }
            }
        }
        void InitializeViews()
        {
            this.Content = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition{Width=new GridLength(1,GridUnitType.Star)},
                    new ColumnDefinition{Width=new GridLength(1,GridUnitType.Auto)},
                },
                Children =
                {
                    new Viewbox
                    {
                        StretchDirection = StretchDirection.Both,
                        Stretch = Stretch.Uniform,
                        Child = (mainViewport=new MainViewport3D())
                    }.Set(0,0),
                    (labelInfo=new Label{Content="Info" }).Set(0,1)
                }
            };
        }
        HashSet<Key> keyPressed = new HashSet<Key>();
        void ShowKeyStates()
        {
            this.Title = string.Join(", ", keyPressed);
        }
    }
}
