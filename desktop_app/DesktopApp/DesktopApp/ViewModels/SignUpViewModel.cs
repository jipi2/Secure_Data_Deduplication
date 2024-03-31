using DesktopApp.Dto;
using DesktopApp.HttpFolder;
using FileStorageApp.Client;
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

namespace DesktopApp.ViewModels
{
    public class SignUpViewModel :INotifyCollectionChanged
    {
        private CryptoService _cryptoService;

        public SignUpViewModel()
        {
            _cryptoService = new CryptoService();
        }

        private string _lastName;
        public string LastName
        {
            get { return _lastName; }
            set
            {
                if (_lastName != value)
                {
                    _lastName = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _firstName;
        public string FirstName
        {
            get { return _firstName; }
            set
            {
                if (_firstName != value)
                {
                    _firstName = value;
                    OnPropertyChanged();
                }
            }
        }

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

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get { return _confirmPassword; }
            set
            {
                if (_confirmPassword != value)
                {
                    _confirmPassword = value;
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

        private async Task<RsaDto?> ComputeRSAKeysForUser()
        {
            RsaDto? rsaDto = await _cryptoService.GetRsaDto(_password);
            return rsaDto;
        }
        public async Task<bool> Register()
        {
            if(_password != _confirmPassword)
            {
                throw new Exception("Passwords do not match");
            }
            RsaDto? rsaDto = await ComputeRSAKeysForUser();
            if (rsaDto == null)
            {
                return false;
            }
            RegisterUser regUser = new RegisterUser
            {
                LastName = _lastName,
                FirstName = _firstName,
                Email = _email,
                Password = _password,
                ConfirmPassword = _confirmPassword,
                rsaKeys = rsaDto

            };
            var httpClient = HttpServiceCustom.GetApiClient();
            var result = await httpClient.PostAsJsonAsync("api/User/register", regUser);
            if (result.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                var errorContent = await result.Content.ReadAsStringAsync();
                throw new Exception(errorContent);
            }

            return false;
        }
    }
}
