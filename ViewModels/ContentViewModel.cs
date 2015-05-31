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
                if (_selectedFileCount != value)
                {
                    _selectedFileCount = value;
                    RaisePropertyChanged("SelectedFileCount");
                    RaisePropertyChanged("SingleFileSelected");
                    OnSelectedFileCountChanged();
                }
            }
        }

        protected virtual void OnSelectedFileCountChanged()
        {
        }

        public bool SingleFileSelected
        {
            get { return SelectedFileCount == 1; }
        }
    }
}
