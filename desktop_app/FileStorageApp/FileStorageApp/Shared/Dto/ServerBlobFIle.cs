using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorageApp.Shared.Dto
{
    public class ServerBlobFIle
    {   
        public string FileName { get; set; }
        public string FileKey { get; set; }
        public string EncBase64File { get; set; }
        public string FileIv { get; set; }

    }
}
