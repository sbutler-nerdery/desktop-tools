using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        #region Fields

        private Action<string> _Callback;
        private string _LoginUrl;

        #endregion

        #region Methods

        public Login()
        {
            InitializeComponent();
        }

        public Login(Action<string> callback, string loginUrl) : this()
        {
            _Callback = callback;
            _LoginUrl = loginUrl;
        }

        private void ShowAuthenticatingMode()
        {
            Loader.Visibility = Visibility.Visible;
            Browser.Visibility = Visibility.Collapsed;
            Title = "Authenticating...";            
        }

        #endregion

        #region Event Handlers

        private void Login_OnLoaded(object sender, RoutedEventArgs e)
        {
            ShowAuthenticatingMode();
            Browser.Navigate(_LoginUrl);
        }

        private void Browser_OnNavigated(object sender, NavigationEventArgs e)
        {
            var url = e.Uri.ToString(); 

            if (url.Contains("#access_token"))
            {
                var token = url.Split('#')[1].Split('&')[0].Split('=')[1];

                _Callback(token);
                Hide();
            }
            else
            {
                Title = "Login";
                Loader.Visibility = Visibility.Collapsed;
                Browser.Visibility = Visibility.Visible;         
            }
        }

        private void Browser_OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            ShowAuthenticatingMode();
        }

        #endregion
    }
}
