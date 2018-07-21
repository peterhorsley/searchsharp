using System;
using System.Text;
using System.Collections.Generic;

namespace SearchSharp.ViewModels
{
    public class TextContentViewModel : ContentViewModel
    {
        private string _lineNumbers;
        private string _selectedFileContent = "";
        private FileContentSearchParameters _fileContentSearchParams;
        private bool _selectedFileTooBig;
        private bool _showFullContent = false;
        private int _linesOfContext = 2;
        private bool _isBinary;

        public TextContentViewModel()
        {
            ScrollGroupId = Guid.NewGuid().ToString();

            IncreaseLinesOfContextCommand = new DelegateCommand(
                () => LinesOfContext++,
                () => LinesOfContext < 10);

            DecreaseLinesOfContextCommand = new DelegateCommand(
                () => { LinesOfContext = Math.Max(0, LinesOfContext - 1); },
                () => LinesOfContext > 0);
        }

        public DelegateCommand IncreaseLinesOfContextCommand { get; private set; }
        public DelegateCommand DecreaseLinesOfContextCommand { get; private set; }

        public int LinesOfContext
        {
            get { return _linesOfContext; }
            set 
            {
                _linesOfContext = value; 
                RaisePropertyChanged("LinesOfContext");
            }
        }

        public String ScrollGroupId { get; private set; }

        public string SelectedFileContent
        {
            get { return _selectedFileContent; }
            set
            {
                _selectedFileContent = value;
                LineNumbers = GetLineNumbersForText(value);
                RaisePropertyChanged("SelectedFileContent");
            }
        }

        private string GetLineNumbersForText(string content)
        {
            var lines = SplitStringIntoLines(content);
            var lineNumberBuilder = new StringBuilder();
            for (int i = 1; i <= lines.Length; i++)
            {
                lineNumberBuilder.AppendLine(i.ToString());
            }
            return lineNumberBuilder.ToString();
        }

        public static string[] SplitStringIntoLines(string content)
        {
            if (content == null)
            {
                return new string[0];
            }
            var contentWithJustLineFeed = content.Replace("\r\n", "\n");
            return contentWithJustLineFeed.Split(new char[] {'\n'});
        }

        public FileContentSearchParameters ExecutedFileContentSearchParameters
        {
            get
            {
                return _fileContentSearchParams;
            }
            set
            {
                _fileContentSearchParams = value;
                RaisePropertyChanged("ExecutedFileContentSearchParameters");
                RaisePropertyChanged("LinesOfContextAvailable");
            }
        }

        public bool ShowFullContent
        {
            get { return _showFullContent; }
            set
            {
                _showFullContent = value;
                RaisePropertyChanged("ShowFullContent");
                RaisePropertyChanged("LinesOfContextAvailable");
            }
        }

        public string LineNumbers
        {
            get { return _lineNumbers; }
            set
            {
                _lineNumbers = value;
                RaisePropertyChanged("LineNumbers");
            }
        }

        public bool SelectedFileTooBig
        {
            get { return _selectedFileTooBig; }
            set
            {
                _selectedFileTooBig = value;
                RaisePropertyChanged("SelectedFileTooBig");
            }
        }

        public void SetLineNumbers(List<string> lineNumbers)
        {
            var lineNumberBuilder = new StringBuilder();
            foreach (var entry in lineNumbers)
            {
                lineNumberBuilder.AppendLine(entry);
            }

            LineNumbers = lineNumberBuilder.ToString();
        }

        public void SetLineNumbers()
        {
            LineNumbers = GetLineNumbersForText(SelectedFileContent);
        }

        public bool ContainingTextSpecified
        {
            get { return ExecutedFileContentSearchParameters != null && 
                !String.IsNullOrEmpty(ExecutedFileContentSearchParameters.ContainingText) && 
                !ExecutedFileContentSearchParameters.ContainingTextNot; }
        }

        public bool LinesOfContextAvailable
        {
            get { return ContainingTextSpecifiedAndFileSelected && !ShowFullContent; }
        }

        public bool ContainingTextSpecifiedAndFileSelected
        {
            get { return ContainingTextSpecified && SingleFileSelected; }
        }

        public bool IsBinary
        {
            get { return _isBinary; }
            set 
            {
                if (value != _isBinary)
                {
                    _isBinary = value;
                    RaisePropertyChanged("IsBinary");
                }
            }
        }

        protected override void OnSelectedFileCountChanged()
        {
            base.OnSelectedFileCountChanged();
            RaisePropertyChanged("ContainingTextSpecified");
            RaisePropertyChanged("ContainingTextSpecifiedAndFileSelected");
            RaisePropertyChanged("LinesOfContextAvailable");
        }
    }
}
