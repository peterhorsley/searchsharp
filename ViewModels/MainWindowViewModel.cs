using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace SearchSharp.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        private string _searchPath = "c:\\code";
        private string _fileSpec = "*.cs";
        private string _containingText = "";
        private bool _firstSearchStarted;
        private List<FoundFileViewModel> mSelectedFiles;
        private bool _recurse = true;
        private SearchExecutor _executor;
        private bool _fileSpecMatchCase;
        private bool _fileSpecRegex;
        private bool _fileSpecNot;
        private bool _containingTextMatchCase;
        private bool _containingTextRegex;
        private bool _containingTextNot;
        private const string _matchCaseToolTip = "Match case";
        private const string _regexToolTip = "Regular expression (.NET syntax)";
        private const string _notExpressionToolTip = "Not expression";
        private BackgroundWorker _worker;
        private bool _searching;
        private long _maxFileSizeInBytesToShowContent = 1000000 * 250; // 250MB

        public MainWindowViewModel()
        {
            FoundFiles = new ObservableCollection<FoundFileViewModel>();
            ContentViewModel = new FileContentViewModel();
            SearchCommand = new ActionCommand(RunSearch);
            mSelectedFiles = new List<FoundFileViewModel>();
            _executor = new SearchExecutor(this);
            _worker = new BackgroundWorker { WorkerSupportsCancellation = true };
            _worker.DoWork += worker_DoWork;
            _worker.RunWorkerCompleted += worker_RunWorkerCompleted;
        }

        public FileContentViewModel ContentViewModel { get; private set; }

        public bool Recurse
        {
            get { return _recurse; }
            set
            {
                _recurse = value;
                RaisePropertyChanged("RecurseToolTip");
            }
        }

        public string RecurseToolTip
        {
            get { return String.Format("Include subfolders ({0})", GetEnabledDisabledText(Recurse)); }
        }

        private string GetEnabledDisabledText(bool enabled)
        {
            return enabled ? "enabled" : "disabled";
        }

        public string SearchPath
        {
            get { return _searchPath; }
            set
            {
                _searchPath = value;
                RaisePropertyChanged("SearchPath");
            }
        }

        public string FileSpec
        {
            get { return _fileSpec; }
            set
            {
                _fileSpec = value;
                RaisePropertyChanged("FileSpec");
            }
        }

        public bool FileSpecMatchCase
        {
            get { return _fileSpecMatchCase; }
            set
            {
                _fileSpecMatchCase = value;
                RaisePropertyChanged("FileSpecMatchCaseToolTip");
            }
        }

        public bool FileSpecRegex
        {
            get { return _fileSpecRegex; }
            set
            {
                _fileSpecRegex = value;
                RaisePropertyChanged("FileSpecRegex");
                RaisePropertyChanged("FileSpecRegexToolTip");
            }
        }

        public RegexOptions FileSpecRegexOptions { get; set; }

        public bool FileSpecNot
        {
            get { return _fileSpecNot; }
            set
            {
                _fileSpecNot = value;
                RaisePropertyChanged("FileSpecNotToolTip");
            }
        }

        public string FileSpecMatchCaseToolTip
        {
            get { return String.Format("{0} ({1})", _matchCaseToolTip, GetEnabledDisabledText(FileSpecMatchCase)); }
        }

        public string FileSpecRegexToolTip
        {
            get { return String.Format("{0} ({1})", _regexToolTip, GetEnabledDisabledText(FileSpecRegex)); }
        }

        public string FileSpecNotToolTip
        {
            get { return String.Format("{0} ({1})", _notExpressionToolTip, GetEnabledDisabledText(FileSpecNot)); }
        }

        public string ContainingText
        {
            get { return _containingText; }
            set
            {
                _containingText = value;
                RaisePropertyChanged("ContainingText");
            }
        }

        public bool ContainingTextMatchCase
        {
            get { return _containingTextMatchCase; }
            set
            {
                _containingTextMatchCase = value;
                RaisePropertyChanged("ContainingTextMatchCaseToolTip");
            }
        }

        public bool ContainingTextRegex
        {
            get { return _containingTextRegex; }
            set
            {
                _containingTextRegex = value;
                RaisePropertyChanged("ContainingTextRegex");
                RaisePropertyChanged("ContainingTextRegexToolTip");
            }
        }

        public RegexOptions ContainingTextRegexOptions { get; set; }

        public bool ContainingTextNot
        {
            get { return _containingTextNot; }
            set
            {
                _containingTextNot = value;
                RaisePropertyChanged("ContainingTextNotTip");
            }
        }

        public string ContainingTextMatchCaseToolTip
        {
            get { return String.Format("{0} ({1})", _matchCaseToolTip, GetEnabledDisabledText(ContainingTextMatchCase)); }
        }

        public string ContainingTextRegexToolTip
        {
            get { return String.Format("{0} ({1})", _regexToolTip, GetEnabledDisabledText(ContainingTextRegex)); }
        }

        public string ContainingTextNotTip
        {
            get { return String.Format("{0} ({1})", _notExpressionToolTip, GetEnabledDisabledText(ContainingTextNot)); }
        }

        public List<FoundFileViewModel> SelectedFiles
        {
            get { return mSelectedFiles; }
            set { mSelectedFiles = value; }
        }

        public string FoundTotalText
        {
            get { return _firstSearchStarted ? String.Format("{0} files found.", FoundFiles.Count) : ""; }
        }

        public ObservableCollection<FoundFileViewModel> FoundFiles { get; private set; }

        public ICommand SearchCommand { get; private set; }

        private void RunSearch()
        {
            if (Searching)
            {
                // Cancel search
                _worker.CancelAsync();
                return;
            }

            Searching = true;
            FoundFiles.Clear();
            if (!_firstSearchStarted)
            {
                _firstSearchStarted = true;    
            }
            else
            {
                RaisePropertyChanged("FoundTotalText");
            }

            ContentViewModel.ExecutedFileContentSearchParameters = new FileContentSearchParameters(
                ContainingText, ContainingTextMatchCase, ContainingTextNot, ContainingTextRegex, ContainingTextRegexOptions);

            _worker.RunWorkerAsync();
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Searching = false;
        }

        public bool NotSearching
        {
            get { return !Searching; }
        }

        public bool Searching
        {
            get { return _searching; }
            set
            {
                _searching = value;
                RaisePropertyChanged("Searching");
                RaisePropertyChanged("NotSearching");
            }
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            _executor.StartSearch((BackgroundWorker)sender);
        }

        private void AddResult(string file)
        {
            FoundFiles.Add(new FoundFileViewModel(file));
            RaisePropertyChanged("FoundTotalText");
        }

        void RunOnUI(Action code)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(code);
        }

        public void AddFileToResult(string filePath)
        {
            RunOnUI(() => AddResult(filePath));
        }

        public void UpdateSelectFileContent()
        {
            if (SelectedFiles.Count == 1)
            {
                var file = SelectedFiles.First();
                ContentViewModel.SelectedFileTooBig = (file.SizeInBytes > _maxFileSizeInBytesToShowContent);

                if (!ContentViewModel.SelectedFileTooBig)
                {
                    var content = File.ReadAllText(file.FilePath);
                    ContentViewModel.SelectedFileContent = content;
                }
            }
            else
            {
                ContentViewModel.LineNumbers = String.Empty;
            }

            ContentViewModel.SelectedFileCount = SelectedFiles.Count;
        }
    }
}
