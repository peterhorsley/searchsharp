using System;
using System.Text;

namespace SearchSharp.ViewModels
{
    public class FileContentViewModel : ViewModel
    {
        private string _lineNumbers;
        private string _selectedFileContent = "";
        private FileContentSearchParameters _fileContentSearchParams;
        private int _selectedFileCount;
        private bool _selectedFileTooBig;

        public FileContentViewModel()
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
            var contentWithJustLineFeed = content.Replace("\r\n", "\n");
            var lines = contentWithJustLineFeed.Split(new char[] { '\n' });
            var lineNumberBuilder = new StringBuilder();
            for (int i = 1; i <= lines.Length; i++)
            {
                lineNumberBuilder.AppendLine(i.ToString());
            }
            return lineNumberBuilder.ToString();
        }

        public int SelectedFileCount
        {
            get { return _selectedFileCount; }
            set
            {
                _selectedFileCount = value;
                RaisePropertyChanged("SelectedFileCount");
            }
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
    }
}
