using System;
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
using System.Windows.Shapes;
using Facebook.Tools.EventCreator.ViewModels;

namespace Facebook.Tools.EventCreator
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        #region Fields

        //AppId, PathId, DomainUrl
        private Action<string, string, string> _SaveSettings;

        #endregion

        #region Contructors

        public Settings()
        {
            InitializeComponent();

            var viewModel = new SettingsViewModel
                {
                    AppId = Properties.Settings.Default.AppId,
                    PageId = Properties.Settings.Default.PageId,
                    DomainUrl = Properties.Settings.Default.DomainUrl
                };
            DataContext = viewModel;
        }

        public Settings(Action<string, string, string> saveSettings) : this()
        {
            _SaveSettings = saveSettings;
        }

        #endregion

        #region Event Handlers

        #region Buttons

        private void Save_OnClick(object sender, RoutedEventArgs e)
        {
            _SaveSettings(AppId.Text, PageId.Text, DomainUrl.Text);

            //Save the user's settings
            Properties.Settings.Default.AppId = AppId.Text;
            Properties.Settings.Default.PageId = PageId.Text;
            Properties.Settings.Default.DomainUrl = DomainUrl.Text;
            Properties.Settings.Default.Save();

            Close();
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #endregion
    }
}
