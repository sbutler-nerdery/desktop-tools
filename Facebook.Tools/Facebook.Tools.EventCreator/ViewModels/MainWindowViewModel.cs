namespace Facebook.Tools.EventCreator.ViewModels
{
    /// <summary>
    /// This is the view model for the main window
    /// </summary>
    public class MainWindowViewModel : BaseViewModel
    {
        #region Fields

        private string _CsvFileLocation;

        #endregion

        #region Properties

        /// <summary>
        /// Get or set the location of the CSV file the user is importing from
        /// </summary>
        public string CsvFileLocation
        {
            get { return _CsvFileLocation; }
            set
            {
                _CsvFileLocation = value;
                OnPropertyChanged();
            }
        }

        #endregion        
    }
}
