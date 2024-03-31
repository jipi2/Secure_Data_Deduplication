using Microsoft.Maui.Controls;
using DesktopApp.ViewModels;
using CryptoLib;

namespace DesktopApp
{
	public partial class SignInPage : ContentPage
	{
		public SignInPage()
		{
			InitializeComponent();
            NavigationPage.SetHasBackButton(this, false);
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SignUpPage());
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            if (BindingContext is SignInViewModel viewModel)
            {
                int login = await viewModel.Login();
                if (login == 1)
                {

                    await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    await DisplayAlert("Error", "Invalid email or password", "OK");
                }
                
                
            }
        }
    }
}