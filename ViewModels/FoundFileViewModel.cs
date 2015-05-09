using System;
using System.Diagnostics;
using System.IO;

namespace SearchSharp.ViewModels
{
    public class FoundFileViewModel : ViewModel
    {
        private string _path;
        private long _sizeInBytes = -1;
        private DateTime _dateModified;

        public FoundFileViewModel(string filePath)
        {
            _path = filePath;
            try
            {
                _sizeInBytes = new FileInfo(_path).Length;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(String.Format("Failed to read size of file '{0}' - {1}", _path, ex.Message));
            }
            
            _dateModified = File.GetLastWriteTime(_path);
        }

        public string FilePath
        {
            get { return _path; }
        }

        public string FileName
        {
            get { return Path.GetFileName(_path); }
        }

        public string FileExtension
        {
            get { return Path.GetExtension(_path); }
        }

        public DateTime DateModified
        {
            get { return _dateModified; }
        }

        public long SizeInBytes
        {
            get { return _sizeInBytes; }
        }
    }
}
