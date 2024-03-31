using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorageApp.Shared.Dto
{
    public class FileEncDataDto
    {
        public string userEmail { get; set; }
        public string base64KeyEnc { get; set; }
        public string base64IvEnc { get; set; }
        public string encFileName { get; set; }
        public string encBase64Tag { get; set; }
    }
}
