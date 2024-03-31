using CryptoLib;
using DesktopApp.Dto;
using DesktopApp.HttpFolder;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class MainWindowViewModel :INotifyPropertyChanged
    {
        public class File
        {
            public string fileName { get; set; }
            public string uploadDate { get; set; }
        }

        public MainWindowViewModel()
        {
            _files = new ObservableCollection<File>();
        }

        private ObservableCollection<File> _files ;
        public ObservableCollection<File> Files
        {
            get { return _files; }
            set
            {
                if (_files != value)
                {
                    _files = value;
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

        public void Test()
        {
            File f1 = new File
            {
                fileName = "fileushabfgjhiabdjhfkbaskjhfbasjbfdkjhasbdjkfbaskjbdfbasjldfbjkahsbdfkjhbasdkjhhfbkajbdf1",
                uploadDate = "2021-10-10"
            };
            File f2 = new File
            {
                fileName = "file2",
                uploadDate = "2021-10-11"
            };
            _files.Add(f1);
            _files.Add(f2);
        }

        public async Task GetFilesAndNames()
        {
            string jwt = await SecureStorage.GetAsync(Enums.Symbol.token.ToString());
            List<FilesNameDate>? userFileNames;

            if (jwt == null)
            {
                throw new Exception("Your session expired!");
            }
            var httpClient = HttpServiceCustom.GetProxyClient();
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);

            userFileNames = await httpClient.GetFromJsonAsync<List<FilesNameDate>?>("getFileNamesAndDates");
            _files.Clear();
            if (userFileNames != null)
            {
                foreach (var file in userFileNames)
                {
                    File f = new File
                    {
                        fileName = file.FileName,
                        uploadDate = file.UploadDate.ToString()
                    };
                    _files.Add(f);
                }
            }

        }

        public async Task DownloadFile(string fileName) //ceva nu merge cu stersul fisiereulor
        {
            string jwt = await SecureStorage.GetAsync(Enums.Symbol.token.ToString());
            var httpClient = HttpServiceCustom.GetProxyClient();
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);

            var response = await httpClient.GetStreamAsync("getFileFromStorage/?filename=" + fileName);
            string saveFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName + "_enc");
            Debug.WriteLine(saveFilePath);
            if (response != null && response.CanRead)
            {
                using (FileStream fs = new FileStream(saveFilePath, FileMode.CreateNew))
                {
                    await response.CopyToAsync(fs);
                }

                Debug.WriteLine("File downloaded successfully");
            }


            httpClient = HttpServiceCustom.GetProxyClient();
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwt);
            FileKeyAndIvDto? keyAndIvDto = await httpClient.GetFromJsonAsync<FileKeyAndIvDto>("getKeyAndIvForFile/?filename=" + fileName);

            Debug.WriteLine(keyAndIvDto.base64key);
            Debug.WriteLine(keyAndIvDto.base64iv);

            string downloadFolder = Environment.GetEnvironmentVariable("USERPROFILE") + @"\" + "Downloads";
            string filePath = Path.Combine(downloadFolder, fileName);
            var encFileStream = System.IO.File.OpenRead(saveFilePath);

            Utils.DecryptAndSaveFileWithAesGCM(encFileStream, filePath, Convert.FromBase64String(keyAndIvDto.base64key), Convert.FromBase64String(keyAndIvDto.base64iv));
            System.IO.File.Delete(saveFilePath);
        }

        public async Task test() 
        {
            string download = Environment.GetEnvironmentVariable("USERPROFILE") + @"\" + "Downloads";
            Debug.WriteLine(download);
        }
    }
}
