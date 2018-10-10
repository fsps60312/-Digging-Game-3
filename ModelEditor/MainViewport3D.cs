using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.IO;

namespace ModelEditor
{
    class MainViewport3D : Viewport3D
    {
        FileManager fileManager = null;
        Task Load(List<string> contentLines)
        {
            MessageBox.Show(string.Join("\r\n", contentLines));
            return Task.CompletedTask;
        }
        async Task LoadFile(Stream stream)
        {
            if (stream == null) return;
            string content;
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                content = await reader.ReadToEndAsync();
            }
            var lines = content.Split('\n').Select(s => s.Trim()).ToList();
            await Load(lines);
        }
        public async Task UserLoad()
        {
            if(fileManager==null)
            {
                MessageBox.Show("fileManager is null");
                return;
            }
            try
            {
                using (var stream = fileManager.UserOpenFile())
                {
                    await LoadFile(stream);
                }
            }
            catch (Exception error) { MessageBox.Show(error.ToString()); }
        }
        public void SetFileManager(FileManager fm) { fileManager = fm; }
        public MainViewport3D() { }
    }
}
