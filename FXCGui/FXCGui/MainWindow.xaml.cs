using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using Microsoft.Win32;

namespace FXCGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
            
            if (args.Length>1 && args[1] != null && args[1] != String.Empty)
            {
                FilePath.Text = args[1];
            }
        }

        private void Browse_OnClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.ShowDialog();
            FilePath.Text = "\""+ofd.FileName+"\"";
        }

        private async void Compile_OnClick(object sender, RoutedEventArgs e)
        {
            if (FilePath.Text == "")
            {
                MessageBox.Show("Please browse the file");
                return;
            }
            var filePath = FilePath.Text.Substring(1, FilePath.Text.Length - 2);
            if (!File.Exists(filePath))
            {
                MessageBox.Show("Unable to find file");
                return;
            }
            var outputFilePath = filePath + "o";
            if (OutputFileName.Text != "")
            {
                outputFilePath = System.IO.Path.GetDirectoryName(filePath)+ OutputFileName.Text +".fxo";
            }

            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }

            var psi = new ProcessStartInfo(@"C:\Program Files (x86)\Microsoft DirectX SDK (June 2010)\Utilities\bin\x86\fxc.exe");
            psi.Arguments = "/E"+EntryPoint.Text+" /T " + Profile.SelectedValue.ToString().ToLower() + " /Fo \"" + outputFilePath + "\" \""+GetAnsi(filePath)+"\"";
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;
            var p = new Process();
            p.StartInfo = psi; 
            p.OutputDataReceived += p_OutputDataReceived;
            p.ErrorDataReceived += new DataReceivedEventHandler((s, er) => { Debug.WriteLine(er.Data); });
            p.Start();
            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            Output.Text = output;

        }

        void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Debug.WriteLine(e.Data);
        }

        private string GetAnsi(string path)
        {
            StreamReader fileStream = new StreamReader(path);
            string fileContent = fileStream.ReadToEnd();
            fileStream.Close();
            if (File.Exists(path.Replace(".fx", "-ansi.fx")))
            {
                File.Delete(path.Replace(".fx", "-ansi.fx"));
            }
            // Now writes the content in ANSI
            StreamWriter ansiWriter = new StreamWriter(path.Replace(".fx", "-ansi.fx"), false, Encoding.GetEncoding(1250));
            ansiWriter.Write(fileContent);
            ansiWriter.Close();

            return path.Replace(".fx", "-ansi.fx");
        }

      
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
           this.DragMove();
            base.OnMouseLeftButtonDown(e);
        }
        private void MinimizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
           this.Close();
        }
    }
}
