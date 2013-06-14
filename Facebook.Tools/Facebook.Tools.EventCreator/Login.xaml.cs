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
        private string _LogoutUrl;

        #endregion

        #region Constructors

        public Login()
        {
            InitializeComponent();
        }

        public Login(Action<string> callback, string loginUrl, string logoutUrl)
            : this()
        {
            _Callback = callback;
            _LoginUrl = loginUrl;
            _LogoutUrl = logoutUrl;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Log the user into facebook
        /// </summary>
        public void LoginUser()
        {
            ShowAuthenticatingMode();
            Browser.Navigate(_LoginUrl);            
        }
        /// <summary>
        /// Log the user out of facebook
        /// </summary>
        public void LogoutUser()
        {
            ShowAuthenticatingMode();
            Browser.Navigate(_LogoutUrl);            
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

        }

        private void Browser_OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            var url = e.Uri.ToString();

            Logger.LogInfo("--> " + url);

            if (url.Contains("#access_token"))
            {
                var token = url.Split('#')[1].Split('&')[0].Split('=')[1];

                e.Cancel = true;
                _Callback(token);
                Hide();
            }
            //Hide the window when an error is shown... facebook with display its own error in the browser
            //else if (url.Contains("error_code"))
            //{
            //    var queryParameters = url.Split('?')[1].Split('&');
            //    var errorCode = queryParameters.FirstOrDefault(x => x.Contains("error_code")).Split('=')[1];
            //    var errorMessage = queryParameters.FirstOrDefault(x => x.Contains("error_message")).Split('=')[1];

            //    ErrorCode.Content = errorCode;
            //    ErrorMessage.Content = errorMessage;
            //    ErrorInfo.Visibility = Visibility.Visible;
            //    Loader.Visibility = Visibility.Collapsed;
            //    Browser.Visibility = Visibility.Collapsed;
            //}
            else
            {
                Title = "Login";
                Loader.Visibility = Visibility.Collapsed;
                Browser.Visibility = Visibility.Visible;
            }
        }

        #endregion

        private void Ok_OnClick(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
