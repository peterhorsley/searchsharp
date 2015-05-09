using System.Text.RegularExpressions;

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
            ContentViewModel = new FileContentViewModel() { SelectedFileCount = 1 };
        }

        public bool Apply { get; set; }

        public FileContentViewModel ContentViewModel { get; private set; }

        public void UseInput()
        {
            ContentViewModel.SelectedFileContent = InputString;
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
                UpdateTestContent();
            }
        }

        private void UpdateTestContent()
        {
            ContentViewModel.ExecutedFileContentSearchParameters = new FileContentSearchParameters(RegexString, false, false, true, Options);
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
    }
}