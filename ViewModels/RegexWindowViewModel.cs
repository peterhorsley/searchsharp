using System.Text.RegularExpressions;
using System;

namespace SearchSharp.ViewModels
{
    public class RegexWindowViewModel : ViewModel
    {
        private string _regexString;
        private bool _multiLine;
        private bool _singleLine;
        private bool _settingInput;
        private string _inputString;

        public RegexWindowViewModel()
        {
            TitleText = "Regex Options";
            TextContentViewModel = new TextContentViewModel() { SelectedFileCount = 1, ShowFullContent = true };
        }

        public bool Apply { get; set; }

        public TextContentViewModel TextContentViewModel { get; private set; }

        public void UseInput()
        {
            TextContentViewModel.SelectedFileContent = InputString;
            SettingInput = false;
        }

        public void CancelInput()
        {
            SettingInput = false;
        }

        public bool SettingInput
        {
            get { return _settingInput; }
            set
            {
                _settingInput = value;
                RaisePropertyChanged("SettingInput");
            }
        }

        public string InputString
        {
            get { return _inputString; }
            set
            {
                _inputString = value;
                RaisePropertyChanged("InputString");
            }
        }

        public string RegexString
        {
            get { return _regexString; }
            set
            {
                _regexString = value; 
                RaisePropertyChanged("RegexString");
                RaisePropertyChanged("RegexIsValid");
                UpdateTestContent();
            }
        }

        public bool RegexIsValid
        {
            get { return RegexTester.IsValid(RegexString); }
        }

        private void UpdateTestContent()
        {
            TextContentViewModel.ExecutedFileContentSearchParameters = new FileContentSearchParameters(RegexString, false, false, true, Options, TextContentViewModel);
        }

        public bool MultiLine
        {
            get { return _multiLine; }
            set
            {
                _multiLine = value; 
                RaisePropertyChanged("MultiLine");
                UpdateTestContent();
            }
        }

        public bool SingleLine
        {
            get { return _singleLine; }
            set
            {
                _singleLine = value; 
                RaisePropertyChanged("SingleLine");
                UpdateTestContent();
            }
        }

        public RegexOptions Options
        {
            get
            {
                RegexOptions options = RegexOptions.None;

                if (MultiLine)
                {
                    options |= RegexOptions.Multiline;
                }

                if (SingleLine)
                {
                    options |= RegexOptions.Singleline;
                }
                return options;
            }
            set
            {
                MultiLine = (value & RegexOptions.Multiline) == RegexOptions.Multiline;
                SingleLine = (value & RegexOptions.Singleline) == RegexOptions.Singleline;
            }
        }

        public string TitleText { get; set; }

        public void CopyAsCSharp()
        {
            var options = "";
            const string multiLine = "RegexOptions.Multiline";
            const string singleLine = "RegexOptions.Singleline";
            if (MultiLine && SingleLine)
            {
                options = String.Format(", {0} | {1}", multiLine, singleLine);
            }
            else if (MultiLine)
            {
                options = ", " + multiLine;
            }
            else if (SingleLine)
            {
                options = ", " + singleLine;
            }

            var csharp = String.Format("var regex = new Regex(@\"{0}\"{1});", RegexString, options);
            System.Windows.Clipboard.SetDataObject(csharp);
        }
    }
}