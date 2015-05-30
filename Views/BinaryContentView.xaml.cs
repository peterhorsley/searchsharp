using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Be.Windows.Forms;
using SearchSharp.ViewModels;

namespace SearchSharp.Views
{
    /// <summary>
    /// Interaction logic for BinaryContentView.xaml
    /// </summary>
    public partial class BinaryContentView : UserControl
    {
        private BinaryContentViewModel _viewModel;

        public BinaryContentView()
        {
            InitializeComponent();
            DataContextChanged += BinaryContentView_DataContextChanged;
        }

        void BinaryContentView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = (BinaryContentViewModel)DataContext;
            _viewModel.FileChanged += ViewModel_FileChanged;
        }

        void ViewModel_FileChanged(object sender, EventArgs e)
        {
            _hexBox.ByteProvider = _viewModel.ByteProvider;
        }
    }
}
