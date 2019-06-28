using System;
using System.Net;
using System.Threading;
using System.Windows;
using Microsoft.Win32;

namespace HTTP_SMTP_FTP2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectPathButtonClick(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.ShowDialog();

            pathTextBox.Text = dialog.FileName;
        }

        private void DownloadButtonClick(object sender, RoutedEventArgs e)
        {
            downloadButton.IsEnabled = false;
            downloadButton.Content = "Скачивается...";

            ThreadPool.QueueUserWorkItem(DownloadFile);
        }

        private void DownloadFile(object state)
        {
            string url = "";
            string path = "";
            Dispatcher.Invoke(() => url = urlTextBox.Text);
            Dispatcher.Invoke(() => path = pathTextBox.Text);

            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(url, path);
                }
            }
            catch(WebException exception)
            {
                MessageBox.Show($"Ошибка сети: {exception.Message}");
                Dispatcher.Invoke(() =>
                {
                    downloadButton.IsEnabled = true;
                    downloadButton.Content = "Скачать";
                });
                return;
            }
            catch(Exception exception)
            {
                MessageBox.Show($"Ошибка: {exception.Message}");
                Dispatcher.Invoke(() =>
                {
                    downloadButton.IsEnabled = true;
                    downloadButton.Content = "Скачать";
                });
                return;
            }

            MessageBox.Show("Файл скачан");
            Dispatcher.Invoke(() =>
            {
                downloadButton.IsEnabled = true;
                downloadButton.Content = "Скачать";
            });
        }
    }
}
