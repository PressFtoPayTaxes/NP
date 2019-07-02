using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

namespace HTTP_SMTP_FTP
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

        private Dictionary<string, int> _dictionary = new Dictionary<string, int>();

        private async void ParseButtonClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(urlTextBox.Text))
                return;

            string url = (urlTextBox.Text.Contains("http://")) ? urlTextBox.Text : "http://" + urlTextBox.Text;

            HttpWebRequest request = WebRequest.CreateHttp(url);
            var response = await request.GetResponseAsync();

            string stringResponse = "";
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                stringResponse = await reader.ReadToEndAsync();
            }

            ParseResult(stringResponse);
        }

        private void ParseResult(string result)
        {
            _dictionary.Clear();

            string[] tags = result.Split('<', '>');

            foreach (var tag in tags)
            {
                string trueTag = tag.Split(' ')[0];
                if (_dictionary.ContainsKey(trueTag))
                    _dictionary[trueTag]++;
                else
                    _dictionary.Add(trueTag, 1);
            }

            ShowResult();
        }

        private void ShowResult()
        {
            resultTextBox.Clear();
            foreach (var item in _dictionary)
            {
                resultTextBox.AppendText($"Тег {item.Key} встречается {item.Value} раз\n");
            }
        }
    }
}
