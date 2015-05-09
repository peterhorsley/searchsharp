using System.Windows.Controls;

namespace SearchSharp.Views
{
    /// <summary>
    /// Interaction logic for FileContentView.xaml
    /// </summary>
    public partial class FileContentView : UserControl
    {
        private string _lastText;

        public FileContentView()
        {
            InitializeComponent();
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            //var textBox = (RichTextBox)sender;
            //string newText = new TextRange(textBox.Document.ContentStart, textBox.Document.ContentEnd).Text.Trim();
            //if (_lastText != newText)
            //{
            //    _lastText = newText;
            //    Timer t = new Timer(state =>
            //    {
            //        System.Windows.Application.Current.Dispatcher.Invoke((Action)(() =>
            //        {
            //            ((FileContentViewModel)DataContext).SelectedFileContent = (string)state;
            //        }));

            //    }, newText, 1000, -1);
            //    //((FileContentViewModel)DataContext).Set = "richText";
            //}
        }
    }
}
