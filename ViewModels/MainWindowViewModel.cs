using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace SearchSharp.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        private string _searchPath = "";
        private string _fileSpec = "";
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
        private Regex _compiledFileSpecRegex;
        private Regex _compiledContainingTextRegex;
        private BackgroundWorker _textContentThread;

        public MainWindowViewModel()
        {
            // Only one argument is supported - the path in which to search.
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                var dir = args[1].Trim(new char[] { '"', '\\' }) + "\\";
                if (Directory.Exists(dir))
                {
                    _searchPath = dir;
                }
            }

            FoundFiles = new ObservableCollection<FoundFileViewModel>();
            TextContentViewModel = new TextContentViewModel();
            BinaryContentViewModel = new BinaryContentViewModel();
            SearchCommand = new ActionCommand(RunSearch);
            mSelectedFiles = new List<FoundFileViewModel>();
            _executor = new SearchExecutor(this);
            _worker = new BackgroundWorker { WorkerSupportsCancellation = true };
            _worker.DoWork += worker_DoWork;
            _worker.RunWorkerCompleted += worker_RunWorkerCompleted;
        }

        public TextContentViewModel TextContentViewModel { get; private set; }
        public BinaryContentViewModel BinaryContentViewModel { get; private set; }

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
        
        public bool IsValidFileSpec
        {
            get
            {
                if (FileSpecRegex)
                {
                    return RegexTester.IsValid(FileSpec);
                }
                return true;
            }
        }

        public Regex CompiledFileSpecRegex
        {
            get { return _compiledFileSpecRegex; }
        }

        public bool IsValidSearchPath
        {
            get
            {
                if (String.IsNullOrWhiteSpace(SearchPath))
                {
                    return false;
                }
                return Directory.Exists(SearchPath);
            }
        }
        
        public bool IsValidContainingText
        {
            get
            {
                if (ContainingTextRegex)
                {
                    return RegexTester.IsValid(ContainingText);
                }
                return true;
            }
        }

        public string SearchPath
        {
            get { return _searchPath; }
            set
            {
                _searchPath = value;
                RaisePropertyChanged("SearchPath");
                RaisePropertyChanged("IsValidSearchPath");
                RaisePropertyChanged("SearchInputsValid");
            }
        }

        public string FileSpec
        {
            get { return _fileSpec; }
            set
            {
                _fileSpec = value;
                RaisePropertyChanged("FileSpec");
                RaisePropertyChanged("IsValidFileSpec");
                RaisePropertyChanged("SearchInputsValid");
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
                RaisePropertyChanged("IsValidFileSpec");
                RaisePropertyChanged("SearchInputsValid");
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
                RaisePropertyChanged("IsValidContainingText");
                RaisePropertyChanged("SearchInputsValid");
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

        public bool SearchInputsValid
        {
            get { return IsValidFileSpec && IsValidContainingText && IsValidSearchPath; }
        }

        public Regex CompiledContainingTextRegex
        {
            get { return _compiledContainingTextRegex; }
        }

        public bool ContainingTextRegex
        {
            get { return _containingTextRegex; }
            set
            {
                _containingTextRegex = value;
                RaisePropertyChanged("ContainingTextRegex");
                RaisePropertyChanged("ContainingTextRegexToolTip");
                RaisePropertyChanged("IsValidContainingText");
                RaisePropertyChanged("SearchInputsValid");
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

            CreateFilenameRegex();
            CreateContentRegex();

            TextContentViewModel.ExecutedFileContentSearchParameters = new FileContentSearchParameters(
                ContainingText, 
                ContainingTextMatchCase, 
                ContainingTextNot, 
                ContainingTextRegex, 
                ContainingTextRegexOptions,
                TextContentViewModel);

            _worker.RunWorkerAsync();
        }

        private void CreateContentRegex()
        {
            RegexOptions options = ContainingTextMatchCase ? RegexOptions.Compiled : RegexOptions.Compiled | RegexOptions.IgnoreCase;
            options |= ContainingTextRegexOptions;
            _compiledContainingTextRegex = null;
            try
            {
                _compiledContainingTextRegex = new Regex(ContainingText, options);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(String.Format("Invalid regex pattern '{0}' - {1}", ContainingText, ex.Message));
            }
        }

        private void CreateFilenameRegex()
        {
            _compiledFileSpecRegex = null;
            RegexOptions options = FileSpecMatchCase ? RegexOptions.Compiled : RegexOptions.Compiled | RegexOptions.IgnoreCase;
            if (FileSpecRegex)
            {
                options |= FileSpecRegexOptions;
                try
                {
                    _compiledFileSpecRegex = new Regex(FileSpec, options);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(String.Format("Invalid regex pattern '{0}' - {1}", FileSpecRegex, ex.Message));
                }
            }
            else
            {
                try
                {
                    _compiledFileSpecRegex = FindFilesPatternToRegex.Convert(FileSpec, options);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(String.Format("Invalid wildcard pattern '{0}' - {1}", FileSpecRegex, ex.Message));
                }
            }
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
            UpdateBinaryContent();
            UpdateTextContent();
        }

        private void UpdateTextContent()
        {
            if (SelectedFiles.Count == 1)
            {
                var file = SelectedFiles.First();
                TextContentViewModel.SelectedFileTooBig = (file.SizeInBytes > _maxFileSizeInBytesToShowContent);

                if (!TextContentViewModel.SelectedFileTooBig)
                {
                    if (_textContentThread != null)
                    {
                        _textContentThread.CancelAsync();
                    }

                    _textContentThread = new BackgroundWorker() {WorkerSupportsCancellation = true};
                    _textContentThread.DoWork += textContentThread_DoWork;
                    _textContentThread.RunWorkerAsync(new object[] { _textContentThread, file.FilePath });
                    _textContentThread.RunWorkerCompleted += textContentThread_RunWorkerCompleted;
                }
            }
            else
            {
                TextContentViewModel.LineNumbers = String.Empty;
            }

            TextContentViewModel.SelectedFileCount = SelectedFiles.Count;
        }

        void textContentThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _textContentThread = null;
        }

        void textContentThread_DoWork(object sender, DoWorkEventArgs e)
        {
            var args = (object[]) e.Argument;
            var worker = (BackgroundWorker)args[0];
            var file = (string)args[1];

            if (worker.CancellationPending)
            {
                return;
            }

            TextContentViewModel.IsBinary = IsFileProbablyBinary(file, worker);

            if (worker.CancellationPending)
            {
                return;
            }

            if (!TextContentViewModel.IsBinary)
            {
                var content = File.ReadAllText(file);

                if (worker.CancellationPending)
                {
                    return;
                }

                TextContentViewModel.SelectedFileContent = content;
            }
        }

        private bool IsFileProbablyBinary(string file, BackgroundWorker worker)
        {
            using (var stream = File.OpenRead(file))
            {
                var data = 0;
                while (data != -1)
                {
                    data = stream.ReadByte();
                    if (data == 0)
                    {
                        return true;
                    }
                    if (worker.CancellationPending)
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        private void UpdateBinaryContent()
        {
            if (SelectedFiles.Count == 1)
            {
                var file = SelectedFiles.First();
                BinaryContentViewModel.LoadFile(file.FilePath);
            }
            else
            {
                BinaryContentViewModel.CloseFile();
            }

            BinaryContentViewModel.SelectedFileCount = SelectedFiles.Count;
        }
    }
}
