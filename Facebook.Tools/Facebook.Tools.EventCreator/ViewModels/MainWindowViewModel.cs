using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Facebook.Tools.EventCreator.ViewModels
{
    /// <summary>
    /// This is the view model for the main window
    /// </summary>
    public class MainWindowViewModel : BaseViewModel, IDataErrorInfo
    {
        #region Fields

        private string _CsvFileLocation;
        private bool _CanCreateEvents;
        private bool _IsAuthenticated;
        private Dictionary<string, bool> _ValidationErrorList = new Dictionary<string, bool>();

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

        /// <summary>
        /// Get or set whether or not events can be created
        /// </summary>
        public bool CanCreateEvents
        {
            get { return _CanCreateEvents; }
            set
            {
                _CanCreateEvents = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Get or set whether or not the user is logged in or not
        /// </summary>
        public bool IsAuthenticated
        {
            get { return _IsAuthenticated; }
            set
            {
                _IsAuthenticated = value;
                OnPropertyChanged();
            }
        }

        #region IDataErrorInfo

        public string this[string columnName]
        {
            get
            {
                string result = null;
                bool isValid = true;

                CanCreateEvents = true;

                if (!_ValidationErrorList.ContainsKey(columnName))
                    _ValidationErrorList.Add(columnName, isValid);
                else
                    _ValidationErrorList[columnName] = isValid;

                if (columnName == "CsvFileLocation")
                {
                    if (string.IsNullOrEmpty(CsvFileLocation))
                    {
                        result = "The file path cannot be empty";
                        isValid = false;
                    }
                }

                _ValidationErrorList[columnName] = isValid;

                //Make sure that no changes can be saved if there are any errors being returned.
                var isValidationError = _ValidationErrorList.Values.Count(x => x == false);
                if (isValidationError > 0)
                    CanCreateEvents = false;

                return result;
            }
        }

        //Not used by WPF
        public string Error { get { throw new NotImplementedException(); } }

        #endregion

        #endregion        
    }
}
