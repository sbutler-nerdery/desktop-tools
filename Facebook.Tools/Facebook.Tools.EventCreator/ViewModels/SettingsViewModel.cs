using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Facebook.Tools.EventCreator.ViewModels
{
    /// <summary>
    /// This is the view model for the settings window
    /// </summary>
    public class SettingsViewModel : BaseViewModel, IDataErrorInfo
    {
        #region Fields

        private string _AppId;
        private string _DomainUrl;
        private string _PageId;
        private bool _CanSaveChanges;
        private Dictionary<string, bool> _ValidationErrorList = new Dictionary<string, bool>();

        #endregion

        #region Properties

        /// <summary>
        /// Get or set the Facebook App Id
        /// </summary>
        public string AppId
        {
            get { return _AppId; }
            set
            {
                _AppId = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Get or set the Facebook Domain Url
        /// </summary>
        public string DomainUrl
        {
            get { return _DomainUrl; }
            set
            {
                _DomainUrl = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Get or set the Facebook Page Id
        /// </summary>
        public string PageId
        {
            get { return _PageId; }
            set
            {
                _PageId = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Get or set whether or not the settings can be saved
        /// </summary>
        public bool CanSaveChanges
        {
            get { return _CanSaveChanges; }
            set
            {
                _CanSaveChanges = value;
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

                CanSaveChanges = true;

                if (!_ValidationErrorList.ContainsKey(columnName))
                    _ValidationErrorList.Add(columnName, isValid);
                else
                    _ValidationErrorList[columnName] = isValid;

                if (columnName == "AppId")
                {
                    if (string.IsNullOrEmpty(AppId))
                    {
                        result = "The Facebook AppId cannot be empty";
                        isValid = false;
                    }
                }

                if (columnName == "DomainUrl")
                {
                    if (string.IsNullOrEmpty(DomainUrl))
                    {
                        result = "The Facebook domain Url cannot be empty";
                        isValid = false;
                    }
                }

                if (columnName == "PageId")
                {
                    if (string.IsNullOrEmpty(AppId))
                    {
                        result = "The Facebook PageId cannot be empty";
                        isValid = false;
                    }
                }

                _ValidationErrorList[columnName] = isValid;

                //Make sure that no changes can be saved if there are any errors being returned.
                var isValidationError = _ValidationErrorList.Values.Count(x => x == false);
                if (isValidationError > 0)
                    CanSaveChanges = false;

                return result;
            }
        }

        //Not used by WPF
        public string Error { get { throw new NotImplementedException(); } }

        #endregion

        #endregion

    }
}
