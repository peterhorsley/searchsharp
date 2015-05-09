using System.ComponentModel;

namespace SearchSharp.ViewModels
{
    public class ViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Raises the property changed event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
