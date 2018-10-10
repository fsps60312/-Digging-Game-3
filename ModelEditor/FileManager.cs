using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Win32;

namespace ModelEditor
{
    class FileManager
    {
        public Stream UserOpenFile()
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.ShowDialog();
            MainWindow.Instance.Title = fd.FileName;
            return fd.OpenFile();
        }
    }
}
