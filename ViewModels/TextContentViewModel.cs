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

        public TextContentViewModel()
        {
            ScrollGroupId = Guid.NewGuid().ToString();
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
            }
        }

        public bool ShowFullContent
        {
            get { return _showFullContent; }
            set
            {
                _showFullContent = value;
                RaisePropertyChanged("ShowFullContent");
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
    }
}
