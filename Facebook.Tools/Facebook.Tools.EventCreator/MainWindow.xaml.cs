﻿using System;
using System.Collections.Generic;
using System.Configuration;
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
using Newtonsoft.Json.Linq;

namespace Facebook.Tools.EventCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Fields

        private string _AccessToken;
        private string _PageAccessToken;
        private string _DomainUrl;
        private string _AppId;
        private string _PageId;
        private string _Data;
        private Login _LoginForm;

        #endregion

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();

            //Get setting from the app config
            _AppId = Properties.Settings.Default.AppId;
            _DomainUrl = Properties.Settings.Default.DomainUrl;
            _PageId = Properties.Settings.Default.PageId;

            const string urlTemplate = "https://www.facebook.com/dialog/oauth?client_id={0}&scope=create_event&redirect_uri={1}/somepage.html&response_type=token";
            _LoginForm = new Login(LoginCallback, string.Format(urlTemplate, _AppId, _DomainUrl));
        }

        #endregion

        #region Event Handlers

        #region Window

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            //Make sure that the user is logged in.
            _LoginForm.ShowDialog();
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        #endregion

        #region Buttons

        private void BrowseForFiles_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            
            if (dialog.ShowDialog() == true)
            {
                CsvFile.Text = dialog.FileName;
            }
        }

        private void CreateEvents_OnClick(object sender, RoutedEventArgs e)
        {
            if (_Data == "")
            {
                MessageBox.Show("I can't create events out of nothing! Please enter some CVS event values.", "Doh!", MessageBoxButton.OK,
                                MessageBoxImage.Exclamation);
                return;
            }

            var events = _Data.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in events)
            {
                //split by commas
                var csvValues = item.Split(',');
                var name = csvValues[0];
                var date = csvValues[1];

                var fb = new FacebookClient();
                var parameters = new Dictionary<string, object>();
                parameters.Add("access_token", _PageAccessToken);
                parameters.Add("name", name);
                parameters.Add("start_time", date);
                var postUrl = string.Format("/{0}/events", _PageId);
                var json = fb.Post(postUrl, parameters).ToString();

                if (json.Contains("error"))
                {
                    var errorLabel = new Label();
                    var root = JObject.Parse(json);
                    errorLabel.Content = string.Format("Error creating event '{0}' --> ", name) + root["error"]["message"];
                    ResultLinks.Children.Add(errorLabel);
                }
                else
                {
                    var root = JObject.Parse(json);
                    var eventId = root["id"];

                    var linkButton = new Button();
                    linkButton.Width = 100;
                    linkButton.Content = name;
                    linkButton.HorizontalAlignment = HorizontalAlignment.Left;
                    linkButton.Click += (s, args) =>
                    {
                        var eventUrl = string.Format("https://www.facebook.com/events/{0}/", eventId);

                        //Open a browser window with the event URL
                        System.Diagnostics.Process.Start(eventUrl);
                    };
                    ResultLinks.Children.Add(linkButton);
                }
            }
        }

        #endregion

        #region MenuItems

        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }


        private void Settings_OnClick(object sender, RoutedEventArgs e)
        {
            var settingsForm = new Settings(SaveSettingsCallback);
            settingsForm.ShowDialog();
        }

        #endregion

        #endregion

        #region Methods

        private void LoginCallback(string token)
        {
            _AccessToken = token;

            var fb = new FacebookClient(token);
            var json = fb.Get("/me/accounts/").ToString();
            var root = JObject.Parse(json);

            var pageAccessToken =
                root["data"].Children().FirstOrDefault(x => x["id"].ToString() == _PageId)["access_token"].ToString();

            if (!string.IsNullOrEmpty(pageAccessToken))
            {
                _PageAccessToken = pageAccessToken;
            }
            else
                MessageBox.Show("Unable to find the specified Page ID! I can't create any events for this.", "Doh!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void SaveSettingsCallback(string appId, string pageId, string domainUrl)
        {
            _AppId = appId;
            _PageId = pageId;
            _DomainUrl = domainUrl;
        }

        #endregion
    }
}
