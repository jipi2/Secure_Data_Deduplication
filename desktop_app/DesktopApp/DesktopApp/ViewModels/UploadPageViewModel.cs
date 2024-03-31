using CryptoLib;
using DesktopApp.Dto;
using DesktopApp.HttpFolder;
using DesktopApp.Models;
using FileStorageApp.Client;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DesktopApp.ViewModels
{
    public class UploadPageViewModel : INotifyPropertyChanged
    {
        private Task<byte[]> _computeHashTask;

        private FileModel _fileModel = new FileModel();

        private CryptoService _cryptoService = new CryptoService();

        private string _fileNamePicked = "No file selected";
        
        public string FileNamePicked
        {
            get { return _fileNamePicked; }
            set
            {
                if (_fileNamePicked != value)
                {
                    _fileNamePicked = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task SelectFile()
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Please select a file"
            });

            if (result == null)
                throw new Exception("You canceled the uploading process");

            FileNamePicked = result.FileName;
            Debug.WriteLine($"File name: {_fileNamePicked}");
            _fileModel.filePath = result.FullPath;
            _fileModel.fileName = result.FileName;
            _fileModel.encFileName = result.FileName + "_enc_";

            _fileModel.fileStream = new FileStream(result.FullPath, FileMode.Open);

            _computeHashTask = Task.Run(async () => await _cryptoService.GetHashOfFile(_fileModel.fileStream));
            //_computeHashTask.Start();
        }

        public async Task EncryptFile()
        {
            byte[] hash = await _computeHashTask;
            _fileModel.fileStream.Close();

            _fileModel.hash = hash;

            byte[] fileKey = await _cryptoService.ExtractKey(_fileModel.hash, Defines.CryptoDefines.AES_GCM_KEY_SIZE);
            byte[] fileIv = await _cryptoService.ExtractIv(_fileModel.hash,Defines.CryptoDefines.AES_GCM_KEY_SIZE, Defines.CryptoDefines.AES_GCM_IV_SIZE);
            _fileModel.key = fileKey;
            _fileModel.iv = fileIv;

            Debug.WriteLine(Utils.ByteToHex(_fileModel.key));

            _fileModel.fileStream = new FileStream(_fileModel.filePath, FileMode.Open);
        
            _fileModel.base64Tag = Convert.ToBase64String( await _cryptoService.EncryptFileStream(_fileModel.fileStream, _fileModel.key, _fileModel.iv, _fileModel.encFileName));
            _fileModel.fileStream.Close();
        }


        public async Task UploadFile()
        {
            if (_fileModel.key == null || _fileModel.iv == null || _fileModel.base64Tag == null 
                || _fileModel.fileName == null || _fileModel.encFileName == null)
                throw new Exception("An error has occured, please try again!");

            string jwt = await SecureStorage.GetAsync(Enums.Symbol.token.ToString());
            var httpClient = HttpServiceCustom.GetProxyClient();
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);

            FileParamsDto fileDto = new FileParamsDto
            {
                base64Key = Convert.ToBase64String(_fileModel.key),
                base64Iv = Convert.ToBase64String(_fileModel.iv),
                base64Tag = _fileModel.base64Tag,
                fileName = _fileModel.fileName
            };

            string encFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _fileModel.encFileName);
            
            var multipartContent = new MultipartFormDataContent();
           
            var fileStream = File.OpenRead(encFilePath);
            multipartContent.Add(new StringContent(fileDto.fileName), "fileName");
            multipartContent.Add(new StringContent(fileDto.base64Key), "base64Key");
            multipartContent.Add(new StringContent(fileDto.base64Iv), "base64Iv");
            multipartContent.Add(new StringContent(fileDto.base64Tag), "base64Tag");
            multipartContent.Add(new StreamContent(fileStream), "file", fileDto.fileName);
            

            var response = await httpClient.PostAsync("uploadFile", multipartContent);
            if (!response.IsSuccessStatusCode)
            {
                string errorPhrase = await response.Content.ReadAsStringAsync();
                if (errorPhrase.Equals("{\"detail\":\"User already has this file\"}"))
                    throw new Exception("You already have this file in your storage");
                throw new Exception("An error has occured, while sending the file, please try again");
            }
            
            File.Delete(encFilePath);
        }

    }
}
