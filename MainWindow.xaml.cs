using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace gstreamer_shuffleboard_installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string GStreamerExeName
        {
            get { if (Environment.Is64BitOperatingSystem)
                {
                    return "gstreamer-1.0-mingw-x86_64-1.16.2.msi";
                } else
                {
                    return "gstreamer-1.0-mingw-x86-1.16.2.msi";
                }
            }
        }
        private static string GStreamerExeDownload => "https://gstreamer.freedesktop.org/data/pkg/windows/1.16.2/" + GStreamerExeName;
        public string TitleLabel
        {
            get
            {
                if (Environment.Is64BitOperatingSystem)
                {
                    return "GStreamer Shuffleboard (64-bit)";
                }
                else
                {
                    return "GStreamer Shuffleboard (32-bit)";
                }
            }
        }

        private static readonly WebClient client = new WebClient();
        private bool downloading = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(GStreamerExeDownload);
        }

        private void StartDownload(object sender, RoutedEventArgs e)
        {
            if (downloading) return;
            downloading = true;
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
            client.DownloadFileAsync(new Uri(GStreamerExeDownload), GStreamerExeName);
        }
        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            DownloadInfo.Content = $"{e.BytesReceived} out of {e.TotalBytesToReceive} bytes downloaded ({percentage:F02}%)";
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            downloading = false;
            DownloadInfo.Content = "Download Completed";
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (InstallOptionsGStreamer.SelectedIndex == 0)
            {
                if (!File.Exists(GStreamerExeName))
                {
                    MessageBox.Show(this, "GStreamer Exe Not Downloaded!", "Installer");
                    return;
                }

                using (var process = new Process())
                {
                    process.StartInfo.FileName = "msiexec.exe";
                    if (InstallOptionsAutomatic.SelectedIndex == 0)
                    {
                        process.StartInfo.Arguments = string.Format("/passive /i {0}", GStreamerExeName);
                    }
                    else
                    {
                        process.StartInfo.Arguments = string.Format("/i {0}", GStreamerExeName);
                    }

                    await Task.Run(() =>
                    {
                        process.Start();
                        process.WaitForExit();
                    });
                    if (process.ExitCode != 0)
                    {
                        MessageBox.Show(this, "GStreamer Installation Failed!", "Installer");
                        return;
                    }
                }
            }

            await Task.Run(() =>
            {
                var resource = Properties.Resources.gstreamer_shuffleboard_1_0_0;
                File.WriteAllBytes(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), @"Shuffleboard\plugins\", Properties.Resources.ShuffleboardJarName), resource);
            });

            MessageBox.Show("Installation Successful!", "Installer");
            this.Close();
        }
    }
}
