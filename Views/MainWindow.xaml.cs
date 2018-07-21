using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SearchSharp.ViewModels;

namespace SearchSharp.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel mViewModel;
        private Brush _transparent = new SolidColorBrush(Colors.Transparent);
        private Brush _lightBlue = new SolidColorBrush(Colors.LightBlue);

        public MainWindow()
        {
            InitializeComponent();
            DataContext = mViewModel = new MainWindowViewModel();
            _searchInTextBox.GotKeyboardFocus += searchInTextBox_GotKeyboardFocus;
            _containingTextTextBox.GotKeyboardFocus += containingTextTextBox_GotKeyboardFocus;
            _fileSpecTextBox.GotKeyboardFocus += fileSpecTextBox_GotKeyboardFocus;
            _searchInTextBox.Focus();
            _searchInTextBox.KeyDown += TextBox_KeyDown;
            _fileSpecTextBox.KeyDown += TextBox_KeyDown;
            _containingTextTextBox.KeyDown += TextBox_KeyDown;
        }

        void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                StartSearch();
            }
        }

        private void StartSearch()
        {
            ClickButton(_searchButton);
        }

        private void ClickButton(Button button)
        {
            var peer = new ButtonAutomationPeer(button);
            var invokeProv = (IInvokeProvider)peer.GetPattern(PatternInterface.Invoke);
            invokeProv.Invoke();
        }

        /// <summary>
        /// Handles the GotKeyboardFocus event of the _fileSpecTextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyboardFocusChangedEventArgs" /> instance containing the event data.</param>
        void fileSpecTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        /// <summary>
        /// Handles the GotKeyboardFocus event of the _containingTextTextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyboardFocusChangedEventArgs" /> instance containing the event data.</param>
        void containingTextTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        /// <summary>
        /// Handles the GotKeyboardFocus event of the _searchInTextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyboardFocusChangedEventArgs" /> instance containing the event data.</param>
        void searchInTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ((TextBox)sender).GotKeyboardFocus -= searchInTextBox_GotKeyboardFocus;
            ((TextBox)sender).SelectAll();
        }

        /// <summary>
        /// Handles the OnMouseRightButtonUp event of the DataGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void DataGrid_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataGrid grid = (DataGrid)sender;
            var dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is DataGridCell))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }
            if (dep == null) return;

            if (dep is DataGridCell)
            {
                DataGridCell cell = dep as DataGridCell;
                cell.Focus();

                while ((dep != null) && !(dep is DataGridRow))
                {
                    dep = VisualTreeHelper.GetParent(dep);
                }
                DataGridRow row = dep as DataGridRow;
                if (!mViewModel.SelectedFiles.Contains(row.DataContext))
                {
                    grid.SelectedItem = row.DataContext;
                }
            }

            var point = e.GetPosition(Application.Current.MainWindow);
            var drawingPoint = new System.Drawing.Point((int)point.X, (int)point.Y + 22);
            var ctxMnu = new ShellContextMenu();
            FileInfo[] arrFI = (from o in mViewModel.SelectedFiles select new FileInfo(o.FilePath)).ToArray();
            ctxMnu.ShowContextMenu(arrFI, drawingPoint); // Not sure why this Y offset is needed.
        }

        private void DataGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mViewModel.SelectedFiles.RemoveAll(o => e.RemovedItems.Contains(o));
            mViewModel.SelectedFiles.AddRange(e.AddedItems.Cast<FoundFileViewModel>());
            mViewModel.UpdateSelectFileContent();
        }

        private void _gridSplitter_OnMouseEnter(object sender, MouseEventArgs e)
        {
            _gridSplitter.Background = new SolidColorBrush(Colors.Silver);
        }

        private void _gridSplitter_OnMouseLeave(object sender, MouseEventArgs e)
        {
            _gridSplitter.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void _gridSplitter_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Grid parent = _gridSplitter.Parent as Grid;
            if (parent == null)
            {
                return;
            }

            if (parent.ColumnDefinitions[2].ActualWidth == 0)
            {
                parent.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                parent.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                parent.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                parent.ColumnDefinitions[2].Width = new GridLength(0);
            }
        }

        private void RegexSettingsButton_OnMouseEnter(object sender, MouseEventArgs e)
        {
            ((Border)sender).BorderBrush = _lightBlue;
        }

        private void RegexSettingsButton_OnMouseLeave(object sender, MouseEventArgs e)
        {
            ((Border)sender).BorderBrush = _transparent;
        }

        private void FileNameRegexSettingsButton_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var regexViewModel = new RegexWindowViewModel
            {
                RegexString = mViewModel.FileSpec, 
                Options = mViewModel.FileSpecRegexOptions,
            };
            var regexWindow = new RegexWindow(regexViewModel, this);
            regexWindow.ShowDialog();
            if (regexViewModel.Apply)
            {
                mViewModel.FileSpec = regexViewModel.RegexString;
                mViewModel.FileSpecRegexOptions = regexViewModel.Options;
            }
        }

        private void ContainingTextRegexSettingsButton_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var regexViewModel = new RegexWindowViewModel
            {
                RegexString = mViewModel.ContainingText,
                Options = mViewModel.ContainingTextRegexOptions,
            };
            var regexWindow = new RegexWindow(regexViewModel, this);
            regexWindow.ShowDialog();
            if (regexViewModel.Apply)
            {
                mViewModel.ContainingText = regexViewModel.RegexString;
                mViewModel.ContainingTextRegexOptions = regexViewModel.Options;
            }
        }

        private void _regexTesterLink_OnClick(object sender, RoutedEventArgs e)
        {
            var regexViewModel = new RegexWindowViewModel
            {
                RegexString = "",
                TitleText = "Regex Tester",
            };
            var regexWindow = new RegexWindow(regexViewModel, this);
            regexWindow.Show();
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var file = mViewModel.SelectedFiles.FirstOrDefault();
            if (file != null && File.Exists(file.FilePath))
            {
                Process.Start(file.FilePath);
            }
        }

        private void _aboutLink_OnClick(object sender, RoutedEventArgs e)
        {
            var window = new AboutWindow {Owner = this};
            window.ShowDialog();
        }

        private void _optionsLink_OnClick(object sender, RoutedEventArgs e)
        {
            var window = new OptionsWindow {Owner = this};
            window.ShowDialog();
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}

