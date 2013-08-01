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
using System.Windows.Shapes;

namespace Facebook.Tools.EventCreator
{
    /// <summary>
    /// Interaction logic for HelpBox.xaml
    /// </summary>
    public partial class HelpBox : Window
    {
        public HelpBox()
        {
            InitializeComponent();
        }

        private void HelpBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            var helpFile = System.IO.Path.Combine(new[] { Environment.CurrentDirectory, "HelpPage.html" });
            Browser.Navigate(helpFile);
        }
    }
}
