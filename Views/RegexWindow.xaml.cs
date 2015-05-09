using System.Windows;
using System.Windows.Input;
using SearchSharp.ViewModels;

namespace SearchSharp.Views
{
    /// <summary>
    /// Interaction logic for RegexWindow.xaml
    /// </summary>
    public partial class RegexWindow : Window
    {
        private readonly RegexWindowViewModel _viewModel;

        public RegexWindow()
            : this(null, null)
        {
        }

        public RegexWindow(RegexWindowViewModel viewModel, Window owner)
        {
            InitializeComponent();
            DataContext = _viewModel = viewModel;
            Owner = owner;
            Background = Owner.Background;
            Icon = Owner.Icon;
        }

        private void _cancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void _applyButton_OnClick(object sender, RoutedEventArgs e)
        {
            _viewModel.Apply = true;
            Close();
        }

        private void _cancelInputLink_OnClick(object sender, RoutedEventArgs e)
        {
            _viewModel.CancelInput();
        }

        private void _setInputLink_OnClick(object sender, RoutedEventArgs e)
        {
            _viewModel.SettingInput = true;
        }

        private void _useInputLink_OnClick(object sender, RoutedEventArgs e)
        {
            _viewModel.UseInput();
        }

        private void RegexWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
