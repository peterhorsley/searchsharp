using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Be.Windows.Forms;

namespace SearchSharp.ViewModels
{
    public class BinaryContentViewModel : ContentViewModel
    {
        private DynamicFileByteProvider _byteProvider;

        public DynamicFileByteProvider ByteProvider
        {
            get { return _byteProvider; }
            set
            {
                _byteProvider = value; 
                RaisePropertyChanged("ByteProvider");
            }
        }

        public void LoadFile(string filePath)
        {
            try
            {
                CloseFile();

                // try to open in read-only mode
                ByteProvider = new DynamicFileByteProvider(filePath, true);
                RaiseFileChangedEvent();
            }
            catch (IOException) { }
        }

        private void RaiseFileChangedEvent()
        {
            var handler = FileChanged;
            if (handler != null)
            {
                handler(this, null);
            }
        }

        public void CloseFile()
        {
            if (ByteProvider != null)
            {
                ByteProvider.Dispose();
            }
        }

        public event EventHandler FileChanged;
    }
}
