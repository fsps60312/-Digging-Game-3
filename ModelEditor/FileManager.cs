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
        public delegate void NewDataEventHandler(string data);
        public event NewDataEventHandler NewData;
        public async Task UserOpenFile()
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.ShowDialog();
            MainWindow.Instance.Title = fd.FileName;
            var stream = fd.OpenFile();
            if (stream == null) return;
            string content;
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                content = await reader.ReadToEndAsync();
            }
            NewData?.Invoke(content);
        }
        public void LoadString(string content)
        {
            NewData?.Invoke(content);
        }
    }
}
