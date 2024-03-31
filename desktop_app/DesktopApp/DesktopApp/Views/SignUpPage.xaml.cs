using DesktopApp.ViewModels;
using System.Diagnostics;

namespace DesktopApp
{

	public partial class SignUpPage : ContentPage
	{
		public SignUpPage()
		{
			InitializeComponent();
		}

        private async void registerButton_Clicked(object sender, EventArgs e)
        {
			if (BindingContext is SignUpViewModel viewModel)
			{
				try
				{
					bool registered = await viewModel.Register();
					if (registered)
					{
						await DisplayAlert(Enums.Symbol.Success.ToString(), "User registered successfully", "OK");
						await Navigation.PopAsync();
					}
				}
                catch (Exception ex)
				{
                    await DisplayAlert(Enums.Symbol.Error.ToString(), ex.Message, "OK");
                }

			}
			else
				await DisplayAlert(Enums.Symbol.Error.ToString(), "Error registering user", "OK");
        }
    }
}