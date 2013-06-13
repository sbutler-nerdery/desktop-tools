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

        #endregion

        #region Event Handlers

        private void Browser_OnNavigated(object sender, NavigationEventArgs e)
        {
            var url = e.Uri.ToString();
            if (url.Contains("#access_token"))
            {
                var token = url.Split('#')[1].Split('&')[0].Split('=')[1];
                _Callback(token);
                Hide();
            }
        }

        private void Login_OnLoaded(object sender, RoutedEventArgs e)
        {
            Browser.Navigate(_LoginUrl);
        }

        #endregion
    }
}
