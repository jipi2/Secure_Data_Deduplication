using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileStorageApp.Shared.Dto
{
    public class FileParamsDto
    {
        public string base64EncFile { get; set; }
        public string base64KeyEnc { get; set; }
        public string base64IvEnc { get; set; }
        public string base64TagEnc { get; set; }
        public string encFileName { get; set; }
    }
}
