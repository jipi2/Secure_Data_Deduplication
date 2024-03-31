using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorageApp.Shared.Dto
{
    public class AcceptFileTransferDto
    {
        public string senderEmail { get; set; }
        public string fileName { get; set; }
        public string base64FileKey { get; set; }
        public string base64FileIv { get; set; }
    }
}
