using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
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
using Facebook.Tools.EventCreator.ViewModels;
using Microsoft.Win32;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Newtonsoft.Json.Linq;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using VerticalAlignment = System.Windows.VerticalAlignment;

namespace Facebook.Tools.EventCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        private const string URL_TEMPLATE = "https://www.facebook.com/dialog/oauth?client_id={0}&scope=create_event,manage_pages&redirect_uri={1}/somepage.html&response_type=token";
        private string _LoginUrl;
        private string _AccessToken;
        private string _PageAccessToken;
        private string _DomainUrl;
        private string _AppId;
        private string _PageId;
        private string _Data;
        private Login _LoginForm;
        private MainWindowViewModel _ViewModel;

        #endregion

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Event Handlers

        #region Window

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            //Get setting from the app config
            _AppId = Properties.Settings.Default.AppId;
            _DomainUrl = Properties.Settings.Default.DomainUrl;
            _PageId = Properties.Settings.Default.PageId;
            _LoginUrl = string.Format(URL_TEMPLATE, _AppId, _DomainUrl);
            _LoginForm = new Login(LoginCallback, _LoginUrl , "");

            //Plugin the view model
            _ViewModel = new MainWindowViewModel { CsvFileLocation = string.Empty };
            DataContext = _ViewModel;

            //Make sure that there are settings in place
            if (string.IsNullOrEmpty(_AppId) || string.IsNullOrEmpty(_DomainUrl) || string.IsNullOrEmpty(_PageId))
            {
                var settingsForm = new Settings(SaveSettingsCallback);
                settingsForm.ShowDialog();
            }
            else
            {
                //Make sure that the user is logged in.
                _LoginForm.LoginUser();
                _LoginForm.ShowDialog();
            }
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        #endregion

        #region Buttons

        private void BrowseForFiles_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv",
                    FilterIndex = 1
                };

            if (dialog.ShowDialog() == true)
            {
                ImportFilePath.Text = dialog.FileName;
            }
        }

        private void CreateEvents_OnClick(object sender, RoutedEventArgs e)
        {
            //Open the specified file and get the contents
            try
            {
                var file = new FileInfo(ImportFilePath.Text);

                if (file.Extension == ".csv")
                {
                    using (var fs = File.OpenRead(ImportFilePath.Text))
                    {
                        using (var reader = new StreamReader(fs))
                        {
                            _Data = reader.ReadToEnd();
                        }
                    }
                }
                else if (file.Extension == ".xlsx")
                {
                    XSSFWorkbook workbook;

                    using (var fs = File.OpenRead(ImportFilePath.Text))
                    {
                        workbook = new XSSFWorkbook(fs);
                    }

                    var sheet = workbook.GetSheetAt(0);
                    var tempData = "";

                    //Assume that the first row has the names of the columns
                    for (int i = 1; i <= sheet.LastRowNum; i++)
                    {
                        if (sheet.GetRow(i) != null)
                        {
                            var row = sheet.GetRow(i);
                            var rowString = "";

                            for (int j = 0; j < row.LastCellNum; j++)
                            {
                                string cellData = "";
                                var cell = row.Cells[j];

                                //This is the data type for date? Oh well... 
                                if (cell.CellType == CellType.NUMERIC)
                                {
                                    var dateTime = cell.DateCellValue;
                                    cellData = dateTime.ToString("yyyy-MM-ddTHH:mm:ss-0500");
                                }
                                else
                                    cellData = row.Cells[j].StringCellValue;

                                rowString += (rowString == "") ? cellData : "," + cellData;
                            }

                            rowString += Environment.NewLine;
                            tempData += rowString;
                        }
                    }

                    _Data = tempData;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("I tried reading the contents of the file and I couldn't.", "Doh!", MessageBoxButton.OK,
                                MessageBoxImage.Error);
                Logger.LogInfo(ex.Message + Environment.NewLine + ex.StackTrace);
                return;
            }

            if (_Data == "")
            {
                MessageBox.Show("I can't create events out of nothing! The contents of the specified file appear to be blank.", "Doh!", MessageBoxButton.OK,
                                MessageBoxImage.Exclamation);
                return;
            }

            var events = _Data.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            //Clear the old event links..
            ResultLinks.Children.Clear();

            Progress.Maximum = events.Length;
            Progress.Value = 0;

            var worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            worker.DoWork += (o, wArgs) =>
                {
                    var thisWorker = o as BackgroundWorker;

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
                        var state = new WorkerState();

                        try
                        {
                            var json = fb.Post(postUrl, parameters).ToString();
                            var root = JObject.Parse(json);
                            var eventId = root["id"];
                            state.Error = false;
                            state.Name = name;
                            state.Id = eventId.ToString();

                            thisWorker.ReportProgress(1, state);
                        }
                        catch (Exception ex)
                        {
                            state.Error = true;
                            state.Name = string.Format("Error creating event '{0}' --> ", name) + ex.Message;

                            thisWorker.ReportProgress(1, state);
                            Logger.LogInfo(ex.Message + Environment.NewLine + ex.StackTrace);
                        }
                    }
                };

            worker.ProgressChanged += (s, wArgs) =>
                {
                    Progress.Value += 1;

                    var state = wArgs.UserState as WorkerState;

                    if (state.Error)
                    {
                        var errorLabel = new Label();
                        errorLabel.Margin = new Thickness(0, 2, 0, 2);
                        errorLabel.Content = state.Name;
                        ResultLinks.Children.Add(errorLabel);
                    }
                    else
                    {
                        var linkButton = new Button();
                        linkButton.Margin = new Thickness(0, 2, 0, 2);
                        linkButton.Content = string.Format("Open event '{0}'", state.Name);
                        linkButton.HorizontalAlignment = HorizontalAlignment.Left;
                        linkButton.Click += (snd, args) =>
                        {
                            var eventUrl = string.Format("https://www.facebook.com/events/{0}/", state.Id);

                            //Open a browser window with the event URL
                            System.Diagnostics.Process.Start(eventUrl);
                        };
                        ResultLinks.Children.Add(linkButton);
                    }
                };

            worker.RunWorkerCompleted += (s, wArgs) =>
                {
                    Loading.Visibility = Visibility.Collapsed;
                    NewLinks.Visibility = Visibility.Visible;
                    ImportFilePath.Text = string.Empty;
                };

            Loading.Visibility = Visibility.Visible;
            NewLinks.Visibility = Visibility.Collapsed;

            worker.RunWorkerAsync();
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

        private void LoginMenu_OnClick(object sender, RoutedEventArgs e)
        {
            var fb = new FacebookClient();
            var parameters = new Dictionary<string, object>();
            parameters.Add("access_token", _AccessToken);
            parameters.Add("next", _LoginUrl);
            var logoutUrl = fb.GetLogoutUrl(parameters).ToString();

            if (_ViewModel.IsAuthenticated)
            {
                _ViewModel.IsAuthenticated = false;

                _AppId = Properties.Settings.Default.AppId;
                _DomainUrl = Properties.Settings.Default.DomainUrl;
                _PageId = Properties.Settings.Default.PageId;
                _LoginForm = new Login(LoginCallback, _LoginUrl, logoutUrl);
                _LoginForm.LogoutUser();
                _LoginForm.ShowDialog();
            }
            else
            {
                _AppId = Properties.Settings.Default.AppId;
                _DomainUrl = Properties.Settings.Default.DomainUrl;
                _PageId = Properties.Settings.Default.PageId;
                _LoginForm = new Login(LoginCallback, _LoginUrl, "");
                _LoginForm.LoginUser();
                _LoginForm.ShowDialog();
            }

            LoginMenu.Header = _ViewModel.IsAuthenticated ? "Logout" : "Login";
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

            try
            {
                var pageAccessToken =
                    root["data"].Children().FirstOrDefault(x => x["id"].ToString() == _PageId)["access_token"].ToString();

                if (!string.IsNullOrEmpty(pageAccessToken))
                {
                    _PageAccessToken = pageAccessToken;
                    _ViewModel.IsAuthenticated = true;
                }
                else
                    ShowErrorDialog("Unable to find the specified Page ID! I can't create any events for this.");
            }
            catch (Exception ex)
            {
                Logger.LogInfo(string.Format(ex.Message + Environment.NewLine + ex.StackTrace));
                ShowErrorDialog("You do not have permission to manage pages.");
            }

            LoginMenu.Header = _ViewModel.IsAuthenticated ? "Logout" : "Login";
        }

        private void SaveSettingsCallback(string appId, string pageId, string domainUrl)
        {
            _AppId = appId;
            _PageId = pageId;
            _DomainUrl = domainUrl;
            _LoginUrl = string.Format(URL_TEMPLATE, _AppId, _DomainUrl);
        }

        private void ShowErrorDialog(string message, string title = "Doh!")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion

        #region Helpers

        public class WorkerState
        {
            public string Name { get; set; }
            public string Id { get; set; }
            public bool Error { get; set; }
        }

        #endregion
    }
}
