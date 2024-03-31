using DesktopApp.Dto;
using DesktopApp.HttpFolder;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace DesktopApp.ViewModels
{
    public class SignInViewModel: INotifyCollectionChanged
    {
        private string _email;
        public string Email
        {
            get { return _email; }
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged();
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task<int> Login()
        {
            Debug.WriteLine($"Username: {_email}, Password: {_password}");
            Debug.WriteLine(Enums.Symbol.token.ToString());
            LoginUser logUser = new LoginUser
            {
                Email = _email,
                password = _password
            };
            
            var httpClient = HttpServiceCustom.GetApiClient();
            var result = await httpClient.PostAsJsonAsync("api/User/login", logUser);
            if (result.IsSuccessStatusCode)
            {
                Response loginResponse = await result.Content.ReadFromJsonAsync<Response>();
                if(loginResponse != null)
                    if (loginResponse.Succes)
                    {
                        string token = loginResponse.AccessToken;
                        if(token != null)
                            await SecureStorage.SetAsync(Enums.Symbol.token.ToString(), token);
                    }
                return 1;

            }
            else
            {
                return 0;
            }

        }
    }
}

