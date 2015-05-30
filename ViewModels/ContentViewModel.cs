using System;
using System.Text;
using System.Collections.Generic;

namespace SearchSharp.ViewModels
{
    public abstract class ContentViewModel : ViewModel
    {
        private int _selectedFileCount;

        public int SelectedFileCount
        {
            get { return _selectedFileCount; }
            set
            {
                _selectedFileCount = value;
                RaisePropertyChanged("SelectedFileCount");
                RaisePropertyChanged("SingleFileSelected");
            }
        }

        public bool SingleFileSelected
        {
            get { return SelectedFileCount == 1; }
        }
    }
}
