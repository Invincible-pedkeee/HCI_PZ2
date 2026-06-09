using System.ComponentModel;
using System.Windows;
using NetworkService.ViewModel;

namespace NetworkService
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            MainWindowViewModel viewModel = DataContext as MainWindowViewModel;

            if (viewModel != null)
            {
                viewModel.StopServices();
            }
        }
    }
}