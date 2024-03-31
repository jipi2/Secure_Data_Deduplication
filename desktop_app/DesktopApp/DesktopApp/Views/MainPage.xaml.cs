using DesktopApp.ViewModels;
using System.Diagnostics;

namespace DesktopApp
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
            ShellContent content = AppShell.Current.FindByName<ShellContent>("SignInShell");
            content.IsVisible = false;
            content = AppShell.Current.FindByName<ShellContent>("SignUpShell");
            content.IsVisible = false;
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            // Call your function here
            if (BindingContext is MainWindowViewModel viewModel)
            {
                viewModel.GetFilesAndNames();
            }
        }
        private void OnCounterClicked(object sender, EventArgs e)
        {

        }

        private void OnDownloadClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string fileName)
            {
                var viewModel = BindingContext as MainWindowViewModel;
                viewModel.DownloadFile(fileName);
            }
        }
        private void OnSendClicked(object sender, EventArgs e)
        {

        }
        private void OnDeleteClicked(object sender, EventArgs e)
        {

        }
    }

}
