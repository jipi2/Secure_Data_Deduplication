using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorageApp.Shared.Dto
{
    public class FileTransferDto
    {
        public string senderToken { get; set; }
        public string recieverEmail { get; set; }
        public string fileName { get; set; }
        public string base64EncKey { get; set; }

        public string base64EncIv { get; set; }
    }
}
