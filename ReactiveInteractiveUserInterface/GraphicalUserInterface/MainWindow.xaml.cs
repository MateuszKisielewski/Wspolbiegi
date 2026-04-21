using System.Windows;

namespace TP.ConcurrentProgramming.PresentationView
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        protected override void OnClosed(System.EventArgs e)
        {
            if (DataContext is TP.ConcurrentProgramming.Presentation.ViewModel.MainWindowViewModel viewModel)
                viewModel.Dispose();
            base.OnClosed(e);
        }
    }
}