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

namespace Facebook.Tools.EventCreator
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Action<string> Callback { get; set; }
        public string LoginUrl { get; set; }

        public Login()
        {
            InitializeComponent();
        }

        private void Browser_OnNavigated(object sender, NavigationEventArgs e)
        {
            var url = e.Uri.ToString();
            if (url.Contains("#access_token"))
            {
                var token = url.Split('#')[1].Split('&')[0].Split('=')[1];
                Callback(token);
                Hide();
            }
        }

        private void Login_OnLoaded(object sender, RoutedEventArgs e)
        {
            Browser.Navigate(LoginUrl);
        }
    }
}
